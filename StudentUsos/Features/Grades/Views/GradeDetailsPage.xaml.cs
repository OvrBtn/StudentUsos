using StudentUsos.Controls;
using StudentUsos.Features.Grades.Models;

namespace StudentUsos.Features.Grades.Views
{
    public partial class GradeDetailsPage : CustomBottomSheet
    {
        GradeDetailsViewModel gradeDetailsViewModel;
        public GradeDetailsPage(GradeDetailsViewModel gradeDetailsViewModel)
        {
            InitializeComponent();
            BindingContext = this.gradeDetailsViewModel = gradeDetailsViewModel;
        }

        public void Init(FinalGradeGroup finalGradeGroup)
        {
            gradeDetailsViewModel.InitAsync(finalGradeGroup);
            this.ShowPopup();
        }
    }
}