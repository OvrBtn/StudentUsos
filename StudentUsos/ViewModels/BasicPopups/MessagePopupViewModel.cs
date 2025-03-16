using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudentUsos.ViewModels;

public partial class MessagePopupPageViewModel : ObservableObject
{
    MessagePopupPage popup;

    public MessagePopupPageViewModel(MessagePopupPage popup, MessagePopupPage.YesAndNoPopupParameters intent)
    {
        this.popup = popup;

        YesAndNoParameters = intent;

        Title = intent.Title;
        Description = intent.Description;
        No = intent.No;
        Yes = intent.Yes;
        onYesClicked = intent.YesAction;
        onNoClicked = intent.NoAction;
        IsOkVisible = false;
    }

    public MessagePopupPageViewModel(MessagePopupPage popup, MessagePopupPage.OkPopupParameters intent)
    {
        this.popup = popup;

        OkParameters = intent;

        Title = intent.Title;
        Description = intent.Description;
        Ok = intent.Ok;
        onOkClicked = intent.OkAction;
        IsYesNoVisible = false;
    }

    public MessagePopupPage.YesAndNoPopupParameters YesAndNoParameters { get; private set; }
    public MessagePopupPage.OkPopupParameters OkParameters { get; private set; }

    [ObservableProperty] string title;
    [ObservableProperty] string description;
    [ObservableProperty] string yes;
    [ObservableProperty] string no;
    [ObservableProperty] string ok;
    [ObservableProperty] bool isOkVisible = true;
    [ObservableProperty] bool isYesNoVisible = true;


    Action? onYesClicked;
    Action? onNoClicked;
    Action? onOkClicked;

    [RelayCommand]
    void YesClicked()
    {
        popup.Close();
        onYesClicked?.Invoke();
    }

    [RelayCommand]
    void NoClicked()
    {
        popup.Close();
        onNoClicked?.Invoke();
    }

    [RelayCommand]
    void OkClicked()
    {
        popup.Close();
        onOkClicked?.Invoke();
    }



}