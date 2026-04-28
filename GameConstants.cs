using SFML.Graphics;

namespace GrayHare.Snake;

/// <summary>Shared constants for window layout, grid sizing, timing, and colors.</summary>
internal static class GameConstants
{
    // Window
    public const uint WindowWidth = 640;
    public const uint WindowHeight = 680;

    // Grid layout
    public const int GridCols = 20;
    public const int GridRows = 20;
    public const int CellSize = 28;
    public const int GridOffsetX = 40;
    public const int GridOffsetY = 80;

    // Game speed (8 Hz tick rate)
    public const float TickRate = 8f;
    public const float TickInterval = 1f / TickRate;

    // Scoring
    public const int NormalFoodPoints = 10;
    public const int BonusFoodPoints = 50;

    // Bonus food
    public const float BonusFoodChance = 0.15f;
    public const float BonusFoodDuration = 8.0f;

    // High score
    public static readonly string HighScorePath = Path.Combine(AppContext.BaseDirectory, "snake_highscore.dat");

    // UI
    public const string FontName = "fonts/Sansation_Regular.ttf";
    public const int GameOverWaitTimer = 6;

    // Colors
    public static readonly Color SnakeHeadColor = new(60, 200, 60);
    public static readonly Color SnakeBodyColor = new(30, 150, 30);
    public static readonly Color FoodColor = new(220, 60, 60);
    public static readonly Color BonusFoodColor = new(255, 200, 50);
    public static readonly Color GridLineColor = new(40, 40, 40);
    public static readonly Color HudColor = Color.White;
}
