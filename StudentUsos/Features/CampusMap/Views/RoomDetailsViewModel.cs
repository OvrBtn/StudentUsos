using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Features.CampusMap.Services;
using StudentUsos.Resources.LocalizedStrings;
using System.Text.RegularExpressions;

namespace StudentUsos.Features.CampusMap.Views;

public partial class RoomDetailsViewModel : BaseViewModel
{
    public RoomDetailsParameters Parameters { get; set; }
    ICampusMapService campusMapService;
    IApplicationService applicationService;
    public RoomDetailsViewModel(RoomDetailsParameters parameters)
    {
        Parameters = parameters;
        campusMapService = App.ServiceProvider.GetService<ICampusMapService>()!;
        applicationService = App.ServiceProvider.GetService<IApplicationService>()!; ;
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

    bool isCurrentlySendingSuggestion = false;

    [RelayCommand]
    void SendButtonPressed()
    {
        if (isCurrentlySendingSuggestion)
        {
            return;
        }

        isCurrentlySendingSuggestion = true;
        Parameters.ConfirmAction?.Invoke(UserSuggestion);
        isCurrentlySendingSuggestion = false;
    }

    bool isCurrentlyCastingVote = false;

    [RelayCommand]
    async Task UpvoteButtonPressed(RoomInfo roomInfo)
    {
        if (isCurrentlyCastingVote)
        {
            return;
        }

        isCurrentlyCastingVote = true;

        var statusCode = await campusMapService.UpvoteUserSuggestion(roomInfo.BuildingId, roomInfo.Floor, roomInfo.RoomId, roomInfo.InternalId);
        if (statusCode == System.Net.HttpStatusCode.OK)
        {
            applicationService.ShowToast(LocalizedStrings.Success);
            roomInfo.IsDownvoted = false;
            roomInfo.IsUpvoted = !roomInfo.IsUpvoted;
        }
        else
        {
            applicationService.ShowToast(LocalizedStrings.SomethingWentWrong);
        }

        isCurrentlyCastingVote = false;
    }

    [RelayCommand]
    async Task DownvoteButtonPressed(RoomInfo roomInfo)
    {
        if (isCurrentlyCastingVote)
        {
            return;
        }

        isCurrentlyCastingVote = true;

        var statusCode = await campusMapService.DownvoteUserSuggestion(roomInfo.BuildingId, roomInfo.Floor, roomInfo.RoomId, roomInfo.InternalId);
        if (statusCode == System.Net.HttpStatusCode.OK)
        {
            applicationService.ShowToast(LocalizedStrings.Success);
            roomInfo.IsUpvoted = false;
            roomInfo.IsDownvoted = !roomInfo.IsDownvoted;
        }
        else
        {
            applicationService.ShowToast(LocalizedStrings.SomethingWentWrong);
        }

        isCurrentlyCastingVote = false;
    }
}
