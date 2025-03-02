using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace StudentUsos.Services
{
    /// <summary>
    /// Main purpose of this is to abstract any methods which are MAUI specific so they can be mocked in unit tests
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        public static IApplicationService Default { get; private set; }

        public ApplicationService()
        {
            Default = this;
        }

        public void MainThreadInvoke(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }

        public void ShowToast(string message, IApplicationService.ToastDuration toastDuration = IApplicationService.ToastDuration.Short, double textSize = 14)
        {
            var toast = Toast.Make(message, toastDuration == IApplicationService.ToastDuration.Short ? ToastDuration.Short : ToastDuration.Long, textSize);
            toast.Show();
        }

        static Color? Gray600 { get; set; }
        public async Task ShowSnackBarAsync(string text, string buttonText, Action? action = null)
        {
            if (Gray600 == null)
            {
                Gray600 = Utilities.GetColorFromResources("Gray600");
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var snackbarOptions = new SnackbarOptions
            {
                BackgroundColor = Gray600,
                TextColor = Colors.White,
                ActionButtonTextColor = Colors.White,
                CornerRadius = new CornerRadius(20),
            };

            TimeSpan duration = TimeSpan.FromSeconds(3);

            var snackbar = Snackbar.Make(text, action, buttonText, duration, snackbarOptions);

            await snackbar.Show(cancellationTokenSource.Token);
        }

        public void ShowErrorMessage(string title, string message)
        {
            _ = ShowSnackBarAsync($"{title}: {message}", "ok");
        }

        public Task WorkerThreadInvoke(Action action)
        {
            return Task.Run(action);
        }

#nullable enable
        public Task WorkerThreadInvoke(Func<Task?> func)
        {
            return Task.Run(func);
        }
#nullable disable
    }
}
