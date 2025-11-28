namespace StudentUsos.Services.Navigation;

public interface INavigableWithParameter<T>
{
    public void OnNavigated(T navigationParameter);
}
