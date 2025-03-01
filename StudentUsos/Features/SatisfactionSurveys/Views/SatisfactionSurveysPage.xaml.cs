using StudentUsos.Controls;

namespace StudentUsos.Features.SatisfactionSurveys.Views
{
    public partial class SatisfactionSurveysPage : CustomContentPageNotAnimated
    {
        public SatisfactionSurveysPage(SatisfactionSurveysViewModel satisfactionSurveysViewModel)
        {
            BindingContext = satisfactionSurveysViewModel;
            InitializeComponent();
        }
    }
}