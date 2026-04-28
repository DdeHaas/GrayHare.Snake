namespace GrayHare.Snake;

/// <summary>Persists and retrieves the highest score from disk.</summary>
internal static class HighScoreStore
{
    /// <summary>Loads the saved high score, returning 0 if no valid file exists.</summary>
    public static int Load()
    {
        try
        {
            return int.Parse(File.ReadAllText(GameConstants.HighScorePath));
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>Saves the score to disk if it exceeds the current high score.</summary>
    /// <remarks>
    /// Holds an exclusive file lock for the entire read-check-write sequence to prevent
    /// a TOCTOU race when multiple game instances close simultaneously.
    /// </remarks>
    public static void Save(int score)
    {
        try
        {
            using var fs = new FileStream(
                GameConstants.HighScorePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);

            int current = 0;

            if (fs.Length > 0)
            {
                byte[] buf = new byte[fs.Length];
                _ = fs.Read(buf, 0, buf.Length);

                if (int.TryParse(System.Text.Encoding.UTF8.GetString(buf), out int parsed))
                {
                    current = parsed;
                }
            }

            if (score > current)
            {
                byte[] newBytes = System.Text.Encoding.UTF8.GetBytes(score.ToString());
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(newBytes);
                fs.SetLength(newBytes.Length);
            }
        }
        catch { /* best-effort */ }
    }
}
