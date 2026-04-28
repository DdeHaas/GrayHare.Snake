using GrayHare.GameEngine.Ecs;
using GrayHare.Snake.Components;

namespace GrayHare.Snake.Systems;

/// <summary>Decrements the remaining time on bonus food entities and destroys them when expired.</summary>
internal static class BonusFoodTimerSystem
{
    /// <summary>Updates all bonus food timers and removes expired ones.</summary>
    /// <param name="world">The ECS world containing bonus food entities.</param>
    /// <param name="deltaTime">Frame delta time in seconds.</param>
    public static void Update(World world, float deltaTime)
    {
        ArgumentNullException.ThrowIfNull(world);

        world.ForEach<BonusFoodTag>((entity, tag) =>
        {
            float remaining = tag.RemainingSeconds - deltaTime;
            if (remaining <= 0f)
            {
                world.DestroyEntity(entity);
            }
            else
            {
                world.AddComponent(entity, new BonusFoodTag(remaining));
            }
        });
    }
}
