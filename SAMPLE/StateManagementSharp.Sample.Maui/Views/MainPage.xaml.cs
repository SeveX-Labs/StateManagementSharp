using StateManagementSharp.Sample.Maui.ViewModels;

namespace StateManagementSharp.Sample.Maui.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
