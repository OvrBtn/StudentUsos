using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StudentUsos.ViewModels;

public class BaseViewModel : ObservableObject, INotifyPropertyChanged
{

    public static class StateKey
    {
        public const string Loading = nameof(Loading);
        public const string Loaded = nameof(Loaded);
        public const string Empty = nameof(Empty);
        public const string LoadingError = nameof(LoadingError);
        public const string ConnectionError = nameof(ConnectionError);
    }
}