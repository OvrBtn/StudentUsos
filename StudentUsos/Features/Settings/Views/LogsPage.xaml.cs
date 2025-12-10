using StudentUsos.Controls;

namespace StudentUsos.Features.Settings.Views;

public partial class LogsPage : CustomContentPageNotAnimated, INavigableWithParameter<bool>
{
    LogsViewModel viewModel;
    public LogsPage(LogsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    bool isViewModelSet = false;
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (isViewModelSet)
        {
            return;
        }
        isViewModelSet = true;
        Dispatcher.Dispatch(() =>
        {
            viewModel.Init();
        });
    }

    public void OnNavigated(bool navigationParameter)
    {
        this.IsTabBarVisible = navigationParameter;
        this.removeButton.IsVisible = false;
        this.sendButton.IsVisible = false;
    }
}