using GrayHare.Snake.Components;

namespace GrayHare.Snake;

/// <summary>Utility methods for working with <see cref="MoveDirection"/> values.</summary>
internal static class DirectionHelper
{
    /// <summary>Returns <see langword="true"/> when two directions are directly opposite (180° reversal).</summary>
    public static bool IsOpposite(MoveDirection a, MoveDirection b) =>
        a.DeltaCol == -b.DeltaCol && a.DeltaRow == -b.DeltaRow;
}
