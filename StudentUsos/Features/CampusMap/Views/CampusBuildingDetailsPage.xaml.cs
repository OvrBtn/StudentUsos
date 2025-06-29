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


    public static void CreateAndShow(CampusBuildingDetailsParameters parameters)
    {
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            var popup = new CampusBuildingDetailsPage(parameters);
            App.Current?.Windows[0]?.Page?.ShowPopup(popup);
        });
    }
}