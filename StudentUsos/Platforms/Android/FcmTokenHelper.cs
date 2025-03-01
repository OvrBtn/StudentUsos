using Android.Gms.Tasks;
using Firebase.Messaging;

namespace StudentUsos.Platforms.Android
{
    public class FcmTokenHelper : Java.Lang.Object, IOnSuccessListener
    {
        private TaskCompletionSource<string> taskCompletionSource;

        public Task<string> GetFirebaseTokenAsync()
        {
            taskCompletionSource = new TaskCompletionSource<string>();
            FirebaseMessaging.Instance.GetToken().AddOnSuccessListener(this);
            return taskCompletionSource.Task;
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            taskCompletionSource.TrySetResult(result.ToString());
        }
    }
}
