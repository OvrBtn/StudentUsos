using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Groups.Models;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Groups.Services;

namespace StudentUsos.Features.Groups.Views
{
    public partial class GroupsViewModel : BaseViewModel
    {
        IGroupsService groupsService;
        IGroupsRepository groupsRepository;
        IApplicationService applicationService;
        ILogger? logger;
        public GroupsViewModel(IGroupsService groupsService,
            IGroupsRepository groupsRepository,
            IApplicationService applicationService,
            ILogger? logger = null)
        {
            this.groupsService = groupsService;
            this.groupsRepository = groupsRepository;
            this.applicationService = applicationService;
            this.logger = logger;
        }

        public void Init()
        {
            _ = applicationService.WorkerThreadInvoke(async () =>
            {
                GroupsListStateKey = StateKey.Loading;
                await Task.Delay(10);

                LoadGroupLocal();
                await Task.Delay(2000);
                LoadGroupsServerAsync();
            });
        }

        [ObservableProperty] string groupsListStateKey;

        /// <summary>
        /// Collection of groups displayed in View
        /// </summary>
        [ObservableProperty] List<GroupsGrouped> groupsGroupedObservable = new();

        /// <summary>
        /// Load groups from local database
        /// </summary>
        void LoadGroupLocal()
        {
            try
            {
                var groupsGrouped = groupsRepository.GetActiveTermsGroupsGrouped();
                groupsGrouped.Reverse();
                applicationService.MainThreadInvoke(() =>
                {
                    GroupsGroupedObservable = groupsGrouped;
                });
                GroupsListStateKey = StateKey.Loaded;
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                GroupsListStateKey = StateKey.LoadingError;
            }
        }

        /// <summary>
        /// Loading groups from USOS API
        /// </summary>
        async void LoadGroupsServerAsync()
        {
            try
            {
                var groupsServer = await groupsService.GetGroupedGroupsServerAsync(false, true);
                if (groupsServer == null)
                {
                    if (GroupsGroupedObservable.Count == 0) GroupsListStateKey = StateKey.LoadingError;
                    return;
                }
                groupsServer.GroupsGrouped.Reverse();
                await groupsService.SetEctsPointsAsync(groupsServer.Groups);
                if (Utilities.CompareCollections(GroupsGroupedObservable, groupsServer.GroupsGrouped, GroupsGrouped.AreEqual) == false)
                {
                    applicationService.MainThreadInvoke(() => GroupsGroupedObservable = groupsServer.GroupsGrouped);

                    groupsRepository.InsertOrReplace(groupsServer.Terms);
                    groupsRepository.InsertOrReplace(groupsServer.Groups);
                }
                GroupsListStateKey = StateKey.Loaded;
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                if (GroupsGroupedObservable.Count == 0) GroupsListStateKey = StateKey.LoadingError;
            }
        }

    }
}
