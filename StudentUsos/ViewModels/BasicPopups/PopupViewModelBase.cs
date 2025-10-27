using CommunityToolkit.Mvvm.ComponentModel;

namespace StudentUsos.ViewModels;

public partial class PopupViewModelBase : BaseViewModel
{
    [ObservableProperty] float contentOpacity = 1f;

    public void Close()
    {
        ContentOpacity = 0;
        App.Current?.Windows[0].Page?.Navigation.PopModalAsync(false);
    }
}