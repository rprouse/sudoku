using Sudoku.App.Services;
using Sudoku.App.ViewModels;

namespace Sudoku.App;

public partial class App : Application
{
    private readonly GameViewModel _gameViewModel;
    private readonly GamePersistenceService _persistenceService;

    public App(SettingsService settingsService, GameViewModel gameViewModel, GamePersistenceService persistenceService)
    {
        InitializeComponent();
        _gameViewModel = gameViewModel;
        _persistenceService = persistenceService;

        var settings = settingsService.Load();
        UserAppTheme = settings.IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        window.Stopped += (_, _) =>
        {
            _gameViewModel.StopTimer();
            if (_gameViewModel.State != null)
                _ = _persistenceService.SaveAsync(_gameViewModel.State);
        };

        window.Resumed += (_, _) =>
        {
            _gameViewModel.ResumeTimer();
        };

        return window;
    }
}
