namespace GrayHare.Snake.Components;

/// <summary>RGB color used when rendering an entity on the grid.</summary>
/// <param name="R">Red channel (0–255).</param>
/// <param name="G">Green channel (0–255).</param>
/// <param name="B">Blue channel (0–255).</param>
internal readonly record struct SpriteColor(byte R, byte G, byte B);
