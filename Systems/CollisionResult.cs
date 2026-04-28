namespace GrayHare.Snake.Systems;

/// <summary>Possible outcomes when the snake head moves to a new cell.</summary>
internal enum CollisionResult
{
    /// <summary>No collision; the cell is empty.</summary>
    None,

    /// <summary>Head hit a grid boundary.</summary>
    Wall,

    /// <summary>Head hit its own body.</summary>
    Self,

    /// <summary>Head landed on normal food.</summary>
    Food,

    /// <summary>Head landed on bonus food.</summary>
    BonusFood
}
