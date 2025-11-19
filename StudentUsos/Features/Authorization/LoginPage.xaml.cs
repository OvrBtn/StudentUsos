using StudentUsos.Controls;

namespace StudentUsos.Features.Authorization;

public partial class LoginPage : CustomContentPageNotAnimated
{
    public LoginPage(LoginViewModel loginViewModel)
    {
        BindingContext = loginViewModel;
        InitializeComponent();
    }
}