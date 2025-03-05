using StudentUsos.Controls;
using StudentUsos.Features.Groups.Models;

namespace StudentUsos.Features.Groups.Views;

public partial class GroupDetailsPage : CustomBottomSheet
{
    GroupDetailsViewModel groupDetailsViewModel;
    public GroupDetailsPage(GroupDetailsViewModel groupDetailsViewModel)
    {
        InitializeComponent();

        BindingContext = this.groupDetailsViewModel = groupDetailsViewModel;
    }

    public void Init(Group group, Action? onClose = null)
    {
        groupDetailsViewModel.Init(group, onClose);
    }
}