using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Groups.Models;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Groups.Views;

public partial class GroupDetailsViewModel : BaseViewModel
{
    public GroupDetailsViewModel()
    {
        //Group = new Group();
    }

    public void Init(Group group, Action? onClose)
    {
        Participants = new ObservableCollection<Person.Models.Person>();
        Group = group;
        OnClose = onClose;

        GetMoreParticipants = AddParticipantsToList;

        AddParticipantsToList();

        OnPropertyChanged("LecturerClickedCommand");
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