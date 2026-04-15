using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sudoku.App.Controls;
using Sudoku.App.Services;
using Sudoku.Core.Game;

namespace Sudoku.App.ViewModels;

public partial class GameViewModel : ObservableObject, IDisposable
{
    private readonly GamePersistenceService _persistenceService;
    private readonly SettingsService _settingsService;
    private System.Timers.Timer? _timer;

    [ObservableProperty]
    private GameState? _state;

    [ObservableProperty]
    private GameSettings _settings = new();

    [ObservableProperty]
    private int _selectedRow = -1;

    [ObservableProperty]
    private int _selectedCol = -1;

    [ObservableProperty]
    private bool _isNormalMode = true;

    [ObservableProperty]
    private bool _isAutoCandidateMode;

    [ObservableProperty]
    private string _timerText = "0:00";

    [ObservableProperty]
    private string _difficultyText = "";

    [ObservableProperty]
    private int _errorCount;

    [ObservableProperty]
    private bool _isGameComplete;

    public SudokuBoardDrawable Drawable { get; } = new();
    public Action? RequestInvalidate { get; set; }

    public GameViewModel(GamePersistenceService persistenceService, SettingsService settingsService)
    {
        _persistenceService = persistenceService;
        _settingsService = settingsService;
        Settings = _settingsService.Load();
    }

    public void LoadState(GameState state)
    {
        State = state;
        DifficultyText = state.Difficulty.ToString();
        ErrorCount = state.ErrorCount;
        IsGameComplete = false;
        UpdateTimerText();
        UpdateDrawable();
        StartTimer();
    }

    public void OnCellTapped(int row, int col)
    {
        if (State == null || IsGameComplete) return;
        SelectedRow = row;
        SelectedCol = col;
        UpdateDrawable();
    }

    [RelayCommand]
    private void NumberInput(int number)
    {
        if (State == null || SelectedRow < 0 || SelectedCol < 0 || IsGameComplete) return;

        if (IsNormalMode)
        {
            State.PlaceNumber(SelectedRow, SelectedCol, number, Settings);
            ErrorCount = State.ErrorCount;
            if (IsAutoCandidateMode) State.ComputeAutoCandidates();
            if (State.IsComplete())
            {
                IsGameComplete = true;
                StopTimer();
            }
        }
        else
        {
            State.ToggleCandidate(SelectedRow, SelectedCol, number);
        }

        UpdateDrawable();
        _ = SaveGameAsync();
    }

    [RelayCommand]
    private void ClearInput()
    {
        if (State == null || SelectedRow < 0 || SelectedCol < 0 || IsGameComplete) return;

        if (IsNormalMode)
        {
            State.ClearCell(SelectedRow, SelectedCol);
            if (IsAutoCandidateMode) State.ComputeAutoCandidates();
        }
        else
        {
            State.ClearCandidates(SelectedRow, SelectedCol);
        }

        UpdateDrawable();
        _ = SaveGameAsync();
    }

    [RelayCommand]
    private void UndoMove()
    {
        if (State == null || IsGameComplete) return;
        State.Undo();
        if (IsAutoCandidateMode) State.ComputeAutoCandidates();
        ErrorCount = State.ErrorCount;
        UpdateDrawable();
        _ = SaveGameAsync();
    }

    partial void OnIsAutoCandidateModeChanged(bool value)
    {
        if (State == null) return;
        if (value)
            State.ComputeAutoCandidates();
        else
            State.ClearAutoCandidates();
        UpdateDrawable();
    }

    private void UpdateDrawable()
    {
        Drawable.State = State;
        Drawable.Settings = Settings;
        Drawable.SelectedRow = SelectedRow;
        Drawable.SelectedCol = SelectedCol;
        Drawable.IsDarkTheme = Settings.IsDarkTheme;
        RequestInvalidate?.Invoke();
    }

    private void StartTimer()
    {
        _timer?.Dispose();
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += (_, _) =>
        {
            if (State == null) return;
            State.ElapsedSeconds++;
            MainThread.BeginInvokeOnMainThread(UpdateTimerText);
        };
        _timer.Start();
    }

    public void StopTimer() => _timer?.Stop();

    public void ResumeTimer()
    {
        if (!IsGameComplete) _timer?.Start();
    }

    private void UpdateTimerText()
    {
        if (State == null) return;
        var minutes = State.ElapsedSeconds / 60;
        var seconds = State.ElapsedSeconds % 60;
        TimerText = $"{minutes}:{seconds:D2}";
    }

    private async Task SaveGameAsync()
    {
        if (State != null) await _persistenceService.SaveAsync(State);
    }

    public void RefreshSettings()
    {
        Settings = _settingsService.Load();
        UpdateDrawable();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
