namespace GrayHare.Snake;

/// <summary>Holds relative paths to all game audio and font assets.</summary>
internal sealed record AssetsManifest
{
    public AssetsManifest(
        string eatSoundPath,
        string bonusEatSoundPath,
        string deathSoundPath,
        string musicPath,
        string fontPath)
    {
        EatSoundPath = string.IsNullOrEmpty(eatSoundPath) ? throw new ArgumentException("Value cannot be null or empty.", nameof(eatSoundPath)) : eatSoundPath;
        BonusEatSoundPath = string.IsNullOrEmpty(bonusEatSoundPath) ? throw new ArgumentException("Value cannot be null or empty.", nameof(bonusEatSoundPath)) : bonusEatSoundPath;
        DeathSoundPath = string.IsNullOrEmpty(deathSoundPath) ? throw new ArgumentException("Value cannot be null or empty.", nameof(deathSoundPath)) : deathSoundPath;
        MusicPath = string.IsNullOrEmpty(musicPath) ? throw new ArgumentException("Value cannot be null or empty.", nameof(musicPath)) : musicPath;
        FontPath = string.IsNullOrEmpty(fontPath) ? throw new ArgumentException("Value cannot be null or empty.", nameof(fontPath)) : fontPath;
    }

    public string EatSoundPath { get; init; }
    public string BonusEatSoundPath { get; init; }
    public string DeathSoundPath { get; init; }
    public string MusicPath { get; init; }
    public string FontPath { get; init; }
}
