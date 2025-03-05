using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Calendar;
using StudentUsos.Features.Calendar.Models;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services;
using StudentUsos.Features.Calendar.Views;
using StudentUsos.Features.Dashboard.Models;
using StudentUsos.Features.Grades.Models;
using StudentUsos.Features.Grades.Repositories;
using StudentUsos.Features.Grades.Services;
using StudentUsos.Features.Grades.Views;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Groups.Services;
using StudentUsos.Features.UserInfo;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.Dashboard.Views
{
    //TODO: split into separate viewmodels to minimize amount of dependencies and allow testing

    public partial class DashboardViewModel : BaseViewModel
    {
        public DashboardPage DashboardPage;

        public DashboardActivitiesViewModel DashboardActivitiesViewModel { get; init; }

        INavigationService navigationService;
        IUserInfoRepository userInfoRepository;
        IUserInfoService userinfoService;
        IGroupsService groupsService;
        IGroupsRepository groupsRepository;
        IGradesService gradesService;
        IGradesRepository gradesRepository;
        IUsosCalendarService usosCalendarService;
        IUsosCalendarRepository usosCalendarRepository;
        IGoogleCalendarService googleCalendarService;
        IGoogleCalendarRepository googleCalendarRepository;
        ILocalStorageManager localStorageManager;
        IApplicationService applicationService;
        ILogger? logger;
        public DashboardViewModel(
            DashboardActivitiesViewModel dashboardActivitiesViewModel,
            INavigationService navigationService,
            IUserInfoRepository userInfoRepository,
            IUserInfoService userinfoService,
            IGroupsService groupsService,
            IGroupsRepository groupsRepository,
            IGradesService gradesService,
            IGradesRepository gradesRepository,
            IUsosCalendarService usosCalendarService,
            IUsosCalendarRepository usosCalendarRepository,
            IGoogleCalendarService googleCalendarService,
            IGoogleCalendarRepository googleCalendarRepository,
            ILocalStorageManager localStorageManager,
            IApplicationService applicationService,
            ILogger? logger = null)
        {
            this.DashboardActivitiesViewModel = dashboardActivitiesViewModel;

            this.navigationService = navigationService;
            this.userInfoRepository = userInfoRepository;
            this.userinfoService = userinfoService;
            this.groupsRepository = groupsRepository;
            this.groupsService = groupsService;
            this.gradesService = gradesService;
            this.gradesRepository = gradesRepository;
            this.usosCalendarService = usosCalendarService;
            this.usosCalendarRepository = usosCalendarRepository;
            this.googleCalendarService = googleCalendarService;
            this.googleCalendarRepository = googleCalendarRepository;
            this.localStorageManager = localStorageManager;
            this.applicationService = applicationService;
            this.logger = logger;

            instance = this;

            CalendarStateKey = GoogleCalendarStateKey = StudentNumberStateKey = UserInfoStateKey = LatestFinalGradeStateKey = StateKey.Loading;

            Shell.Current.Navigated += (sender, e) =>
            {
                if (e.Previous != null && e.Previous.Location.OriginalString == "//" + nameof(LoginPage)) webrequestDelay = 0;
            };

            AuthorizationService.OnLogout += AuthorizationService_OnLogout;
            AuthorizationService.OnLoginSucceeded += AuthorizationService_OnLoginSucceeded;

            DashboardActivitiesViewModel.OnSynchronousLoadingFinished += SynchronousOperationFinished;
            DashboardActivitiesViewModel.OnAsynchronousLoadingFinished += AsynchronousOperationFinished;
        }

        static DashboardViewModel? instance;
        private void AuthorizationService_OnLoginSucceeded()
        {
            if (instance != null)
            {
                _ = instance.InitAsync();
            }
        }

        private void AuthorizationService_OnLogout()
        {
            if (instance != null)
            {
                instance.CalendarStateKey = instance.GoogleCalendarStateKey = instance.StudentNumberStateKey = instance.UserInfoStateKey = instance.LatestFinalGradeStateKey = StateKey.Loading;
            }
        }

        public void PassPage(DashboardPage dashboardPage)
        {
            this.DashboardPage = dashboardPage;
        }

        [ObservableProperty] string mainContentStateKey = StateKey.Loading;
        [ObservableProperty] string userInfoStateKey = StateKey.Loading;
        [ObservableProperty] string studentNumberStateKey = StateKey.Loading;
        [ObservableProperty] string calendarStateKey = StateKey.Loading;
        [ObservableProperty] string googleCalendarStateKey = StateKey.Loading;

        object syncLoadingLock = new();
        /// <summary>
        /// Expected number of synchronous operations to finish before 
        ///changing <see cref="MainContentStateKey"/> to Loaded
        /// </summary>
        int syncLoadingTotal = 4;
        int syncLoadingFinishedCounter = 0;
        void RegisterSynchronousOperation()
        {
            //lock (syncLoadingTotalLock)
            //{
            //    syncLoadingTotal++;
            //}
        }
        void SynchronousOperationFinished()
        {
            lock (syncLoadingLock)
            {
                syncLoadingFinishedCounter++;
                if (syncLoadingFinishedCounter == syncLoadingTotal)
                {
                    applicationService.MainThreadInvoke(() =>
                    {
                        FinishedSynchronousLoading?.Invoke();
                        MainContentStateKey = StateKey.Loaded;
                    });
                }
            }
        }
        /// <summary>
        /// Invoked when all methods loading data from local databased finished executing
        /// </summary>
        public static event Action FinishedSynchronousLoading;

        /// <summary>
        /// Used to update all UI elements at the same time when they are all loaded from local database
        /// </summary>
        /// <param name="action"></param>
        void ExecuteOnceWhenSynchronousLoadingFinished(Action action)
        {
            if (action == null) return;
            Action handler = null;
            handler = () =>
            {
                FinishedSynchronousLoading -= handler;
                applicationService.MainThreadInvoke(() =>
                {
                    action.Invoke();
                });

            };
            FinishedSynchronousLoading += handler;
        }

        int asyncLoadingTotal = 3;
        int asyncLoadingFinishedCounter = 0;
        void RegisterAsynchronousOperation()
        {
            //applicationService.MainThreadInvoke(() => asyncLoadingTotal++);
        }
        void AsynchronousOperationFinished()
        {
            applicationService.MainThreadInvoke(() =>
            {
                asyncLoadingFinishedCounter++;
                if (asyncLoadingFinishedCounter == asyncLoadingTotal)
                {
                    FinishedAsynchronousLoading?.Invoke();
                }
            });
        }
        /// <summary>
        /// Invoked when all methods loading data from USOS API finished executing
        /// </summary>
        public event Action FinishedAsynchronousLoading;


        public async Task InitAsync()
        {


            if (Utilities.IsAppRunningForTheFirstTime)
            {
                _ = HandleEmptyLocalDatabase();
            }


            LoadDashboard();
        }


        async Task HandleEmptyLocalDatabase()
        {
            localStorageManager.SetData(LocalStorageKeys.IsAppRunningForTheFirstTime, false.ToString());

            var groupsServer = await groupsService.GetGroupedGroupsServerAsync(false, true);
            if (groupsServer == null)
            {
                return;
            }
            groupsServer.GroupsGrouped.Reverse();
            await groupsService.SetEctsPointsAsync(groupsServer.Groups);
            groupsRepository.InsertOrReplace(groupsServer.Terms);
            groupsRepository.InsertOrReplace(groupsServer.Groups);
        }

        [RelayCommand]
        void OpenGradesPage()
        {
            navigationService.PushAsync<GradesPage>(false);
        }

        [RelayCommand]
        void OpenCalendarPage()
        {
            navigationService.PushAsync<CalendarPage>(false);
        }

        /// <summary>
        /// Webrequest delay in miliseconds
        /// </summary>
        int webrequestDelay = 1000;

        void LoadDashboard()
        {
            LoadUserInfo();
            applicationService.WorkerThreadInvoke(DashboardActivitiesViewModel.Init);
            _ = LoadCalendarEvents();
            LoadLatestFinalGrade();
        }

        [ObservableProperty] string firstName;
        [ObservableProperty] string lastName;
        [ObservableProperty] string indexNumber;
        [ObservableProperty] string libraryCardId;

        public void LoadUserName()
        {
            if (localStorageManager.TryGettingData(LocalStorageKeys.UserName, out string name))
            {
                FirstName = name;
                UserInfoStateKey = StateKey.Loaded;
            }
        }

        public void LoadUserInfo()
        {
            LoadUserInfoFromLocalDatabase();
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(IndexNumber)
                || UserInfoStateKey == StateKey.Loading)
            {
                _ = LoadUserInfoFromApiAsync();
            }
        }

        void LoadUserInfoFromLocalDatabase()
        {
            try
            {
                RegisterSynchronousOperation();
                var userInfo = userInfoRepository.GetUserInfo();
                if (userInfo != null)
                {
                    HandleUserInfo(userInfo);
                }
                SynchronousOperationFinished();

            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
            }
        }

        async Task LoadUserInfoFromApiAsync()
        {
            try
            {
                //RegisterAsynchronousOperation();
                await Task.Delay(webrequestDelay);
                var userInfo = await userinfoService.GetUserInfoAsync();
                if (userInfo == null) return;
                HandleUserInfo(userInfo);
                //AsynchronousOperationFinished();
                userInfoRepository.SaveUserInfo(userInfo);
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                if (string.IsNullOrEmpty(FirstName)) UserInfoStateKey = StudentNumberStateKey = StateKey.LoadingError;
            }
        }

        void HandleUserInfo(UserInfo.UserInfo userInfo)
        {
            try
            {
                FirstName = userInfo.FirstName;
                localStorageManager.SetData(LocalStorageKeys.UserName, FirstName);
                LastName = userInfo.LastName;
                IndexNumber = userInfo.StudentNumber;
                UserInfoStateKey = StudentNumberStateKey = StateKey.Loaded;
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                UserInfoStateKey = StudentNumberStateKey = StateKey.LoadingError;
            }
        }

        #region Activities



        #endregion

        #region Calendar

        [ObservableProperty] ObservableCollection<UsosCalendarEvent> events = new();
        [ObservableProperty] ObservableCollection<GoogleCalendarEvent> eventsGoogle = new();

        [ObservableProperty] ObservableCollection<CalendarEvent> calendarEvents = new();

        const int MaxCalendarEvents = 5;
        async Task LoadCalendarEvents()
        {
            try
            {
                RegisterSynchronousOperation();
                RegisterAsynchronousOperation();

                CalendarEvents.Clear();

                LoadCalendarEventsLocal();
                SynchronousOperationFinished();

                await Task.Delay(webrequestDelay);

                await LoadCalendarEventsServerAsync();
                AsynchronousOperationFinished();
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
            }
        }

        async Task LoadCalendarEventsServerAsync()
        {
            var usosEvents = await usosCalendarService.TryFetchingAvailableEventsAsync();
            List<UsosCalendarEvent> usosEventsLinearized;
            if (usosEvents == null)
            {
                usosEventsLinearized = usosCalendarRepository.GetAllEvents();
            }
            else
            {
                usosEventsLinearized = usosEvents.SelectMany(x => x.events).ToList();
                foreach (var item in usosEvents)
                {
                    await usosCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(item.date.Year, item.date.Month, item.events, item.isPrimaryUpdate);
                }

                if (usosEvents.Count != CalendarSettings.MonthsToGetInTotal)
                {
                    var date = DateTimeOffset.Now.DateTime;
                    for (int i = 0; i < CalendarSettings.MonthsToGetInTotal; i++)
                    {
                        if (usosEvents.Any(x => x.date.Year == date.Year && x.date.Month == date.Month) == false)
                        {
                            usosEventsLinearized.AddRange(usosCalendarRepository.GetEvents(date.Year, date.Month));
                        }
                        date = date.AddMonths(1);
                    }
                    Utilities.RemoveDuplicates(usosEventsLinearized, UsosCalendarEvent.AreEqual);
                }
            }

            List<GoogleCalendarEvent> googleCalendarEventsLinearized = new();
            var googleCalendars = googleCalendarRepository.GetAllCalendars();
            foreach (var calendar in googleCalendars)
            {
                var googleEvents = await googleCalendarService.GetGoogleCalendarEventsAsync(calendar);
                if (googleEvents == null)
                {
                    googleCalendarEventsLinearized = googleCalendarRepository.GetAllEvents().Where(x => x.CalendarName == calendar.Name).ToList();
                }
                else
                {
                    await googleCalendarRepository.SaveEventsFromServerAndHandleLocalNotificationsAsync(googleEvents);
                    googleCalendarEventsLinearized.AddRange(googleEvents);
                }
            }

            var grouped = GroupCalendarEvents(usosEventsLinearized, googleCalendarEventsLinearized);
            SetCalendarEvents(grouped);
        }

        void LoadCalendarEventsLocal()
        {
            var usosEventsLocal = usosCalendarRepository.GetAllEvents();
            var googleCalendarEventsLocal = googleCalendarRepository.GetAllEvents();

            var events = GroupCalendarEvents(usosEventsLocal, googleCalendarEventsLocal);
            ExecuteOnceWhenSynchronousLoadingFinished(() => SetCalendarEvents(events));
        }

        List<CalendarEvent> GroupCalendarEvents(List<UsosCalendarEvent> usosEvents, List<GoogleCalendarEvent> googleEvents)
        {
            List<CalendarEvent> calendarEvents = new();

            for (int i = 0; i < usosEvents.Count; i++)
            {
                calendarEvents.Add(new(usosEvents[i]));
                if (i > MaxCalendarEvents * 2)
                {
                    break;
                }
            }

            var now = DateTimeOffset.Now.DateTime;
            for (int i = 0; i < googleEvents.Count; i++)
            {
                if (googleEvents[i].Start < now && googleEvents[i].End < now)
                {
                    continue;
                }
                calendarEvents.Add(new(googleEvents[i]));
                if (calendarEvents.Count > MaxCalendarEvents * 4)
                {
                    break;
                }
            }

            calendarEvents = calendarEvents.Where(x => x.StartDateTime >= now || now <= x.EndDateTime)
                .OrderBy(x => x.StartDateTime)
                .Take(MaxCalendarEvents).ToList();
            return calendarEvents;
        }

        void SetCalendarEvents(List<CalendarEvent> calendarEvents)
        {
            if (Utilities.CompareCollections(CalendarEvents, calendarEvents, CalendarEvent.AreEqual) == false)
            {
                applicationService.MainThreadInvoke(() =>
                {
                    CalendarEvents = calendarEvents.ToObservableCollection();
                });
            }
            if (CalendarEvents.Count > 0) CalendarStateKey = StateKey.Loaded;
        }

        #endregion

        #region LatestFinalGrade

        [ObservableProperty] FinalGrade latestFinalGrade;
        [ObservableProperty] string latestFinalGradeStateKey = StateKey.Loading;

        void LoadLatestFinalGrade()
        {
            RegisterSynchronousOperation();
            LatestFinalGradeStateKey = StateKey.Loading;
            Task task = applicationService.WorkerThreadInvoke(async () =>
            {
                try
                {
                    RegisterAsynchronousOperation();

                    FinalGrade local = gradesRepository.GetLatestGrade();
                    if (local != null)
                    {
                        ExecuteOnceWhenSynchronousLoadingFinished(() =>
                        {
                            LatestFinalGrade = local;
                            LatestFinalGradeStateKey = StateKey.Loaded;
                        });
                    }
                    SynchronousOperationFinished();

                    await Task.Delay(webrequestDelay);
                    var server = await gradesService.GetLatestGradeServerAsync();
                    applicationService.MainThreadInvoke(() =>
                    {
                        if (server != null && server.IsEmpty)
                        {
                            LatestFinalGradeStateKey = StateKey.Empty;
                        }
                        else if (server != null && server.IsCourseExamAndGradeEqual(local) == false)
                        {
                            LatestFinalGrade = server;
                            LatestFinalGradeStateKey = StateKey.Loaded;
                        }
                        else if (LatestFinalGrade == null) LatestFinalGradeStateKey = StateKey.LoadingError;
                        AsynchronousOperationFinished();
                    });
                }
                catch (Exception ex)
                {
                    applicationService.MainThreadInvoke(() =>
                    {
                        logger?.LogCatchedException(ex);
                        AsynchronousOperationFinished();
                        SynchronousOperationFinished();
                    });
                }
            });

        }

        #endregion

    }
}