using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Services.ServerConnection;

namespace StudentUsos.Features.Grades.Views;

public partial class GradesSummaryViewModel : BaseViewModel
{
    IServerConnectionManager serverConnectionManager;
    ILocalStorageManager localStorageManager;
    ILogger? logger;
    public GradesSummaryViewModel(IServerConnectionManager serverConnectionManager,
        ILocalStorageManager localStorageManager,
        ILogger? logger = null)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.localStorageManager = localStorageManager;
        this.logger = logger;

        SetEctsPointsSumAsync();
    }

    [ObservableProperty] string mainStateKey = StateKey.Loading;
    [ObservableProperty] string ectsPointsSum;


    async void SetEctsPointsSumAsync()
    {
        if (localStorageManager.TryGettingString(LocalStorageKeys.EctsPointsSum, out string result))
        {
            EctsPointsSum = result;
            MainStateKey = StateKey.Loaded;
        }
        var pointsApi = await GetEctsPointsSumAsync();
        if (pointsApi != null && pointsApi != EctsPointsSum)
        {
            EctsPointsSum = pointsApi;
            localStorageManager.SetString(LocalStorageKeys.EctsPointsSum, pointsApi);
            MainStateKey = StateKey.Loaded;
        }
    }

    async Task<string?> GetEctsPointsSumAsync()
    {
        try
        {
            var sum = await serverConnectionManager.SendRequestToUsosAsync("services/credits/used_sum", new Dictionary<string, string>());
            if (sum == null) return null;

            //removing the fractional part
            int indexOfDot = sum.IndexOf('.');
            sum = sum.Remove(indexOfDot);

            return sum;
        }
        catch (Exception ex) { logger?.LogCatchedException(ex); return null; }
    }
}