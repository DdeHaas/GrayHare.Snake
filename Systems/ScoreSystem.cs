namespace GrayHare.Snake.Systems;

/// <summary>Tracks the current score during gameplay.</summary>
internal sealed class ScoreSystem
{
    /// <summary>The player's current score.</summary>
    public int Score { get; private set; }

    /// <summary>Adds points to the current score.</summary>
    /// <param name="points">Number of points to add.</param>
    public void Add(int points)
    {
        Score += points;
    }

    /// <summary>Resets the score to zero.</summary>
    public void Reset()
    {
        Score = 0;
    }
}
