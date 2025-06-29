using CommunityToolkit.Maui.Views;

namespace StudentUsos.Features.CampusMap.Views;

public partial class RoomDetailsPage : Popup
{
    EntryPopupViewModel viewModel;
    public RoomDetailsPage(RoomDetailsParameters parameters)
    {
        InitializeComponent();

        BindingContext = new RoomDetailsViewModel(parameters);
    }


    public static void CreateAndShow(RoomDetailsParameters parameters)
    {
        ApplicationService.Default.MainThreadInvoke(() =>
        {
            var popup = new RoomDetailsPage(parameters);
            App.Current?.Windows[0]?.Page?.ShowPopup(popup);
        });
    }
}