using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sudoku.App.Services;
using Sudoku.Core;
using Sudoku.Core.Game;
using Sudoku.Core.Generators;
using Sudoku.Core.Solvers;

namespace Sudoku.App.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    private readonly SudokuGenerator _generator;
    private readonly ISolver _solver;
    private readonly GamePersistenceService _persistenceService;
    private readonly GameViewModel _gameViewModel;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    public partial bool HasSavedGame { get; set; }

    [ObservableProperty]
    public partial string SavedGameInfo { get; set; } = "";

    [ObservableProperty]
    public partial bool IsGenerating { get; set; }

    public MenuViewModel(SudokuGenerator generator, ISolver solver,
        GamePersistenceService persistenceService, GameViewModel gameViewModel,
        SettingsService settingsService)
    {
        _generator = generator;
        _solver = solver;
        _persistenceService = persistenceService;
        _gameViewModel = gameViewModel;
        _settingsService = settingsService;
    }

    public async Task CheckForSavedGameAsync()
    {
        var state = await _persistenceService.LoadAsync();
        if (state != null)
        {
            HasSavedGame = true;
            var minutes = state.ElapsedSeconds / 60;
            var seconds = state.ElapsedSeconds % 60;
            SavedGameInfo = $"{state.Difficulty.DisplayName()} — {minutes}:{seconds:D2}";
        }
        else
        {
            HasSavedGame = false;
            SavedGameInfo = "";
        }
    }

    [RelayCommand]
    private async Task ContinueGame()
    {
        var state = await _persistenceService.LoadAsync();
        if (state == null) return;
        _gameViewModel.LoadState(state);
        await Shell.Current.GoToAsync(nameof(Views.GamePage));
    }

    [RelayCommand]
    private async Task NewGame(string difficultyName)
    {
        if (IsGenerating) return;
        IsGenerating = true;

        var difficulty = Enum.Parse<Difficulty>(difficultyName);
        var (puzzle, solution) = await Task.Run(() =>
        {
            var p = _generator.Generate(difficulty);
            var s = _solver.Solve(new SudokuBoard(p));
            return (p, s);
        });

        _persistenceService.Delete();
        var state = new GameState(puzzle, solution, difficulty);
        _gameViewModel.LoadState(state);

        IsGenerating = false;

        var route = _settingsService.Load().ShowKoans
            ? nameof(Views.KoanPage)
            : nameof(Views.GamePage);
        await Shell.Current.GoToAsync(route);
    }
}
