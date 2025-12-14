using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

namespace StudentUsos.Features.CampusMap.Views;

public partial class RoomDetailsPage : Popup
{
    public RoomDetailsPage(RoomDetailsParameters parameters)
    {
        InitializeComponent();

        BindingContext = new RoomDetailsViewModel(parameters);
    }

    static bool isPopupActive = false;

    public static void CreateAndShow(RoomDetailsParameters parameters)
    {
        if (isPopupActive)
        {
            return;
        }

        isPopupActive = true;
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            var popup = new RoomDetailsPage(parameters);
            popup.Closed += Popup_Closed;
            App.Current?.Windows[0]?.Page?.ShowPopup(popup);
        });
    }

    private static void Popup_Closed(object? sender, EventArgs e)
    {
        isPopupActive = false;
    }
}