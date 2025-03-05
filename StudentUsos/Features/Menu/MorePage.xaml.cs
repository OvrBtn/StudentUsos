using StudentUsos.Controls;

namespace StudentUsos.Features.Menu;

public partial class MorePage : CustomContentPageNotAnimated
{
    public MorePage(MoreViewModel moreViewModel)
    {
        BindingContext = moreViewModel;
        InitializeComponent();
    }
}