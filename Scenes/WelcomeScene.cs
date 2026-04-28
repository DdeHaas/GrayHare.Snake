using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Extensions;
using GrayHare.GameEngine.Scenes;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GrayHare.Snake.Scenes;

/// <summary>Title screen with high score display, blinking prompt, and a grid-based demo snake.</summary>
internal sealed class WelcomeScene : GameSceneBase
{
    private const int SnakeLength = 10;
    private const int DemoCellSize = 16;
    private const int DemoCols = 32;
    private const int DemoRows = 12;
    private const float GridOriginX = 64f;
    private const float GridOriginY = 440f;
    private const float TickInterval = 0.12f;
    private const int TurnCooldown = 3;
    private const float BlinkInterval = 0.55f;

    private readonly AssetsManifest _assets;
    private readonly List<(int Col, int Row)> _segments = [];
    private readonly Random _random = Random.Shared;

    private Font? _font;
    private float _blinkTimer;
    private bool _showPrompt = true;
    private float _tickAccumulator;
    private int _dirCol = 1;
    private int _dirRow;
    private int _stepsSinceTurn;

    /// <summary>Creates a new welcome scene.</summary>
    /// <param name="assets">Asset manifest containing sound file paths.</param>
    public WelcomeScene(AssetsManifest assets)
    {
        _assets = assets ?? throw new ArgumentNullException(nameof(assets));
    }

    /// <inheritdoc/>
    public override void Load(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        // Prewarm sound buffers to avoid first-frame disk I/O stutter during gameplay.
        host.Audio.PrewarmSound(_assets.EatSoundPath);
        host.Audio.PrewarmSound(_assets.BonusEatSoundPath);
        host.Audio.PrewarmSound(_assets.DeathSoundPath);

        _font = host.Assets.LoadFont(GameConstants.FontName);
        _tickAccumulator = 0f;
        _dirCol = 1;
        _dirRow = 0;
        _stepsSinceTurn = 0;

        int startCol = DemoCols / 2;
        int startRow = DemoRows / 2;
        _segments.Clear();
        for (int i = 0; i < SnakeLength; i++)
        {
            _segments.Add((startCol - i, startRow));
        }

        base.Load(host);
    }

    /// <inheritdoc/>
    public override void Unload(GameHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        base.Unload(host);
        _font = null;
    }

    /// <inheritdoc/>
    public override void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        base.Update(host, in gameTime);

        if (host.Input.WasKeyPressed(Keyboard.Key.Escape))
        {
            host.Exit();

            return;
        }

        if (host.Input.WasAnyKeyPressed())
        {
            host.ChangeScene(new GameplayScene(_assets, _font!));

            return;
        }

        // Check gamepad button 0 for start.
        if (host.Input.IsJoystickConnected(0) && host.Input.WasJoystickButtonPressed(0, 0))
        {
            host.ChangeScene(new GameplayScene(_assets, _font!));

            return;
        }

        float deltaTime = gameTime.DeltaTotalSeconds;

        _blinkTimer += deltaTime;
        if (_blinkTimer >= BlinkInterval)
        {
            _blinkTimer -= BlinkInterval;
            _showPrompt = !_showPrompt;
        }

        _tickAccumulator += deltaTime;
        while (_tickAccumulator >= TickInterval)
        {
            _tickAccumulator -= TickInterval;
            TickDemoSnake();
        }
    }

    /// <inheritdoc/>
    public override void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        if (_font is null)
        {
            return;
        }

        window.DrawCenteredText(_font, 72, new Color(60, 200, 60), "SNAKE", 150f);
        window.DrawCenteredText(_font, 18, new Color(0, 180, 0), new string('=', 48), 210f);

        int highScore = HighScoreStore.Load();
        if (highScore > 0)
        {
            window.DrawCenteredText(_font, 26, Color.White, $"HIGH SCORE: {highScore:D5}", 280f);
        }

        if (_showPrompt)
        {
            window.DrawCenteredText(_font, 28, Color.Yellow, "PRESS ANY KEY OR FIRE BUTTON TO START", 390f);
        }

        RenderDemoSnake(window);

        window.DrawCenteredText(_font, 14, new Color(100, 100, 100), "Powered by GrayHare GameEngine", 620f);
    }

    /// <summary>Advances the demo snake by one grid cell with 90-degree turns.</summary>
    private void TickDemoSnake()
    {
        if (_segments.Count == 0)
        {
            return;
        }

        _stepsSinceTurn++;
        (int headCol, int headRow) = _segments[0];
        int nextCol = headCol + _dirCol;
        int nextRow = headRow + _dirRow;

        bool mustTurn = nextCol < 0 || nextCol >= DemoCols || nextRow < 0 || nextRow >= DemoRows;
        bool wantTurn = !mustTurn && _stepsSinceTurn >= TurnCooldown && _random.Next(100) < 25;

        if (mustTurn || wantTurn)
        {
            PickPerpendicularDirection(headCol, headRow);
            nextCol = headCol + _dirCol;
            nextRow = headRow + _dirRow;
        }

        _segments.RemoveAt(_segments.Count - 1);
        _segments.Insert(0, (nextCol, nextRow));
    }

    /// <summary>Picks a perpendicular direction (90-degree turn) that stays inside the grid.</summary>
    private void PickPerpendicularDirection(int headCol, int headRow)
    {
        // Rotate 90 degrees left: (dc, dr) -> (dr, -dc)
        (int c1, int r1) = (_dirRow, -_dirCol);
        // Rotate 90 degrees right: (dc, dr) -> (-dr, dc)
        (int c2, int r2) = (-_dirRow, _dirCol);

        bool valid1 = IsInsideGrid(headCol + c1, headRow + r1);
        bool valid2 = IsInsideGrid(headCol + c2, headRow + r2);

        if (valid1 && valid2)
        {
            bool pickFirst = _random.Next(2) == 0;
            _dirCol = pickFirst ? c1 : c2;
            _dirRow = pickFirst ? r1 : r2;
        }
        else if (valid1)
        {
            _dirCol = c1;
            _dirRow = r1;
        }
        else if (valid2)
        {
            _dirCol = c2;
            _dirRow = r2;
        }

        _stepsSinceTurn = 0;
    }

    private static bool IsInsideGrid(int col, int row)
    {
        return col >= 0 && col < DemoCols && row >= 0 && row < DemoRows;
    }

    /// <summary>Renders each segment as a grid-aligned square.</summary>
    private void RenderDemoSnake(RenderWindow window)
    {
        for (int i = _segments.Count - 1; i >= 0; i--)
        {
            (int col, int row) = _segments[i];
            bool isHead = i == 0;
            Color color = isHead ? GameConstants.SnakeHeadColor : GameConstants.SnakeBodyColor;
            float inset = isHead ? 0f : DemoCellSize * 0.1f;

            float x = GridOriginX + (col * DemoCellSize) + inset;
            float y = GridOriginY + (row * DemoCellSize) + inset;
            float size = DemoCellSize - 1 - (inset * 2);

            using RectangleShape rect = new(new Vector2f(size, size));
            rect.Position = new Vector2f(x, y);
            rect.FillColor = color;
            window.Draw(rect);
        }
    }
}
