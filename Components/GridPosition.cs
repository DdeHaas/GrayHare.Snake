namespace GrayHare.Snake.Components;

/// <summary>Cell coordinate on the game grid.</summary>
/// <param name="Col">Zero-based column index.</param>
/// <param name="Row">Zero-based row index.</param>
internal readonly record struct GridPosition(int Col, int Row);
