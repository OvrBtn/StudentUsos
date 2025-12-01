using StudentUsos.Controls;
using StudentUsos.Features.Authorization.Services;

namespace StudentUsos.Features.Menu;

public partial class MorePage : CustomContentPageNotAnimated
{
    public MorePage(MoreViewModel moreViewModel, IUsosInstallationsService usosInstallationsService)
    {
        BindingContext = moreViewModel;
        InitializeComponent();

        const string campusMapAllowedInstallation = "https://usosapps.put.poznan.pl/";
        var installation = usosInstallationsService.GetCurrentInstallation();
        if (installation != campusMapAllowedInstallation)
        {
            campusMapButton.IsVisible = false;
        }
    }
}