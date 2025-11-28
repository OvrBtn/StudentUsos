namespace StudentUsos.Services;

public interface INavigationResultProvider<T>
{
    public TaskCompletionSource<T?> TaskCompletionSource { get; set; }
}
