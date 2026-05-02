using GrayHare.GameEngine;
using GrayHare.GameEngine.Application;
using GrayHare.GameEngine.Extensions;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Snake.Layers;

/// <summary>Overlay layer that displays the game-over screen with score and countdown.</summary>
internal sealed class GameOverLayer : ISceneLayer
{
    private readonly Font _font;
    private string _waitTimerText = string.Empty;
    private float _returnTimer;

    private bool _isGameOver;
    private bool _isEnabled;

    /// <summary>Raised when the game-over countdown completes.</summary>
    public event Action? GameOverHandled;

    /// <inheritdoc/>
    public int RenderOrder => int.MaxValue;

    /// <summary>Gets or sets whether the game is over; setting to <see langword="true"/> enables the layer.</summary>
    public bool IsGameOver
    {
        get => _isGameOver;
        set
        {
            _isGameOver = value;
            if (value)
            {
                _isEnabled = true;
            }
        }
    }

    /// <summary>Gets or sets the final score to display.</summary>
    public int Score { get; set; }

    /// <summary>Creates a new game-over layer.</summary>
    /// <param name="font">The font used for rendering text.</param>
    public GameOverLayer(Font font)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
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

        if (!_isEnabled)
        {
            return;
        }

        _returnTimer += gameTime.RawDeltaTotalSeconds;

        _waitTimerText = new string('.', Math.Max(0, GameConstants.GameOverWaitTimer - (int)_returnTimer));
        if (_returnTimer >= GameConstants.GameOverWaitTimer)
        {
            GameOverHandled?.Invoke();
        }
    }

    /// <inheritdoc/>
    public void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        if (!_isEnabled)
        {
            return;
        }

        using RectangleShape overlay = new(new Vector2f(window.Size.X, window.Size.Y));
        overlay.FillColor = new Color(0, 0, 0, 160);
        window.Draw(overlay);

        window.DrawCenteredText(_font, 60, Color.Red, "GAME OVER", 220f);
        window.DrawCenteredText(_font, 30, Color.White, $"SCORE: {Score:D5}", 310f);

        if (Score > 0 && Score >= HighScoreStore.Load())
        {
            window.DrawCenteredText(_font, 30, Color.Yellow, "NEW HIGH SCORE!", 360f);
        }

        window.DrawCenteredText(_font, 40, Color.White, _waitTimerText, 420f);
    }
}
