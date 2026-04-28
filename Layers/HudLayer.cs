using GrayHare.GameEngine.Abstractions;
using GrayHare.GameEngine.Application;
using SFML.Graphics;
using SFML.System;

namespace GrayHare.Snake.Layers;

/// <summary>Heads-up display layer showing score, high score, and snake length.</summary>
internal sealed class HudLayer : ISceneLayer
{
    private readonly Font _font;
    private readonly Func<int> _getScore;
    private readonly Func<int> _getLength;

    private int _lastScore = -1;
    private int _lastLength = -1;
    private string _scoreText = string.Empty;
    private string _highScoreText = string.Empty;
    private string _lengthText = string.Empty;

    /// <inheritdoc/>
    public int RenderOrder => 10;

    /// <summary>Creates a new HUD layer.</summary>
    /// <param name="font">The font used for rendering text.</param>
    /// <param name="getScore">Callback returning the current score.</param>
    /// <param name="getLength">Callback returning the current snake length.</param>
    public HudLayer(Font font, Func<int> getScore, Func<int> getLength)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _getScore = getScore ?? throw new ArgumentNullException(nameof(getScore));
        _getLength = getLength ?? throw new ArgumentNullException(nameof(getLength));
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

        int score = _getScore();
        int length = _getLength();

        if (score != _lastScore)
        {
            _scoreText = $"SCORE: {score:D5}";
            _lastScore = score;

            int highScore = HighScoreStore.Load();
            _highScoreText = score > highScore
                ? $"HIGH: {score:D5}"
                : $"HIGH: {highScore:D5}";
        }

        if (length != _lastLength)
        {
            _lengthText = $"LENGTH: {length}";
            _lastLength = length;
        }
    }

    /// <inheritdoc/>
    public void RenderLayer(GameHost host, RenderWindow window)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(window);

        float gridRight = GameConstants.GridOffsetX + (GameConstants.GridCols * GameConstants.CellSize);

        // Score (left-aligned)
        using Text scoreText = new(_font, _scoreText, 22);
        scoreText.FillColor = GameConstants.HudColor;
        scoreText.Position = new Vector2f(GameConstants.GridOffsetX, 20f);
        window.Draw(scoreText);

        // High score (right-aligned)
        using Text hiText = new(_font, _highScoreText, 22);
        hiText.FillColor = new Color(200, 200, 100);
        FloatRect hb = hiText.GetLocalBounds();
        hiText.Origin = new Vector2f(hb.Position.X + hb.Size.X, hb.Position.Y);
        hiText.Position = new Vector2f(gridRight, 20f);
        window.Draw(hiText);

        // Snake length (centered)
        using Text lengthText = new(_font, _lengthText, 18);
        lengthText.FillColor = new Color(180, 180, 180);
        FloatRect lb = lengthText.GetLocalBounds();
        lengthText.Origin = new Vector2f(lb.Position.X + (lb.Size.X / 2f), lb.Position.Y);
        lengthText.Position = new Vector2f(GameConstants.GridOffsetX + (GameConstants.GridCols * GameConstants.CellSize / 2f), 50f);
        window.Draw(lengthText);
    }
}
