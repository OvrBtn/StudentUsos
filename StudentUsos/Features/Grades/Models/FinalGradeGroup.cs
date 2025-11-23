using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using StudentUsos.Features.Grades.Services;
using StudentUsos.Features.Grades.Views;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace StudentUsos.Features.Grades.Models;

public partial class FinalGradeGroup : BaseViewModel
{
    public string CourseId { get; set; }
    public string CourseName { get; set; }
    public FinalGrade FirstTermGrade { get; set; }
    public FinalGrade SecondTermGrade { get; set; }

    [ObservableProperty]
    ObservableCollection<ChartData> chartEntries = new();

    /// <summary>
    /// Used by <see cref="GradesViewModel"/> when viewing old terms
    /// </summary>
    [Ignore]
    public bool SaveUpdatedGradeDistributionToLocalDatabse { get; set; } = true;

    [Ignore]
    public bool IsAnyTermModified { get => FirstTermGrade.IsModified || SecondTermGrade.IsModified; }


    public FinalGradeGroup()
    {

    }

    //cooldown to avoid spamming too many requests to API
    public DateTime lastChartUpdate = DateAndTimeProvider.Current.Now.AddYears(-1);

    public void BuildGradeDistributionChart()
    {
        try
        {
            const int chartMaxHeight = 200;
            if (FirstTermGrade.IsEmpty || string.IsNullOrEmpty(FirstTermGrade.GradeDistribution)) return;
            var deserializedGradeDistribution = JsonSerializer.Deserialize(
                FirstTermGrade.GradeDistribution,
                GradeDistributionJsonContext.Default.ListGradeDistributionRecord);
            if (deserializedGradeDistribution is null)
            {
                return;
            }
            int maxPercentage = 0;
            ObservableCollection<ChartData> chartEntriesLocal = new();
            foreach (var item in deserializedGradeDistribution)
            {
                string gradeSymbol = item.GradeSymbol;
                int percentageInt = (int)item.Percentage;
                if (percentageInt > maxPercentage) maxPercentage = percentageInt;
                chartEntriesLocal.Add(new ChartData(gradeSymbol, percentageInt));
            }
            for (int j = 0; j < chartEntriesLocal.Count; j++)
            {
                chartEntriesLocal[j].ValueScaled = Utilities.Lerp(1f * chartEntriesLocal[j].Value / maxPercentage, 1, chartMaxHeight);
            }
            ChartEntries = chartEntriesLocal;
        }
        catch (Exception ex)
        {
            Logger.Default?.LogCatchedException(ex);
        }
    }

    public static bool AreCourseExamAndGradeEqual(FinalGradeGroup g1, FinalGradeGroup g2)
    {
        return g1.CourseId == g2.CourseId && g1.CourseName == g2.CourseName &&
               FinalGrade.AreCourseExamAndGradeEqual(g1.FirstTermGrade, g2.FirstTermGrade) && FinalGrade.AreCourseExamAndGradeEqual(g1.SecondTermGrade, g2.SecondTermGrade);
    }
}