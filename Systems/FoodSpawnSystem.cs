using GrayHare.GameEngine.Ecs;
using GrayHare.Snake.Components;

namespace GrayHare.Snake.Systems;

/// <summary>Spawns normal and bonus food entities at random empty grid cells.</summary>
internal static class FoodSpawnSystem
{
    private static readonly Random _random = Random.Shared;

    /// <summary>Spawns a normal food entity at a random unoccupied cell.</summary>
    /// <param name="world">The ECS world to create entities in.</param>
    public static void SpawnFood(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        GridPosition pos = FindEmptyCell(world);
        Entity food = world.CreateEntity();
        world.AddComponent(food, pos);
        world.AddComponent(food, new FoodTag());
        world.AddComponent(food, new SpriteColor(
            GameConstants.FoodColor.R,
            GameConstants.FoodColor.G,
            GameConstants.FoodColor.B));
        world.AddComponent(food, new ScoreValue(GameConstants.NormalFoodPoints));
    }

    /// <summary>
    /// Rolls a chance to spawn bonus food. Does nothing if bonus food already exists
    /// or the roll fails.
    /// </summary>
    /// <param name="world">The ECS world to create entities in.</param>
    public static void TrySpawnBonusFood(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        // Only one bonus food at a time
        if (world.ComponentCount<BonusFoodTag>() > 0)
        {
            return;
        }

        if (_random.NextDouble() >= GameConstants.BonusFoodChance)
        {
            return;
        }

        GridPosition pos = FindEmptyCell(world);
        Entity bonus = world.CreateEntity();
        world.AddComponent(bonus, pos);
        world.AddComponent(bonus, new BonusFoodTag(GameConstants.BonusFoodDuration));
        world.AddComponent(bonus, new SpriteColor(
            GameConstants.BonusFoodColor.R,
            GameConstants.BonusFoodColor.G,
            GameConstants.BonusFoodColor.B));
        world.AddComponent(bonus, new ScoreValue(GameConstants.BonusFoodPoints));
    }

    /// <summary>Finds a random cell that is not occupied by any entity with a <see cref="GridPosition"/>.</summary>
    private static GridPosition FindEmptyCell(World world)
    {
        HashSet<(int Col, int Row)> occupied = [];
        world.ForEach<GridPosition>(static (_, pos) => { });

        // Collect occupied cells
        foreach (Entity entity in world.Query<GridPosition>())
        {
            GridPosition gp = world.GetComponent<GridPosition>(entity);
            occupied.Add((gp.Col, gp.Row));
        }

        int col;
        int row;
        do
        {
            col = _random.Next(0, GameConstants.GridCols);
            row = _random.Next(0, GameConstants.GridRows);
        }
        while (occupied.Contains((col, row)));

        return new GridPosition(col, row);
    }
}
