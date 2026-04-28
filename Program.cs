using GrayHare.GameEngine.Application;
using GrayHare.Snake;
using GrayHare.Snake.Scenes;
using SFML.Graphics;
using SFML.System;

string contentRoot = Path.Combine(AppContext.BaseDirectory, "Assets");
Directory.CreateDirectory(contentRoot);

AssetsManifest assets = Assets.EnsureCreated(contentRoot);

GameApplicationOptions options = new()
{
    Title = "Snake",
    WindowSize = new Vector2u(GameConstants.WindowWidth, GameConstants.WindowHeight),
    ClearColor = new Color(20, 20, 20),
    FrameRateLimit = 60,
    VerticalSyncEnabled = true,
    ContentRootPath = contentRoot,
    LogHandler = Console.WriteLine,
};

new GameApplication(options).Run(new WelcomeScene(assets));
