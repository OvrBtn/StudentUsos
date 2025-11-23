namespace StudentUsos.Services.Application;

/// <summary>
/// Main purpose of this is to abstract any methods which are MAUI specific so they can be mocked in unit tests
/// </summary>
public interface IApplicationService
{
    public IAppInfo ApplicationInfo { get; }

    public void MainThreadInvoke(Action action);

    public enum ToastDuration
    {
        Short,
        Long
    }
    public void ShowToast(string message, ToastDuration toastDuration = ToastDuration.Short, double textSize = 14);

    public Task ShowSnackBarAsync(string text, string buttonText, Action? action = null);

    public void ShowErrorMessage(string title, string message);

    //mocked to avoid having to use Task.Delay in tests
    public Task WorkerThreadInvoke(Action action);
    public Task WorkerThreadInvoke(Func<Task?> func);

}