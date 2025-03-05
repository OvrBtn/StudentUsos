using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Authorization;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Calendar.Views;
using StudentUsos.Features.Grades.Views;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Groups.Services;
using StudentUsos.Features.UserInfo;

namespace StudentUsos.Features.Dashboard.Views
{
    public partial class DashboardViewModel : BaseViewModel
    {
        public DashboardPage DashboardPage;

        public DashboardActivitiesViewModel DashboardActivitiesViewModel { get; init; }
        public DashboardCalendarViewModel DashboardCalendarViewModel { get; init; }
        public DashboardGradeViewModel DashboardGradeViewModel { get; init; }

        INavigationService navigationService;
        IUserInfoRepository userInfoRepository;
        IUserInfoService userinfoService;
        IGroupsService groupsService;
        IGroupsRepository groupsRepository;
        ILocalStorageManager localStorageManager;
        IApplicationService applicationService;
        ILogger? logger;
        public DashboardViewModel(
            DashboardActivitiesViewModel dashboardActivitiesViewModel,
            DashboardCalendarViewModel dashboardCalendarViewModel,
            DashboardGradeViewModel dashboardGradeViewModel,
            INavigationService navigationService,
            IUserInfoRepository userInfoRepository,
            IUserInfoService userinfoService,
            IGroupsService groupsService,
            IGroupsRepository groupsRepository,
            ILocalStorageManager localStorageManager,
            IApplicationService applicationService,
            ILogger? logger = null)
        {
            this.DashboardActivitiesViewModel = dashboardActivitiesViewModel;
            this.DashboardCalendarViewModel = dashboardCalendarViewModel;
            this.DashboardGradeViewModel = dashboardGradeViewModel;

            this.navigationService = navigationService;
            this.userInfoRepository = userInfoRepository;
            this.userinfoService = userinfoService;
            this.groupsRepository = groupsRepository;
            this.groupsService = groupsService;
            this.localStorageManager = localStorageManager;
            this.applicationService = applicationService;
            this.logger = logger;

            StudentNumberStateKey = UserInfoStateKey = StateKey.Loading;

            Shell.Current.Navigated += (sender, e) =>
            {
                if (e.Previous != null && e.Previous.Location.OriginalString == "//" + nameof(LoginPage)) webrequestDelay = 0;
            };

            AuthorizationService.OnLoginSucceeded += AuthorizationService_OnLoginSucceeded;

            DashboardActivitiesViewModel.OnSynchronousLoadingFinished += SynchronousOperationFinished;
            DashboardActivitiesViewModel.OnAsynchronousLoadingFinished += AsynchronousOperationFinished;

            DashboardCalendarViewModel.OnSynchronousLoadingFinished += SynchronousOperationFinished;
            DashboardCalendarViewModel.OnAsynchronousLoadingFinished += AsynchronousOperationFinished;

            DashboardGradeViewModel.OnSynchronousLoadingFinished += SynchronousOperationFinished;
            DashboardGradeViewModel.OnAsynchronousLoadingFinished += AsynchronousOperationFinished;
        }

        private void AuthorizationService_OnLoginSucceeded()
        {
            _ = InitAsync();
        }

        public void PassPage(DashboardPage dashboardPage)
        {
            this.DashboardPage = dashboardPage;
        }

        [ObservableProperty] string mainContentStateKey = StateKey.Loading;
        [ObservableProperty] string userInfoStateKey = StateKey.Loading;
        [ObservableProperty] string studentNumberStateKey = StateKey.Loading;

        object syncLoadingLock = new();
        /// <summary>
        /// Expected number of synchronous operations to finish before 
        ///changing <see cref="MainContentStateKey"/> to Loaded
        /// </summary>
        int syncLoadingTotal = 4;
        int syncLoadingFinishedCounter = 0;
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
                        syncLoadingFinishedCounter = 0;
                    });
                }
            }
        }
        /// <summary>
        /// Invoked when all methods loading data from local databased finished executing
        /// </summary>
        public static event Action FinishedSynchronousLoading;

        int asyncLoadingTotal = 3;
        int asyncLoadingFinishedCounter = 0;
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
            StudentNumberStateKey = UserInfoStateKey = StateKey.Loading;

            if (Utilities.IsAppRunningForTheFirstTime)
            {
                _ = HandleEmptyLocalDatabaseAsync();
            }

            LoadDashboard();
        }


        async Task HandleEmptyLocalDatabaseAsync()
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
            _ = DashboardCalendarViewModel.InitAsync();
            DashboardGradeViewModel.Init();
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
                await Task.Delay(webrequestDelay);
                var userInfo = await userinfoService.GetUserInfoAsync();
                if (userInfo == null) return;
                HandleUserInfo(userInfo);
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

    }
}