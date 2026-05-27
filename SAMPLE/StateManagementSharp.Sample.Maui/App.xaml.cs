using Microsoft.Extensions.DependencyInjection;
using StateManagementSharp.Sample.Maui.Views;

namespace StateManagementSharp.Sample.Maui;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var mainPage = _serviceProvider.GetRequiredService<MainPage>();

        return new Window(mainPage);
    }
}
