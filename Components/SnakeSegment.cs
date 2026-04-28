namespace GrayHare.Snake.Components;

/// <summary>Marks an entity as part of the snake body.</summary>
/// <param name="Order">Segment index: 0 = head, 1 = first body segment, and so on.</param>
internal readonly record struct SnakeSegment(int Order);
