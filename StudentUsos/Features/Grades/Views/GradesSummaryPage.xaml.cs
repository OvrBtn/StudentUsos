using StudentUsos.Controls;

namespace StudentUsos.Features.Grades.Views;

public partial class GradesSummaryPage : CustomContentPageNotAnimated
{
    public GradesSummaryPage(GradesSummaryViewModel gradesSummaryViewModel)
    {
        InitializeComponent();
        BindingContext = gradesSummaryViewModel;
    }
}