using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Ecs;
using GrayHare.GameEngine.Input;
using GrayHare.GameEngine.Scenes;
using GrayHare.Snake.Components;
using GrayHare.Snake.Layers;
using GrayHare.Snake.Systems;
using SFML.Graphics;
using SFML.Window;

namespace GrayHare.Snake.Scenes;

/// <summary>Main gameplay scene that runs the snake game using ECS systems.</summary>
internal sealed class GameplayScene : GameSceneBase
{
    private const string ActionMoveUp = "MoveUp";
    private const string ActionMoveDown = "MoveDown";
    private const string ActionMoveLeft = "MoveLeft";
    private const string ActionMoveRight = "MoveRight";
    private const string ActionPause = "Pause";
    private const string ActionJoyX = "JoyX";
    private const string ActionJoyY = "JoyY";

    private readonly AssetsManifest _assets;
    private readonly Font _font;

    private MoveDirection _currentDirection = MoveDirection.Right;
    private MoveDirection _bufferedDirection = MoveDirection.Right;
    private float _tickAccumulator;
    private bool _gameOver;

    private RenderSystem? _renderSystem;
    private readonly ScoreSystem _scoreSystem = new();
    private int _snakeLength = 3;
    private PauseLayer? _pauseLayer;
    private GameOverLayer? _gameOverLayer;

    /// <summary>Creates a new gameplay scene.</summary>
    /// <param name="assets">Asset manifest containing sound file paths.</param>
    /// <param name="font">The font used for HUD text rendering.</param>
    public GameplayScene(AssetsManifest assets, Font font)
    {
        _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        _font = font ?? throw new ArgumentNullException(nameof(font));
    }

    /// <inheritdoc/>
    public override void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        // Set up input action map
        InputActionMap map = new();
        map.MapKey(ActionMoveUp, Keyboard.Key.Up);
        map.MapKey(ActionMoveUp, Keyboard.Key.W);
        map.MapKey(ActionMoveDown, Keyboard.Key.Down);
        map.MapKey(ActionMoveDown, Keyboard.Key.S);
        map.MapKey(ActionMoveLeft, Keyboard.Key.Left);
        map.MapKey(ActionMoveLeft, Keyboard.Key.A);
        map.MapKey(ActionMoveRight, Keyboard.Key.Right);
        map.MapKey(ActionMoveRight, Keyboard.Key.D);
        map.MapKey(ActionPause, Keyboard.Key.P);
        map.MapButton(ActionPause, 0, 7);
        map.MapAxis(ActionJoyX, 0, Joystick.Axis.PovX);
        map.MapAxis(ActionJoyY, 0, Joystick.Axis.PovY);

        host.InputActions = map;

        // Initialize systems
        _renderSystem = new RenderSystem();

        // Add HUD layer
        AddLayer(new HudLayer(_font, () => _scoreSystem.Score, () => _snakeLength));

        // Add pause and game-over layers
        _gameOverLayer = new GameOverLayer(_font);
        _gameOverLayer.GameOverHandled += () =>
        {
            host.ChangeScene(new WelcomeScene(_assets));
        };
        AddLayer(_gameOverLayer);

        _pauseLayer = new PauseLayer(_font, _gameOverLayer);
        AddLayer(_pauseLayer);

        // Spawn initial snake(3 segments heading right)
        World world = host.World;
        int midCol = GameConstants.GridCols / 2;
        int midRow = GameConstants.GridRows / 2;

        SpawnSegment(world, midCol, midRow, 0, true);
        SpawnSegment(world, midCol - 1, midRow, 1, false);
        SpawnSegment(world, midCol - 2, midRow, 2, false);

        // Spawn initial food
        FoodSpawnSystem.SpawnFood(world);

