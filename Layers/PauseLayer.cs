using GrayHare.GameEngine;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Extensions;
using GrayHare.GameEngine.Input;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Snake.Layers;

/// <summary>Overlay layer that toggles pause state and renders a pause indicator.</summary>
internal sealed class PauseLayer : ISceneLayer
{
    private const string ActionPause = "Pause";

    private readonly Font _font;
    private readonly GameOverLayer _gameOverLayer;

    /// <inheritdoc/>
    public int RenderOrder => int.MaxValue;

    /// <summary>Creates a new pause layer.</summary>
    /// <param name="font">The font used for rendering text.</param>
    /// <param name="gameOverLayer">Game-over layer used to prevent pausing during game over.</param>
    public PauseLayer(Font font, GameOverLayer gameOverLayer)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _gameOverLayer = gameOverLayer ?? throw new ArgumentNullException(nameof(gameOverLayer));
    }

    /// <inheritdoc/>
    public void Load(GameHost host)
    {
    }

    /// <inheritdoc/>
    public void Unload(GameHost host)
    {
    }

    /// <inheritdoc/>
    public void Update(GameHost host, in GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(host);

        if (_gameOverLayer.IsGameOver)
        {
            return;
        }

        InputActionMap? map = host.InputActions;

        if (map is not null && map.WasActionPressed(ActionPause, host.Input))
        {
            if (host.IsPaused)
            {
                host.Resume();
            }
            else
            {
                host.Pause();
            }
        }
    }

    /// <inheritdoc/>
    public void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        if (!host.IsPaused)
        {
            return;
        }

        using RectangleShape veil = new(new Vector2f(window.Size.X, window.Size.Y))
        {
            FillColor = new Color(0, 0, 0, 160)
        };

        window.Draw(veil);

        window.DrawCenteredText(_font, 64, Color.White, "PAUSED", (window.Size.Y / 2f) - 60f);
        window.DrawCenteredText(_font, 22, new Color(200, 200, 200), "Press P to resume", window.Size.Y / 2f);
    }
}
