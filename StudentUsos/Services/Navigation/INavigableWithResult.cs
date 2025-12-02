namespace StudentUsos.Services.Navigation;

public interface INavigableWithResult<T>
{
    public TaskCompletionSource<T?> TaskCompletionSource { get; set; }
}
