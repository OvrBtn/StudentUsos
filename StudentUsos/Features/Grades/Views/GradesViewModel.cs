using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.AcademicTerms.Models;
using StudentUsos.Features.AcademicTerms.Repositories;
using StudentUsos.Features.AcademicTerms.Services;
using StudentUsos.Features.Grades.Helpers;
using StudentUsos.Features.Grades.Models;
using StudentUsos.Features.Grades.Repositories;
using StudentUsos.Features.Grades.Services;
using StudentUsos.Features.Groups.Models;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Groups.Services;
using StudentUsos.Features.StudentProgrammes.Services;
using StudentUsos.Resources.LocalizedStrings;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StudentUsos.Features.Grades.Views;

public partial class GradesViewModel : BaseViewModel
{
    IServiceProvider serviceProvider;
    IGroupsService groupsService;
    IGroupsRepository groupsRepository;
    IGradesService gradesService;
    IGradesRepository gradesRepository;
    IStudentProgrammeService studentProgrammeService;
    ITermsService termsService;
    ITermsRepository termsRepository;
    IApplicationService applicationService;
    ILogger? logger;
    public GradesViewModel(IServiceProvider serviceProvider,
        IGroupsService groupsService,
        IGroupsRepository groupsRepository,
        IGradesService gradesService,
        IGradesRepository gradesRepository,
        IStudentProgrammeService studentProgrammeService,
        ITermsService termsService,
        ITermsRepository termsRepository,
        IApplicationService applicationService,
        ILogger? logger = null)
    {
        this.serviceProvider = serviceProvider;
        this.groupsService = groupsService;
        this.groupsRepository = groupsRepository;
        this.gradesService = gradesService;
        this.gradesRepository = gradesRepository;
        this.studentProgrammeService = studentProgrammeService;
        this.termsService = termsService;
        this.termsRepository = termsRepository;
        this.applicationService = applicationService;
        this.logger = logger;

        GradesStateKey = StateKey.Loading;
        OpenTermsListCommand = new Command(() => applicationService.WorkerThreadInvoke(MoreTermsButtonClickedAsync));

        CurrentlyDisplayedTerm = new Term();
    }

    [ObservableProperty] string gradesStateKey = StateKey.Loading;

    [ObservableProperty] ObservableCollection<FinalGradeGroup> gradesGroups = new();

    [ObservableProperty] Term currentlyDisplayedTerm;
    [ObservableProperty] string gradeAverage;

    [ObservableProperty] bool isEditingEnabled = false;

    [RelayCommand]
    void SwitchEditing()
    {
        IsEditingEnabled = !IsEditingEnabled;
    }

    public void Init()
    {
        applicationService.WorkerThreadInvoke(InitViewModelAsync);
    }

    async void InitViewModelAsync()
    {
        await Task.Delay(10);
        try
        {
            var gradesLocal = gradesRepository.GetAll();
            var groups = groupsRepository.GetAll();
            bool groupsChanged = await groupsService.SetEctsPointsIfNotSetAsync(groups);
            if (groupsChanged)
            {
                groupsRepository.InsertOrReplace(groups);
            }
            GradesHelper.AssignAcademicGroupsToGrades(groups, gradesLocal);
            var groupedGradesLocal = GradesHelper.GroupGrades(gradesLocal).ToObservableCollection();

            applicationService.MainThreadInvoke(() =>
            {
                GradesGroups = groupedGradesLocal.ToObservableCollection();
                GradeAverage = GradesHelper.CalculateGradeAverage(GradesGroups).ToString();
            });

            //get first active term and term's groups
            if (termsRepository.TryGettingCurrentTerm(out var currentTerm) == false)
            {
                currentTerm = await termsService.GetCurrentTermAsync();
            }
            if (currentTerm == null)
            {
                if (GradesGroups.Count == 0) GradesStateKey = StateKey.ConnectionError;
                return;
            }
            CurrentlyDisplayedTerm = currentTerm;

            if (groupedGradesLocal.Count == 0) GradesStateKey = StateKey.Loading;
            else GradesStateKey = StateKey.Loaded;

            groups = groupsRepository.GetGroups(currentTerm);

            if (groupedGradesLocal.Count > 0) await Task.Delay(2000);

            //get grades
            var grades = await gradesService.GetGradesServerAsync(groups, currentTerm.Id);
            if (grades == null)
            {
                if (GradesGroups.Count == 0) GradesStateKey = StateKey.ConnectionError;
                return;
            }
            var groupedGradesServer = GradesHelper.GroupGrades(grades).ToObservableCollection();
            GradesHelper.CopyGradeDistributions(groupedGradesLocal, groupedGradesServer);

            //set data to collection observed by UI
            applicationService.MainThreadInvoke(() =>
            {
                if (groupedGradesLocal.Count != groupedGradesServer.Count)
                {
                    GradesGroups = groupedGradesServer;
                }
                else
                {
                    for (int i = 0; i < groupedGradesLocal.Count; i++)
                    {
                        if (FinalGradeGroup.AreCourseExamAndGradeEqual(groupedGradesLocal[i], groupedGradesServer[i]) == false)
                        {
                            GradesGroups[i] = groupedGradesServer[i];
                        }
                    }
                }
                GradeAverage = GradesHelper.CalculateGradeAverage(GradesGroups).ToString();
            });

            if (groupedGradesServer.Count > 0) GradesStateKey = StateKey.Loaded;
            else if (GradesGroups.Count == 0) GradesStateKey = StateKey.Empty;

            //getting GradeDistribution for new or modified groups and saving to local database
            await gradesService.FetchOrCopyGradeDistributionForNewGradesFromApiAsync(groupedGradesServer);
            gradesRepository.DeleteAll();
            gradesRepository.InsertAll(GradesHelper.UngroupGrades(groupedGradesServer));
        }
        catch (Exception ex) { logger?.LogCatchedException(ex); }
    }

