using GrayHare.GameEngine.Ecs;
using GrayHare.Snake.Components;

namespace GrayHare.Snake.Systems;

/// <summary>
/// Advances the snake by one grid step. Creates a new head entity, shifts segment
/// orders, and optionally removes the tail when the snake is not growing.
/// </summary>
internal static class SnakeMovementSystem
{
    /// <summary>
    /// Executes one movement tick.
    /// </summary>
    /// <param name="world">The ECS world containing snake entities.</param>
    /// <param name="direction">The direction to move this tick.</param>
    /// <param name="grow">When <see langword="true"/>, the tail is preserved (snake grows).</param>
    /// <returns>The new head <see cref="GridPosition"/> after the move.</returns>
    public static GridPosition Tick(World world, MoveDirection direction, bool grow)
    {
        ArgumentNullException.ThrowIfNull(world);

        // Find the current head entity (SnakeSegment.Order == 0)
        Entity headEntity = default;
        GridPosition headPos = default;
        bool foundHead = false;

        foreach (Entity entity in world.Query<SnakeSegment, GridPosition>())
        {
            SnakeSegment seg = world.GetComponent<SnakeSegment>(entity);
            if (seg.Order == 0)
            {
                headEntity = entity;
                headPos = world.GetComponent<GridPosition>(entity);
                foundHead = true;
                break;
            }
        }

        if (!foundHead)
        {
            return default;
        }

        // Compute new head position
        GridPosition newHeadPos = new(headPos.Col + direction.DeltaCol, headPos.Row + direction.DeltaRow);

        // Increment all existing segment orders by 1
        int maxOrder = 0;
        Entity tailEntity = default;

        foreach (Entity entity in world.Query<SnakeSegment>())
        {
            SnakeSegment seg = world.GetComponent<SnakeSegment>(entity);
            int newOrder = seg.Order + 1;
            world.AddComponent(entity, new SnakeSegment(newOrder));

            if (newOrder > maxOrder)
            {
                maxOrder = newOrder;
                tailEntity = entity;
            }
        }

        // Demote old head color to body color
        world.AddComponent(headEntity, new SpriteColor(
            GameConstants.SnakeBodyColor.R,
            GameConstants.SnakeBodyColor.G,
            GameConstants.SnakeBodyColor.B));

        // Create new head entity at the new position
        Entity newHead = world.CreateEntity();
        world.AddComponent(newHead, newHeadPos);
        world.AddComponent(newHead, new SnakeSegment(0));
        world.AddComponent(newHead, new SpriteColor(
            GameConstants.SnakeHeadColor.R,
            GameConstants.SnakeHeadColor.G,
            GameConstants.SnakeHeadColor.B));

        // If not growing, destroy the tail entity
        if (!grow)
        {
            world.DestroyEntity(tailEntity);
        }

        return newHeadPos;
    }
}
