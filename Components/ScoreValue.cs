namespace GrayHare.Snake.Components;

/// <summary>Points awarded when this entity is consumed by the snake.</summary>
/// <param name="Points">Number of points to award.</param>
internal readonly record struct ScoreValue(int Points);
