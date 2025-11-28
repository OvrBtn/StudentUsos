namespace StudentUsos.Services.Navigation;

public interface INavigationResultProvider<T>
{
    public TaskCompletionSource<T?> TaskCompletionSource { get; set; }
}
