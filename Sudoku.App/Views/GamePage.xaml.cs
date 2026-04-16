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

    private Button[] _numberButtons = [];

    private void SetupBoard()
    {
        if (_viewModel == null) return;

        _numberButtons = [NumButton1, NumButton2, NumButton3, NumButton4, NumButton5,
                          NumButton6, NumButton7, NumButton8, NumButton9];

        BoardView.Drawable = _viewModel.Drawable;
        _viewModel.RequestInvalidate = () =>
        {
            MainThread.BeginInvokeOnMainThread(() => BoardView.Invalidate());
        };
        _viewModel.RequestUpdateNumberButton = (number, isEnabled) =>
        {
            MainThread.BeginInvokeOnMainThread(() => _numberButtons[number - 1].IsEnabled = isEnabled);
        };

        BoardView.StartInteraction += OnBoardTouched;

        // Win detection
        _viewModel.PropertyChanged += async (_, args) =>
        {
            if (args.PropertyName == nameof(GameViewModel.IsGameComplete) && _viewModel.IsGameComplete)
            {
                await ShowWinOverlayAsync();
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

    private async Task ShowWinOverlayAsync()
    {
        WinOverlay.IsVisible = true;
        WinWordLabel.Opacity = 0;

        // Fade in the overlay backdrop and card
        await WinOverlay.FadeToAsync(1, 250, Easing.CubicOut);

        // Then fade in the congratulatory word slightly slower
        await WinWordLabel.FadeToAsync(1, 500, Easing.CubicOut);
    }

    private async void OnContinueClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");

    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");

    private async void OnSettingsClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(SettingsPage));
}
