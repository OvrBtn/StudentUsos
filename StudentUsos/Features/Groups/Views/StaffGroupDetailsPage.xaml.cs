using StudentUsos.Controls;
using StudentUsos.Features.Activities.Models;

namespace StudentUsos.Features.Groups.Views;

public partial class StaffGroupDetailsPage : CustomBottomSheet
{
    StaffGroupDetailsViewModel staffGroupDetailsViewModel;
    public StaffGroupDetailsPage(StaffGroupDetailsViewModel staffGroupDetailsViewModel)
    {
        InitializeComponent();

        BindingContext = this.staffGroupDetailsViewModel = staffGroupDetailsViewModel;
    }

    public void Init(Activity activity, Action? onClose = null)
    {
        staffGroupDetailsViewModel.Init(activity, onClose);
    }
}