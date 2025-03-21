using StudentUsos.Controls;
using StudentUsos.Views.WhatsNew;

namespace StudentUsos.Features.Settings.Views.Subpages;

public partial class ApplicationSubpage : CustomContentPageNotAnimated
{
    INavigationService navigationService;
    public ApplicationSubpage(ApplicationSubpageViewModel applicationSubpageViewModel, INavigationService navigationService)
    {
        InitializeComponent();
        BindingContext = applicationSubpageViewModel;
        this.navigationService = navigationService;
    }

    private async void AppInfoButton_Clicked(object sender, EventArgs e)
    {
        await navigationService.PushAsync<AppInfoPage>();
    }

    private async void FeaturedChangesButton_Clicked(object sender, EventArgs e)
    {
        await navigationService.PushModalAsync<WhatsNewCarouselPage>();
    }

    private async void GeneralChangesButton_Clicked(object sender, EventArgs e)
    {
        await navigationService.PushModalAsync<WhatsNewListPage>();
    }
}