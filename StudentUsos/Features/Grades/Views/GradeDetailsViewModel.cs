using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Grades.Models;
using StudentUsos.Features.Grades.Services;

namespace StudentUsos.Features.Grades.Views;

public partial class GradeDetailsViewModel : BaseViewModel
{
    IGradesService GradesService;
    public GradeDetailsViewModel(IGradesService gradesService)
    {
        this.GradesService = gradesService;
    }

    [ObservableProperty]
    FinalGradeGroup finalGradeGroup;

    public async void InitAsync(FinalGradeGroup finalGradeGroup)
    {
        FinalGradeGroup = finalGradeGroup;
        FinalGradeGroup.BuildGradeDistributionChart();
        await Task.Delay(500);
        _ = GradesService.UpdateGradeDistributionAndRefreshChartAsync(FinalGradeGroup);
    }
}