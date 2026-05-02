using Sudoku.App.ViewModels;

namespace Sudoku.App.Views;

public partial class KoanPage : ContentPage
{
    private readonly KoanViewModel _viewModel;

    public KoanPage(KoanViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
