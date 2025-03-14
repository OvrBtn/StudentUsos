using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudentUsos.Features.Settings.Views.NotificationsDiagnosis;

public partial class NotificationsDiagnosisViewModel : BaseViewModel
{
    FirebasePushNotificationsService firebasePushNotificationsService;
    public NotificationsDiagnosisViewModel(FirebasePushNotificationsService firebasePushNotificationsService)
    {
        this.firebasePushNotificationsService = firebasePushNotificationsService;
    }

    public void Init()
    {
        _ = Analyse();

#if ANDROID
        IsBackgroundRestrictionsVisible = true;
        IsAutoStartRestrictionsVisible = true;
        IsbatteryOptimizationRestrictionsVisible = true;
#endif
    }

    [ObservableProperty] bool isAnalyseButtonVisible;
    [ObservableProperty] States fcmTokenInServerState = States.Loading;

    [ObservableProperty] States backgroundRestrictionsState = States.Loading;
    [ObservableProperty] bool isBackgroundRestrictionsVisible;

    [ObservableProperty] States batteryOptimizationRestrictionsState = States.Loading;
    [ObservableProperty] bool isbatteryOptimizationRestrictionsVisible;

    [ObservableProperty] States autoStartRestrictionsState = States.Loading;
    [ObservableProperty] bool isAutoStartRestrictionsVisible;


    [RelayCommand]
    void AnalyseButtonClicked()
    {
        _ = Analyse();
    }

    const int ArtificialDelayMilliseconds = 200;
    async Task Analyse()
    {
        IsAnalyseButtonVisible = false;
        FcmTokenInServerState = States.Loading;

        BackgroundRestrictionsState = States.Loading;
        AutoStartRestrictionsState = States.Loading;
        BatteryOptimizationRestrictionsState = States.Loading;

        await Task.Delay(ArtificialDelayMilliseconds);

        var token = await firebasePushNotificationsService.GetFcmTokenAsync();
        var fcmTokenResult = await firebasePushNotificationsService.SendFcmTokenToServerAsync(token);
        if (fcmTokenResult)
        {
            FcmTokenInServerState = States.Success;
        }
        else
        {
            FcmTokenInServerState = States.Error;
        }

#if ANDROID
        await Task.Delay(ArtificialDelayMilliseconds);

        var isBackgroundRestricted = NotificationsDiagnosisHelper.IsBackgroundRestricted();
        if (isBackgroundRestricted)
        {
            BackgroundRestrictionsState = States.Warning;
        }
        else
        {
            BackgroundRestrictionsState = States.Success;
        }

        await Task.Delay(ArtificialDelayMilliseconds);

        var isBatteryOptimizationEnabled = NotificationsDiagnosisHelper.IsBatteryOptimizationEnabled();
        if (isBatteryOptimizationEnabled)
        {
            BatteryOptimizationRestrictionsState = States.Error;
        }
        else
        {
            BatteryOptimizationRestrictionsState = States.Success;
        }

        await Task.Delay(ArtificialDelayMilliseconds);

        var isAutoStartRestrictedManufacturer = NotificationsDiagnosisHelper.CheckIfManufacturerWithAutoStartPermission();
        if (isAutoStartRestrictedManufacturer)
        {
            AutoStartRestrictionsState = States.Warning;
        }
        else
        {
            AutoStartRestrictionsState = States.Success;
        }
#endif

        IsAnalyseButtonVisible = true;
    }

    [RelayCommand]
    void BackgroundRestrictedButtonClicked()
    {
#if ANDROID
        NotificationsDiagnosisHelper.RequestDisableBackgroundRestrictions();
#endif
    }

    [RelayCommand]
    void BatteryOptimizationRestrictedButtonClicked()
    {
#if ANDROID
        NotificationsDiagnosisHelper.RequestDisableBatteryOptimization();
#endif
    }

    [RelayCommand]
    void AutoStartRestrictedButtonClicked()
    {
#if ANDROID
        NotificationsDiagnosisHelper.OpenAutoStartSettings();
#endif
    }
}
