using StudentUsos.Controls;

namespace StudentUsos.Features.Authorization.Views;

public partial class InstallationsPage : CustomContentPageNotAnimated
{
    InstallationsViewModel viewModel;
    public InstallationsPage(InstallationsViewModel viewModel)
    {
        BindingContext = this.viewModel = viewModel;
        InitializeComponent();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        viewModel.TaskCompletionSource.TrySetResult(null);
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