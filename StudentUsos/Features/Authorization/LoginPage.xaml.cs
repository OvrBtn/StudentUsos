using StudentUsos.Controls;

namespace StudentUsos.Features.Authorization;

public partial class LoginPage : CustomContentPageNotAnimated
{
    public LoginPage()
    {
        BindingContext = new LoginViewModel();
        InitializeComponent();
    }
}