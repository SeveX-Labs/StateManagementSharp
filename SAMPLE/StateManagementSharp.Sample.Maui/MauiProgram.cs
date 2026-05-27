using Microsoft.Extensions.Logging;
using StateManagementSharp.Sample.Maui.Store;
using StateManagementSharp.Sample.Maui.ViewModels;
using StateManagementSharp.Sample.Maui.Views;

namespace StateManagementSharp.Sample.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddStateManagementSharp(typeof(InternalStore).Assembly);
        builder.Services.AddSingleton<InternalStore>();
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}