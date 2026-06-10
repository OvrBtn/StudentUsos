using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Activities.Models;
using StudentUsos.Features.CampusMap.Views;
using StudentUsos.Features.Groups.Models;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Groups.Views;

public partial class GroupDetailsViewModel : BaseViewModel
{
    INavigationService navigationService;
    public GroupDetailsViewModel(INavigationService navigationService)
    {
        this.navigationService = navigationService;
        //Group = new Group();
    }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsActivitySet))]
    Activity? passedActivity;
    public bool IsActivitySet => PassedActivity is not null;

    public void Init(Group group, Action? onClose, Activity? activity = null)
    {
        Participants = new ObservableCollection<Person.Models.Person>();
        Group = group;
        OnClose = onClose;
        PassedActivity = activity;

        GetMoreParticipants = AddParticipantsToList;

        AddParticipantsToList();

        OnPropertyChanged("LecturerClickedCommand");
    }

    [RelayCommand]
    async Task OpenCampusMapWithSeachingAsync()
    {
        await navigationService.PushAsync<CampusMapPage, Activity>(PassedActivity);
    }

    /// <summary>
    /// Group passed from GroupViewModel
    /// </summary>
    [ObservableProperty] Group group;

    [ObservableProperty] Action? onClose;

    /// <summary>
    /// Collection of participants
    /// </summary>
    [ObservableProperty] ObservableCollection<Person.Models.Person> participants;

    /// <summary>
    /// Starting index of range for loading participansts with scroll
    /// </summary>
    int takeParticipantsFrom = 0;
    /// <summary>
    /// The amount of participants to load when user scrolled to the end of ScrollView
    /// </summary>
    int takeParticipantsAmount = 20;
    [ObservableProperty] Action getMoreParticipants;

    /// <summary>
    /// Handles adding more participants to Participants collection when user scrolled to the end of ScrollView
    /// </summary>
    void AddParticipantsToList()
    {
        var takeTo = Math.Min(Group.Participants.Count - 1, takeParticipantsFrom + takeParticipantsAmount);
        for (int i = takeParticipantsFrom; i <= takeTo; i++)
        {
            Participants.Add(Group.Participants[i]);
        }
        takeParticipantsFrom += takeParticipantsAmount + 1;
    }
}