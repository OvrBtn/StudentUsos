using System.Globalization;
using StudentUsos.Controls;

namespace StudentUsos.Features.Grades.Views;

public partial class GradesPage : CustomContentPageNotAnimated
{
    GradesViewModel gradesViewModel;
    INavigationService navigationService;
    public GradesPage(INavigationService navigationService, GradesViewModel gradesViewModel)
    {
        this.navigationService = navigationService;
        BindingContext = this.gradesViewModel = gradesViewModel;
        InitializeComponent();
    }

    bool isViewModelSet = false;
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (isViewModelSet == false)
        {
            isViewModelSet = true;
            gradesViewModel.Init();
        }
    }

    private void FloatingButton_ButtonClicked()
    {
        var startingRotation = floatingButtonImage.Rotation;
        Utilities.Animate(this, (progress) =>
        {
            floatingButtonImage.Rotation = startingRotation + progress * 45;
        }, 300, Easing.Linear);
    }

    private async void ScholarshipCalculatorButton_OnClick()
    {
        string averageString = gradesViewModel.GradeAverage;
        float parsed = 0;
        float.TryParse(averageString.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
        var page = await navigationService.PushAsync<ScholarshipCalculatorPage>();
        await page.InitAsync(parsed);
    }

    private void GradesSummaryButton_OnClick()
    {
        navigationService.PushAsync<GradesSummaryPage>();
    }
}