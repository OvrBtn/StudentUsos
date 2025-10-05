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

    protected override Task OnClosed(object? result, bool wasDismissedByTappingOutsideOfPopup, CancellationToken token = default)
    {
        isPopupActive = false; ;
        return base.OnClosed(result, wasDismissedByTappingOutsideOfPopup, token);
    }

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
            App.Current?.Windows[0]?.Page?.ShowPopup(popup);
        });
    }
}