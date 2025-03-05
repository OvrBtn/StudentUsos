using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Grades.Models;
using StudentUsos.Features.Grades.Repositories;
using StudentUsos.Features.Grades.Services;

namespace StudentUsos.Features.Dashboard.Views
{
    public partial class DashboardGradeViewModel : BaseViewModel
    {
        IGradesService gradesService;
        IGradesRepository gradesRepository;
        IApplicationService applicationService;
        ILogger? logger;
        public DashboardGradeViewModel(IGradesService gradesService,
            IGradesRepository gradesRepository,
            IApplicationService applicationService,
            ILogger? logger = null)
        {
            this.gradesService = gradesService;
            this.gradesRepository = gradesRepository;
            this.applicationService = applicationService;
            this.logger = logger;
        }

        public void Init()
        {
            LatestFinalGradeStateKey = StateKey.Loading;

            applicationService.WorkerThreadInvoke(LoadLatestFinalGradeAsync);
        }

        [ObservableProperty] FinalGrade latestFinalGrade;
        [ObservableProperty] string latestFinalGradeStateKey = StateKey.Loading;

        public event Action OnSynchronousLoadingFinished;
        public event Action OnAsynchronousLoadingFinished;

        const int WebrequestDelay = 1000;

        async Task LoadLatestFinalGradeAsync()
        {
            LatestFinalGradeStateKey = StateKey.Loading;
            try
            {
                FinalGrade local = gradesRepository.GetLatestGrade();
                if (local != null)
                {
                    LatestFinalGrade = local;
                    LatestFinalGradeStateKey = StateKey.Loaded;
                }
                OnSynchronousLoadingFinished?.Invoke();

                if (local is not null)
                {
                    await Task.Delay(WebrequestDelay);
                }

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
                    OnAsynchronousLoadingFinished?.Invoke();
                });
            }
            catch (Exception ex)
            {
                applicationService.MainThreadInvoke(() =>
                {
                    logger?.LogCatchedException(ex);
                    OnSynchronousLoadingFinished?.Invoke();
                    OnAsynchronousLoadingFinished?.Invoke();
                });
            }

        }
    }
}
