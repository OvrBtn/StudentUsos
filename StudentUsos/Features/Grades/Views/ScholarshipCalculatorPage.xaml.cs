using StudentUsos.Controls;

namespace StudentUsos.Features.Grades.Views
{
    public partial class ScholarshipCalculatorPage : CustomContentPageNotAnimated
    {
        ScholarshipCalculatorViewModel scholarshipCalculatorViewModel;
        public ScholarshipCalculatorPage(ScholarshipCalculatorViewModel scholarshipCalculatorViewModel)
        {
            InitializeComponent();
            BindingContext = this.scholarshipCalculatorViewModel = scholarshipCalculatorViewModel;
        }

        public async Task InitAsync(float gradeAverage)
        {
            await scholarshipCalculatorViewModel.InitAsync(gradeAverage);
        }
    }
}