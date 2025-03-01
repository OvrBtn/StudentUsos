using StudentUsos.Controls;

namespace StudentUsos.Features.Groups.Views
{
    public partial class GroupsPage : CustomContentPageNotAnimated
    {
        GroupsViewModel viewModel;
        public GroupsPage(GroupsViewModel groupsViewModel)
        {
            BindingContext = viewModel = groupsViewModel;
            InitializeComponent();
        }

        bool isViewModelSet = false;
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (isViewModelSet)
            {
                return;
            }
            Dispatcher.Dispatch(() =>
            {
                isViewModelSet = true;
                viewModel.Init();
            });
        }
    }
}