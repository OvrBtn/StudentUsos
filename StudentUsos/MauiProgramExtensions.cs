using StudentUsos.Features.AcademicTerms.Repositories;
using StudentUsos.Features.AcademicTerms.Services;
using StudentUsos.Features.Activities.Repositories;
using StudentUsos.Features.Activities.Services;
using StudentUsos.Features.Activities.Views;
using StudentUsos.Features.Authorization;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.Authorization.Views;
using StudentUsos.Features.Calendar.Repositories;
using StudentUsos.Features.Calendar.Services;
using StudentUsos.Features.Calendar.Views;
using StudentUsos.Features.CampusMap.Repositories;
using StudentUsos.Features.CampusMap.Services;
using StudentUsos.Features.CampusMap.Views;
using StudentUsos.Features.Dashboard.Views;
using StudentUsos.Features.Grades.Repositories;
using StudentUsos.Features.Grades.Services;
using StudentUsos.Features.Grades.Views;
using StudentUsos.Features.Groups.Repositories;
using StudentUsos.Features.Groups.Services;
using StudentUsos.Features.Groups.Views;
using StudentUsos.Features.Menu;
using StudentUsos.Features.Payments.Repositories;
using StudentUsos.Features.Payments.Services;
using StudentUsos.Features.Payments.Views;
using StudentUsos.Features.Person.Repositories;
using StudentUsos.Features.Person.Services;
using StudentUsos.Features.Person.Views;
using StudentUsos.Features.SatisfactionSurveys.Services;
using StudentUsos.Features.SatisfactionSurveys.Views;
using StudentUsos.Features.Settings.Views;
using StudentUsos.Features.Settings.Views.NotificationsDiagnosis;
using StudentUsos.Features.Settings.Views.Subpages;
using StudentUsos.Features.StudentProgrammes.Repositories;
using StudentUsos.Features.StudentProgrammes.Services;
using StudentUsos.Features.UserInfo;
using StudentUsos.Services.LocalNotifications;
using StudentUsos.Services.ServerConnection;
using StudentUsos.Views.WhatsNew;

namespace StudentUsos;

internal static partial class MauiProgramExtensions
{
    public static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        bool isGuestMode = Preferences.Get(LocalStorageKeys.IsGuestMode.ToString(), false);

        if (isGuestMode)
        {
            DateAndTimeProvider.SwitchProvider(new GuestDateAndTimeProvider());
        }

        builder.Services.AddSingleton<IApplicationService, ApplicationService>();
        builder.Services.AddSingleton(provider => new Lazy<IApplicationService>(() => provider.GetService<IApplicationService>()!));

        builder.Services.AddSingleton<ILogger, Logger>();

        builder.Services.AddSingleton<INavigationService, NavigationService>();

        builder.Services.AddSingleton<ServerConnectionManager>();
        builder.Services.AddSingleton<GuestServerConnectionManager>();
        builder.Services.AddSingleton<IServerConnectionManager>(sp =>
        {
            if (isGuestMode)
            {
                return new SwitchableServerConnectionManager(sp.GetRequiredService<GuestServerConnectionManager>());
            }
            return new SwitchableServerConnectionManager(sp.GetRequiredService<ServerConnectionManager>());
        });

        builder.Services.AddSingleton(provider => new Lazy<IServerConnectionManager>(() => provider.GetService<IServerConnectionManager>()!));

        builder.Services.AddSingleton<ILocalDatabaseManager, LocalDatabaseManager>();
        builder.Services.AddSingleton(provider => new Lazy<ILocalDatabaseManager>(() => provider.GetService<ILocalDatabaseManager>()!));

        builder.Services.AddSingleton<ILocalStorageManager, LocalStorageManager>();
        builder.Services.AddSingleton(provider => new Lazy<ILocalStorageManager>(() => provider.GetService<ILocalStorageManager>()!));

        builder.Services.AddSingleton<ILocalNotificationsService, LocalNotificationsService>();

        builder.Services.AddSingleton<IActivitiesRepository, ActivitiesRepository>();
        builder.Services.AddSingleton<IActivitiesService, ActivitiesService>();

        builder.Services.AddSingleton<IUserInfoRepository, UserInfoRepository>();
        builder.Services.AddSingleton(provider => new Lazy<IUserInfoRepository>(() => provider.GetService<IUserInfoRepository>()!));
        builder.Services.AddSingleton<IUserInfoService, UserInfoService>();

        builder.Services.AddSingleton<IGroupsService, GroupsService>();
        builder.Services.AddSingleton<IGroupsRepository, GroupsRepository>();

        builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
        builder.Services.AddSingleton<IPaymentsService, PaymentsService>();

        builder.Services.AddSingleton<IGradesService, GradesService>();
        builder.Services.AddSingleton<IGradesRepository, GradesRepository>();

