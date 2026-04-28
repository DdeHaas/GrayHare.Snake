using GrayHare.GameEngine.Ecs;
using GrayHare.Snake.Components;

namespace GrayHare.Snake.Systems;

/// <summary>
/// Checks the prospective head position against grid boundaries, the snake body, and food entities.
/// </summary>
internal static class CollisionSystem
{
    /// <summary>Evaluates collisions for the given head position.</summary>
    /// <param name="world">The ECS world containing all entities.</param>
    /// <param name="headCol">Target column of the snake head.</param>
    /// <param name="headRow">Target row of the snake head.</param>
    /// <param name="collidedEntity">The food or bonus-food entity involved in the collision, if any.</param>
    public static CollisionResult Check(World world, int headCol, int headRow, out Entity collidedEntity)
    {
        ArgumentNullException.ThrowIfNull(world);

        collidedEntity = default;

        // Boundary check
        if (headCol < 0 || headCol >= GameConstants.GridCols ||
            headRow < 0 || headRow >= GameConstants.GridRows)
        {
            return CollisionResult.Wall;
        }

        // Self-collision: any snake segment at the target cell
        foreach (Entity entity in world.Query<SnakeSegment, GridPosition>())
        {
            GridPosition pos = world.GetComponent<GridPosition>(entity);
            if (pos.Col == headCol && pos.Row == headRow)
            {
                return CollisionResult.Self;
            }
        }

        // Normal food collision
        foreach (Entity entity in world.Query<FoodTag, GridPosition>())
        {
            GridPosition pos = world.GetComponent<GridPosition>(entity);
            if (pos.Col == headCol && pos.Row == headRow)
            {
                collidedEntity = entity;

                return CollisionResult.Food;
            }
        }

        // Bonus food collision
        foreach (Entity entity in world.Query<BonusFoodTag, GridPosition>())
        {
            GridPosition pos = world.GetComponent<GridPosition>(entity);
            if (pos.Col == headCol && pos.Row == headRow)
            {
                collidedEntity = entity;

                return CollisionResult.BonusFood;
            }
        }

        return CollisionResult.None;
    }
}
