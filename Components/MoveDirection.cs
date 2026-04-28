namespace GrayHare.Snake.Components;

/// <summary>Movement delta applied to the snake head each tick.</summary>
/// <param name="DeltaCol">Column offset per step (-1, 0, or 1).</param>
/// <param name="DeltaRow">Row offset per step (-1, 0, or 1).</param>
internal readonly record struct MoveDirection(int DeltaCol, int DeltaRow)
{
    /// <summary>Moving up (row decreases).</summary>
    public static readonly MoveDirection Up = new(0, -1);

    /// <summary>Moving down (row increases).</summary>
    public static readonly MoveDirection Down = new(0, 1);

    /// <summary>Moving left (column decreases).</summary>
    public static readonly MoveDirection Left = new(-1, 0);

    /// <summary>Moving right (column increases).</summary>
    public static readonly MoveDirection Right = new(1, 0);
}
