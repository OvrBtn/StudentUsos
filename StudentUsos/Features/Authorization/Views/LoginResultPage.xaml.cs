using StudentUsos.Controls;

namespace StudentUsos.Features.Authorization.Views;

public partial class LoginResultPage : CustomContentPageNotAnimated
{
    LoginResultViewModel viewModel;
    public LoginResultPage(LoginResultViewModel viewModel)
    {
        BindingContext = this.viewModel = viewModel;
        InitializeComponent();
    }

    bool isViewModelSet = false;
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (isViewModelSet) return;
        Dispatcher.Dispatch(async () =>
        {
            isViewModelSet = true;
            await viewModel.InitAsync();
        });

    }
}