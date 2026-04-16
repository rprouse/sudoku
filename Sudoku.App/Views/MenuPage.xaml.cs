using Sudoku.App.ViewModels;

namespace Sudoku.App.Views;

public partial class MenuPage : ContentPage
{
    private readonly MenuViewModel _viewModel;

    public MenuPage(MenuViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CheckForSavedGameAsync();
    }

    private async void OnSettingsClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(SettingsPage));
}
