using StudentUsos.Controls;

namespace StudentUsos.Features.CampusMap.Views;

public partial class SearchPage : CustomContentPageNotAnimated
{
    SearchViewModel viewModel;
    public SearchPage(SearchViewModel viewModel)
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

    protected override bool OnBackButtonPressed()
    {
        viewModel.Cancel();
        return base.OnBackButtonPressed();
    }
}