        builder.Services.AddSingleton<IUsosCalendarService, UsosCalendarService>();
        builder.Services.AddSingleton<IGoogleCalendarService, GoogleCalendarService>();
        builder.Services.AddSingleton<IUsosCalendarRepository, UsosCalendarRepository>();
        builder.Services.AddSingleton<IGoogleCalendarRepository, GoogleCalendarRepository>();

        builder.Services.AddSingleton<ILecturerService, LecturerService>();
        builder.Services.AddSingleton<ILecturerRepository, LecturerRepository>();

        builder.Services.AddSingleton<ISatisfactionSurveysService, SatisfactionSurveysService>();

        builder.Services.AddSingleton<IStudentProgrammeService, StudentProgrammeService>();
        builder.Services.AddSingleton<IStudentProgrammeRepository, StudentProgrammeRepository>();

        builder.Services.AddSingleton<ITermsService, TermsService>();
        builder.Services.AddSingleton<ITermsRepository, TermsRepository>();

        builder.Services.AddSingleton<ICampusMapService, CampusMapService>();
        builder.Services.AddSingleton<ICampusMapRepository, CampusMapRepository>();

#if ANDROID
        builder.Services.AddSingleton<IBackgroundJobService, BackgroundJobServiceAndroid>();
#endif

        builder.Services.AddSingleton<IUsosInstallationsService, UsosInstallationsService>();
        builder.Services.AddSingleton(provider => new Lazy<IUsosInstallationsService>(() => provider.GetService<IUsosInstallationsService>()!));

        return builder;
    }

    public static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddTransient<ActivitiesPage>();
        builder.Services.AddSingleton<MorePage>();
        builder.Services.AddSingleton<LoginPage>();

        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<CalendarSettingsPage>();

        builder.Services.AddTransient<GradeDetailsPage>();
        builder.Services.AddTransient<GradesPage>();
        builder.Services.AddTransient<ModifyGradePage>();
        builder.Services.AddTransient<ScholarshipCalculatorPage>();
        builder.Services.AddTransient<GradesSummaryPage>();

        builder.Services.AddTransient<GroupDetailsPage>();
        builder.Services.AddTransient<GroupsPage>();
        builder.Services.AddTransient<StaffGroupDetailsPage>();

        builder.Services.AddTransient<PaymentsPage>();

        builder.Services.AddTransient<PersonDetailsPage>();

        builder.Services.AddTransient<FillSatisfactionSurveyPage>();
        builder.Services.AddTransient<SatisfactionSurveysPage>();

        builder.Services.AddTransient<LogsPage>();

        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<NotificationsSubpage>();
        builder.Services.AddTransient<AccountSubpage>();
        builder.Services.AddTransient<ApplicationSubpage>();
        builder.Services.AddTransient<DiagnosisSubpage>();
        builder.Services.AddTransient<AppInfoPage>();
        builder.Services.AddTransient<NotificationsDiagnosisPage>();

        builder.Services.AddTransient<WhatsNewListPage>();
        builder.Services.AddTransient<WhatsNewCarouselPage>();

        builder.Services.AddTransient<CampusMapPage>();

        builder.Services.AddTransient<InstallationsPage>();
        builder.Services.AddTransient<LoginResultPage>();

        return builder;
    }

    public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<DashboardActivitiesViewModel>();
        builder.Services.AddSingleton<DashboardCalendarViewModel>();
        builder.Services.AddSingleton<DashboardGradeViewModel>();

        builder.Services.AddTransient<ActivitiesViewModel>();
        builder.Services.AddSingleton<MoreViewModel>();

        builder.Services.AddSingleton<LoginViewModel>();

        builder.Services.AddTransient<CalendarSettingsViewModel>();
        builder.Services.AddTransient<CalendarViewModel>();

        builder.Services.AddTransient<GradeDetailsViewModel>();
        builder.Services.AddTransient<GradesViewModel>();
        builder.Services.AddTransient<ModifyGradeViewModel>();
        builder.Services.AddTransient<ScholarshipCalculatorViewModel>();
        builder.Services.AddTransient<GradesSummaryViewModel>();

        builder.Services.AddTransient<GroupDetailsViewModel>();
        builder.Services.AddTransient<GroupsViewModel>();
        builder.Services.AddTransient<StaffGroupDetailsViewModel>();

        builder.Services.AddTransient<PaymentsViewModel>();

        builder.Services.AddTransient<FillSatisfactionSurveyViewModel>();
        builder.Services.AddTransient<SatisfactionSurveysViewModel>();

        builder.Services.AddTransient<PersonDetailsViewModel>();

        builder.Services.AddTransient<LogsViewModel>();

        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<NotificationsSubpageViewModel>();
        builder.Services.AddTransient<ApplicationSubpageViewModel>();
        builder.Services.AddTransient<NotificationsDiagnosisViewModel>();

        builder.Services.AddTransient<CampusMapViewModel>();

        builder.Services.AddTransient<InstallationsViewModel>();
        builder.Services.AddTransient<LoginResultViewModel>();

        return builder;
    }
}