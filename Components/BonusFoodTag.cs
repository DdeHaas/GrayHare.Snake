namespace GrayHare.Snake.Components;

/// <summary>Marker component for a timed bonus food entity.</summary>
/// <param name="RemainingSeconds">Time in seconds before this bonus food despawns.</param>
internal readonly record struct BonusFoodTag(float RemainingSeconds);
