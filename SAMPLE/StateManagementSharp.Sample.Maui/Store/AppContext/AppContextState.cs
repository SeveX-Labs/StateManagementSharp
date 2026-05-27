namespace StateManagementSharp.Sample.Maui.Store;

public readonly struct AppContextState : State
{
    public string ApplicationTitle { get; }
    public bool IsBusy { get; }

    public AppContextState(string applicationTitle, bool isBusy)
    {
        ApplicationTitle = applicationTitle;
        IsBusy = isBusy;
    }

    public AppContextState WithApplicationTitle(string applicationTitle)
    {
        return new AppContextState(applicationTitle, IsBusy);
    }

    public AppContextState WithIsBusy(bool isBusy)
    {
        return new AppContextState(ApplicationTitle, isBusy);
    }
}
