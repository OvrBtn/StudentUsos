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
