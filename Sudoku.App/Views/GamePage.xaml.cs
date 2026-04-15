using Sudoku.App.ViewModels;

namespace Sudoku.App.Views;

public partial class GamePage : ContentPage
{
    private GameViewModel? _viewModel;

    public GamePage(GameViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        SetupBoard();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel?.RefreshSettings();
        _viewModel?.ResumeTimer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel?.StopTimer();
    }

    private void SetupBoard()
    {
        if (_viewModel == null) return;

        BoardView.Drawable = _viewModel.Drawable;
        _viewModel.RequestInvalidate = () =>
        {
            MainThread.BeginInvokeOnMainThread(() => BoardView.Invalidate());
        };

        BoardView.StartInteraction += OnBoardTouched;

        // Make the board square
        BoardView.SizeChanged += (_, _) =>
        {
            var size = Math.Min(BoardView.Width, BoardView.Height);
            if (size > 0)
            {
                BoardView.WidthRequest = size;
                BoardView.HeightRequest = size;
            }
        };

        // Win detection
        _viewModel.PropertyChanged += async (_, args) =>
        {
            if (args.PropertyName == nameof(GameViewModel.IsGameComplete) && _viewModel.IsGameComplete)
            {
                await DisplayAlertAsync("Congratulations!",
                    $"You completed the puzzle!\nTime: {_viewModel.TimerText}\nErrors: {_viewModel.ErrorCount}",
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
        };
    }

    private void OnBoardTouched(object? sender, TouchEventArgs e)
    {
        if (_viewModel == null || e.Touches.Length == 0) return;
        var point = e.Touches[0];
        var (row, col) = _viewModel.Drawable.HitTest(point, (float)BoardView.Width, (float)BoardView.Height);
        if (row >= 0 && col >= 0) _viewModel.OnCellTapped(row, col);
    }

    private void OnNumberClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && int.TryParse(button.Text, out var number))
            _viewModel?.NumberInputCommand.Execute(number);
    }

    private void OnClearClicked(object? sender, EventArgs e) =>
        _viewModel?.ClearInputCommand.Execute(null);

    private void OnNormalModeClicked(object? sender, EventArgs e)
    {
        if (_viewModel != null) _viewModel.IsNormalMode = true;
    }

    private void OnCandidateModeClicked(object? sender, EventArgs e)
    {
        if (_viewModel != null) _viewModel.IsNormalMode = false;
    }

    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");

    private async void OnSettingsClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(SettingsPage));
}