        base.Load(host);
    }

    /// <inheritdoc/>
    public override void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        host.InputActions = null;
        _renderSystem?.Dispose();
        _renderSystem = null;

        base.Unload(host);
    }

    /// <inheritdoc/>
    public override void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        base.Update(host, in gameTime);

        if (_gameOver || host.IsPaused)
        {
            return;
        }

        float deltaTime = gameTime.DeltaTotalSeconds;
        InputActionMap map = host.InputActions!;

        // Buffer direction input (anti-reversal: can't go 180°)
        MoveDirection? requested = null;
        if (map.WasActionPressed(ActionMoveUp, host.Input))
        {
            requested = MoveDirection.Up;
        }
        else if (map.WasActionPressed(ActionMoveDown, host.Input))
        {
            requested = MoveDirection.Down;
        }
        else if (map.WasActionPressed(ActionMoveLeft, host.Input))
        {
            requested = MoveDirection.Left;
        }
        else if (map.WasActionPressed(ActionMoveRight, host.Input))
        {
            requested = MoveDirection.Right;
        }

        // Joystick axis fallback for direction (threshold-based digital input)
        if (requested is null)
        {
            float joyX = map.GetAxisValue(ActionJoyX, host.Input);
            float joyY = map.GetAxisValue(ActionJoyY, host.Input);

            if (MathF.Abs(joyY) > 50f && MathF.Abs(joyY) > MathF.Abs(joyX))
            {
                requested = joyY > 0 ? MoveDirection.Up : MoveDirection.Down;
            }
            else if (MathF.Abs(joyX) > 50f)
            {
                requested = joyX < 0 ? MoveDirection.Left : MoveDirection.Right;
            }
        }

        if (requested is not null && !DirectionHelper.IsOpposite(requested.Value, _currentDirection))
        {
            _bufferedDirection = requested.Value;
        }

        // Per-frame systems
        BonusFoodTimerSystem.Update(host.World, deltaTime);

        // Tick accumulator
        _tickAccumulator += deltaTime;
        if (_tickAccumulator >= GameConstants.TickInterval)
        {
            _tickAccumulator -= GameConstants.TickInterval;
            ExecuteTick(host);
        }
    }

    /// <summary>Advances the game by one grid step.</summary>
    private void ExecuteTick(GameHost host)
    {
        _currentDirection = _bufferedDirection;
        World world = host.World;

        // Find current head to compute next position
        int newCol = 0;
        int newRow = 0;
        foreach (Entity entity in world.Query<SnakeSegment, GridPosition>())
        {
            SnakeSegment seg = world.GetComponent<SnakeSegment>(entity);
            if (seg.Order == 0)
            {
                GridPosition headPos = world.GetComponent<GridPosition>(entity);
                newCol = headPos.Col + _currentDirection.DeltaCol;
                newRow = headPos.Row + _currentDirection.DeltaRow;
                break;
            }
        }

        // Check collisions before moving
        CollisionResult result = CollisionSystem.Check(world, newCol, newRow, out Entity collidedEntity);

        if (result is CollisionResult.Wall or CollisionResult.Self)
        {
            host.Audio.PlaySound(_assets.DeathSoundPath);
            HighScoreStore.Save(_scoreSystem.Score);
            _gameOver = true;
            _gameOverLayer!.Score = _scoreSystem.Score;
            _gameOverLayer.IsGameOver = true;

            return;
        }

        bool eating = result is CollisionResult.Food or CollisionResult.BonusFood;

        // Execute movement
        SnakeMovementSystem.Tick(world, _currentDirection, eating);

        if (eating)
        {
            // Get score value before destroying the food entity
            int points = world.TryGetComponent(collidedEntity, out ScoreValue sv)
                ? sv.Points
                : GameConstants.NormalFoodPoints;

            world.DestroyEntity(collidedEntity);
            _scoreSystem.Add(points);
            _snakeLength++;

            if (result is CollisionResult.Food)
            {
                host.Audio.PlaySound(_assets.EatSoundPath);
                FoodSpawnSystem.SpawnFood(world);
                FoodSpawnSystem.TrySpawnBonusFood(world);
            }
            else
            {
                host.Audio.PlaySound(_assets.BonusEatSoundPath);
            }
        }
    }

    /// <inheritdoc/>
    public override void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        _renderSystem?.Draw(window, host.World, (float)DateTime.UtcNow.TimeOfDay.TotalSeconds);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _renderSystem?.Dispose();
            _renderSystem = null;
        }

        base.Dispose(disposing);
    }

    /// <summary>Creates a snake segment entity at the given grid position.</summary>
    private static void SpawnSegment(World world, int col, int row, int order, bool isHead)
    {
        Entity entity = world.CreateEntity();
        world.AddComponent(entity, new GridPosition(col, row));
        world.AddComponent(entity, new SnakeSegment(order));

        Color color = isHead ? GameConstants.SnakeHeadColor : GameConstants.SnakeBodyColor;
        world.AddComponent(entity, new SpriteColor(color.R, color.G, color.B));
    }
}
