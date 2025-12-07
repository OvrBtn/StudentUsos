using StudentUsos.Controls;
using StudentUsos.Features.Authorization.Views;

namespace StudentUsos.Features.Authorization;

public partial class LoginPage : CustomContentPageNotAnimated
{
    INavigationService navigationService;
    IApplicationService applicationService;
    public LoginPage(LoginViewModel loginViewModel, INavigationService navigationService, IApplicationService applicationService)
    {
        BindingContext = loginViewModel;
        InitializeComponent();

        this.navigationService = navigationService;
        this.applicationService = applicationService;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        LoginSettingsPage page = new(navigationService, applicationService);
        page.ShowPopup();
    }
}