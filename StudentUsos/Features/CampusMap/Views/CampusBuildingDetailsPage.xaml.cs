using CommunityToolkit.Maui.Views;

namespace StudentUsos.Features.CampusMap.Views;

public partial class CampusBuildingDetailsPage : Popup
{
    public CampusBuildingDetailsParameters Parameters { get; private set; }

    public CampusBuildingDetailsPage(CampusBuildingDetailsParameters parameters)
    {
        InitializeComponent();

        Parameters = parameters;
        BindingContext = this;
    }

    static bool isPopupActive = false;
    protected override Task OnClosed(object? result, bool wasDismissedByTappingOutsideOfPopup, CancellationToken token = default)
    {
        isPopupActive = false;
        return base.OnClosed(result, wasDismissedByTappingOutsideOfPopup, token);
    }

    public static void CreateAndShow(CampusBuildingDetailsParameters parameters)
    {
        if (isPopupActive)
        {
            return;
        }
        isPopupActive = true;

        ApplicationService.Default.MainThreadInvoke(() =>
        {
            var popup = new CampusBuildingDetailsPage(parameters);
            App.Current?.Windows[0]?.Page?.ShowPopup(popup);
        });
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Parameters.GoToBuildingMap?.Invoke();
        Close();
    }
}