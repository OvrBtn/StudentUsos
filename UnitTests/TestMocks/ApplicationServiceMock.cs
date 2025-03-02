using StudentUsos.Services;

namespace UnitTests.TestMocks
{
    public class ApplicationServiceMock : IApplicationService
    {
        public void MainThreadInvoke(Action action)
        {
            action?.Invoke();
        }

        public void ShowErrorMessage(string title, string message)
        {
            throw new Exception($"{title} {message}");
        }

        public Task ShowSnackBarAsync(string text, string buttonText, Action? action = null)
        {
            return Task.CompletedTask;
        }

        public void ShowToast(string message, IApplicationService.ToastDuration toastDuration = IApplicationService.ToastDuration.Short, double textSize = 14)
        {

        }

        public Task WorkerThreadInvoke(Action action)
        {
            action?.Invoke();
            return Task.CompletedTask;
        }

        public Task WorkerThreadInvoke(Func<Task?> func)
        {
            func?.Invoke();
            return Task.CompletedTask;
        }
    }
}
