using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Features.CampusMap.Services;
using System.Text.RegularExpressions;

namespace StudentUsos.Features.CampusMap.Views;

public partial class RoomDetailsViewModel : BaseViewModel
{
    public RoomDetailsParameters Parameters { get; set; }
    ICampusMapService campusMapService;

    public RoomDetailsViewModel(RoomDetailsParameters parameters)
    {
        Parameters = parameters;
        campusMapService = App.ServiceProvider.GetService<ICampusMapService>()!;
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

    [RelayCommand]
    async Task UpvoteButtonPressed(RoomInfo roomInfo)
    {
        await campusMapService.UpvoteUserSuggestion(roomInfo.BuildingId, roomInfo.Floor, roomInfo.RoomId, roomInfo.InternalId);
    }

    [RelayCommand]
    async Task DownvoteButtonPressed(RoomInfo roomInfo)
    {
        await campusMapService.DownvoteUserSuggestion(roomInfo.BuildingId, roomInfo.Floor, roomInfo.RoomId, roomInfo.InternalId);
    }
}
