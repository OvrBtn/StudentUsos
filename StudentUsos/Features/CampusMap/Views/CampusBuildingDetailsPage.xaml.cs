using CommunityToolkit.Maui.Extensions;
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
            popup.Closed += Popup_Closed;
        });
    }

    private static void Popup_Closed(object? sender, EventArgs e)
    {
        isPopupActive = false;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Parameters.GoToBuildingMap?.Invoke();
        App.Current?.Windows[0]?.Page?.ClosePopupAsync(false);
    }
}