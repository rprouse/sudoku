using Sudoku.App.Services;
using Sudoku.App.ViewModels;
using Sudoku.App.Views;
using Sudoku.Core.Game;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Core services
        builder.Services.AddSingleton<ISolver, BitmaskSolver>();
        builder.Services.AddSingleton<ITechniqueSolver, TechniqueSolver>();
        builder.Services.AddSingleton<DifficultyGrader>();
        builder.Services.AddSingleton<SudokuGenerator>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton(_ => new GamePersistenceService(FileSystem.AppDataDirectory));

        // ViewModels
        builder.Services.AddTransient<MenuViewModel>();
        builder.Services.AddSingleton<GameViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<MenuPage>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder.Build();
    }
}
