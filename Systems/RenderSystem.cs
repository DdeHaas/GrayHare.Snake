using GrayHare.GameEngine.Ecs;
using GrayHare.Snake.Components;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Snake.Systems;

/// <summary>Renders the grid, snake segments, food, and bonus food to the window.</summary>
internal sealed class RenderSystem : IDisposable
{
    private readonly VertexArray _gridLines;
    private readonly RectangleShape _cellRect = new();
    private bool _disposed;

    /// <summary>Pre-builds grid line geometry for efficient reuse each frame.</summary>
    public RenderSystem()
    {
        _gridLines = new VertexArray(PrimitiveType.Lines);

        for (int r = 0; r <= GameConstants.GridRows; r++)
        {
            float y = GameConstants.GridOffsetY + (r * GameConstants.CellSize);
            float x1 = GameConstants.GridOffsetX;
            float x2 = x1 + (GameConstants.GridCols * GameConstants.CellSize);
            _gridLines.Append(new Vertex(new Vector2f(x1, y), GameConstants.GridLineColor));
            _gridLines.Append(new Vertex(new Vector2f(x2, y), GameConstants.GridLineColor));
        }

        for (int c = 0; c <= GameConstants.GridCols; c++)
        {
            float x = GameConstants.GridOffsetX + (c * GameConstants.CellSize);
            float y1 = GameConstants.GridOffsetY;
            float y2 = y1 + (GameConstants.GridRows * GameConstants.CellSize);
            _gridLines.Append(new Vertex(new Vector2f(x, y1), GameConstants.GridLineColor));
            _gridLines.Append(new Vertex(new Vector2f(x, y2), GameConstants.GridLineColor));
        }
    }

    /// <summary>Draws all game entities onto the window.</summary>
    /// <param name="window">The SFML render window.</param>
    /// <param name="world">The ECS world containing renderable entities.</param>
    /// <param name="totalSeconds">Total elapsed game time for visual effects.</param>
    public void Draw(RenderWindow window, World world, float totalSeconds)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(world);

        window.Draw(_gridLines);

        // Draw snake segments, food, and bonus food
        world.ForEach<GridPosition, SpriteColor>((entity, pos, color) =>
        {
            Color fillColor = new(color.R, color.G, color.B);

            // Pulsing glow effect for normal food
            if (world.HasComponent<FoodTag>(entity))
            {
                float pulse = 0.7f + (0.3f * MathF.Sin(totalSeconds * 6f));
                fillColor = new Color(
                    (byte)(color.R * pulse),
                    (byte)(color.G * pulse),
                    (byte)(color.B * pulse));
            }

            // Blinking effect for bonus food based on remaining time
            if (world.TryGetComponent(entity, out BonusFoodTag bonusTag))
            {
                // Blink faster as time runs out
                float blinkRate = bonusTag.RemainingSeconds < 3f ? 10f : 4f;
                bool visible = MathF.Sin(totalSeconds * blinkRate) > 0f;
                if (!visible)
                {
                    return;
                }
            }

            float inset = 0f;
            if (world.HasComponent<FoodTag>(entity) || world.HasComponent<BonusFoodTag>(entity))
            {
                inset = GameConstants.CellSize * 0.15f;
            }

            float x = GameConstants.GridOffsetX + (pos.Col * GameConstants.CellSize) + inset;
            float y = GameConstants.GridOffsetY + (pos.Row * GameConstants.CellSize) + inset;
            float size = GameConstants.CellSize - 1 - (inset * 2);

            _cellRect.Position = new Vector2f(x, y);
            _cellRect.Size = new Vector2f(size, size);
            _cellRect.FillColor = fillColor;
            window.Draw(_cellRect);
        });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _gridLines.Dispose();
        _cellRect.Dispose();
    }
}
