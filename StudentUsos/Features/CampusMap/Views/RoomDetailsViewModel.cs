using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.CampusMap.Models;
using StudentUsos.Features.CampusMap.Repositories;
using StudentUsos.Features.CampusMap.Services;
using StudentUsos.Resources.LocalizedStrings;
using System.Text.RegularExpressions;

namespace StudentUsos.Features.CampusMap.Views;

public partial class RoomDetailsViewModel : BaseViewModel
{
    public RoomDetailsParameters Parameters { get; set; }
    ICampusMapService campusMapService;
    IApplicationService applicationService;
    ICampusMapRepository campusMapRepository;
    public RoomDetailsViewModel(RoomDetailsParameters parameters)
    {
        Parameters = parameters;
        campusMapService = App.ServiceProvider.GetService<ICampusMapService>()!;
        applicationService = App.ServiceProvider.GetService<IApplicationService>()!;
        campusMapRepository = App.ServiceProvider.GetService<ICampusMapRepository>()!;

        foreach (var item in parameters.FullRoomInfos)
        {
            item.IsUpvoted = campusMapRepository.IsUpvoted(item);
            item.IsDownvoted = campusMapRepository.IsDownvoted(item);
        }
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
    async Task UpvoteButtonPressedAsync(RoomInfo roomInfo)
    {
        if (isCurrentlyCastingVote)
        {
            return;
        }

        isCurrentlyCastingVote = true;

        var statusCode = await campusMapService.UpvoteUserSuggestionAsync(roomInfo.BuildingId, roomInfo.Floor, roomInfo.RoomId, roomInfo.InternalId);
        if (statusCode != System.Net.HttpStatusCode.OK)
        {
            applicationService.ShowToast(LocalizedStrings.SomethingWentWrong);
            isCurrentlyCastingVote = false;
            return;
        }

        applicationService.ShowToast(LocalizedStrings.Success);

        campusMapRepository.UnmarkAsDownvoted(roomInfo);
        if (roomInfo.IsUpvoted)
        {
            campusMapRepository.UnmarkAsUpvoted(roomInfo);
        }
        else
        {
            campusMapRepository.MarkAsUpvoted(roomInfo);
        }

        roomInfo.IsDownvoted = false;
        roomInfo.IsUpvoted = !roomInfo.IsUpvoted;

        isCurrentlyCastingVote = false;
    }

    [RelayCommand]
    async Task DownvoteButtonPressedAsync(RoomInfo roomInfo)
    {
        if (isCurrentlyCastingVote)
        {
            return;
        }

        isCurrentlyCastingVote = true;

        var statusCode = await campusMapService.DownvoteUserSuggestionAsync(roomInfo.BuildingId, roomInfo.Floor, roomInfo.RoomId, roomInfo.InternalId);
        if (statusCode != System.Net.HttpStatusCode.OK)
        {
            applicationService.ShowToast(LocalizedStrings.SomethingWentWrong);
            isCurrentlyCastingVote = false;
            return;
        }
        applicationService.ShowToast(LocalizedStrings.Success);

        campusMapRepository.UnmarkAsUpvoted(roomInfo);
        if (roomInfo.IsDownvoted)
        {
            campusMapRepository.UnmarkAsDownvoted(roomInfo);
        }
        else
        {
            campusMapRepository.MarkAsDownvoted(roomInfo);
        }

        roomInfo.IsUpvoted = false;
        roomInfo.IsDownvoted = !roomInfo.IsDownvoted;

        isCurrentlyCastingVote = false;
    }
}
