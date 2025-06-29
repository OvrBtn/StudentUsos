using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace StudentUsos.Features.CampusMap.Views;

public partial class RoomDetailsViewModel : BaseViewModel
{
    public RoomDetailsParameters Parameters { get; set; }

    public RoomDetailsViewModel(RoomDetailsParameters parameters)
    {
        Parameters = parameters;
    }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsSendButtonEnabled))]
    string userSuggestion;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsSendButtonEnabled))]
    bool hasError;
    public bool IsSendButtonEnabled { get => HasError == false && string.IsNullOrEmpty(UserSuggestion) == false; }

    [RelayCommand]
    void TextChanged()
    {
        HasError = Regex.IsMatch(UserSuggestion, @"^[a-zA-Z0-9ąćęłńóśźżĄĆĘŁŃÓŚŹŻ .,_-]*$") == false;
    }

    [RelayCommand]
    void SendButtonPressed()
    {
        Parameters.ConfirmAction?.Invoke(UserSuggestion);
    }
}
