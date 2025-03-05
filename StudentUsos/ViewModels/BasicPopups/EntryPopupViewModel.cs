using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudentUsos.ViewModels;

public partial class EntryPopupViewModel : PopupViewModelBase
{
    public EntryPopupViewModel(EntryPopup.EntryPopupParameters intent)
    {
        Parameters = intent;

        Title = intent.Title;
        Description = intent.Description;
        Confirm = intent.Confirm;
        Cancel = intent.Cancel;
        Keyboard = intent.Keyboard;
    }

    public EntryPopup.EntryPopupParameters Parameters { get; private set; }

    [ObservableProperty] string entryText;
    [ObservableProperty] string title;
    [ObservableProperty] string description;
    [ObservableProperty] string confirm;
    [ObservableProperty] string cancel;
    [ObservableProperty] Keyboard? keyboard;

    [RelayCommand]
    void ConfirmClicked()
    {
        Parameters.ConfirmAction?.Invoke(EntryText);
        Close();
    }

    [RelayCommand]
    void CancelClicked()
    {
        Parameters.CancelAction?.Invoke();
        Close();
    }
}