    [RelayCommand]
    void GradeClicked(FinalGradeGroup finalGradeGroup)
    {
        if (IsEditingEnabled)
        {
            var page = serviceProvider.GetService<ModifyGradePage>()!;
            page.Init(finalGradeGroup, () =>
            {
                GradeAverage = GradesHelper.CalculateGradeAverage(GradesGroups).ToString();
            });
            page.ShowPopup();
        }
        else
        {
            var detailsPage = serviceProvider.GetService<GradeDetailsPage>()!;
            detailsPage.Init(finalGradeGroup);
            detailsPage.ShowPopup();
        }
    }


    static int moreTermsClickedCounter;

    [ObservableProperty] Command openTermsListCommand;
    async Task MoreTermsButtonClickedAsync()
    {
        try
        {
            if (moreTermsClickedCounter >= 15)
            {
                applicationService.ShowToast(LocalizedStrings.OperationCanceledDueToSpam);
                return;
            }

            moreTermsClickedCounter++;

            //show popup with loading animation
            var popup = PickFromListPopup.CreateAndShow(LocalizedStrings.Semesters, new List<string>(), StateKey.Loading);
            if (popup is null)
            {
                return;
            }

            var studentProgrammes = await studentProgrammeService.GetStudentProgrammesAsync();
            if (studentProgrammes == null || studentProgrammes.Count == 0)
            {
                popup.CollectionStateKey = StateKey.ConnectionError;
                return;
            }

            //find the earliest admission date (probably there can be more than one student programme)
            DateTime minAdmissionDate = DateTimeOffset.Now.DateTime;
            foreach (var programme in studentProgrammes)
            {
                if (DateTime.TryParseExact(programme.AdmissionDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime result) && result < minAdmissionDate)
                {
                    minAdmissionDate = result;
                }
            }

            //get terms between when user was admissioned and now
            var terms = await termsService.GetTermsAsync(minAdmissionDate, DateTimeOffset.Now.DateTime);
            if (terms == null || terms.Count == 0)
            {
                popup.CollectionStateKey = StateKey.ConnectionError;
                return;
            }
            List<string> termNames = new();
            foreach (var term in terms) { termNames.Add(term.Name); }

            //update popup
            popup.OnPicked += HandleTermPickedAsync;
            popup.Options = termNames;
            popup.CollectionStateKey = StateKey.Loaded;

            async void HandleTermPickedAsync(PickFromListPopup.PickedItem pickedItem)
            {
                try
                {
                    GradesStateKey = StateKey.Loading;
                    var pickedTerm = terms[pickedItem.Index];

                    //get groups for picked term
                    var groups = await groupsService.GetGroupedGroupsServerAsync(false, false);
                    if (groups is null)
                    {
                        GradesStateKey = StateKey.Empty;
                        return;
                    }
                    await groupsService.SetEctsPointsAsync(groups.Groups);
                    if (groups == null || groups.GroupsGrouped.Count == 0)
                    {
                        GradesStateKey = StateKey.Empty;
                        return;
                    }
                    List<Group>? pickedTermGroups = null;
                    foreach (var group in groups.GroupsGrouped)
                    {
                        if (group.TermName == pickedItem.Value) pickedTermGroups = group.ToList();
                    }
                    if (pickedTermGroups == null)
                    {
                        GradesStateKey = StateKey.Empty;
                        return;
                    }

                    //get grades
                    var grades = await gradesService.GetGradesServerAsync(pickedTermGroups, pickedTerm.Id);
                    if (grades == null)
                    {
                        GradesStateKey = StateKey.Empty;
                        return;
                    }
                    var gradesGrouped = GradesHelper.GroupGrades(grades);

                    //set groups to not modify local database
                    foreach (var groupedGrade in gradesGrouped) groupedGrade.SaveUpdatedGradeDistributionToLocalDatabse = false;

                    applicationService.MainThreadInvoke(() =>
                    {
                        GradesGroups = gradesGrouped.ToObservableCollection();
                        GradeAverage = GradesHelper.CalculateGradeAverage(GradesGroups).ToString();
                        CurrentlyDisplayedTerm = pickedTerm;
                        GradesStateKey = StateKey.Loaded;
                    });
                }
                catch (Exception ex) { logger?.LogCatchedException(ex); GradesStateKey = StateKey.LoadingError; }
            }
        }
        catch (Exception ex) { logger?.LogCatchedException(ex); GradesStateKey = StateKey.LoadingError; }
    }
}