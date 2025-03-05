using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Grades.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StudentUsos.Features.Grades.Views;

public partial class ScholarshipCalculatorViewModel : BaseViewModel
{
    [ObservableProperty] float gradeAverage;
    public ScholarshipCalculatorViewModel()
    {

    }

    public async Task InitAsync(float gradeAverage)
    {
        GradeAverage = gradeAverage;
        RecordOnClickCommand = new(RecordOnClick);
        PointsMinimum = GradeAverage * 10;
        PointsMaximum = GradeAverage * 10;

        await Task.Delay(20);
        string jsonData = await ReadJsonWithScholarshipDataAsync();
        allScholarshipCalculatorRecords = ScholarshipCalculatorRecord.DeserializeJson(jsonData)!;

        ExpandChildren(new() { Id = -1 }, allScholarshipCalculatorRecords, ScholarshipCalculatorRecords);
    }

    [ObservableProperty] ObservableCollection<ScholarshipCalculatorRecord> scholarshipCalculatorRecords = new();
    List<ScholarshipCalculatorRecord> allScholarshipCalculatorRecords = new();

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PointsString))]
    float pointsMinimum;

    partial void OnPointsMinimumChanged(float oldValue, float newValue)
    {
        PointsMinimum = (float)Math.Round(newValue, 1);
    }

    [ObservableProperty, NotifyPropertyChangedFor(nameof(PointsString))]
    float pointsMaximum;

    partial void OnPointsMaximumChanged(float oldValue, float newValue)
    {
        PointsMaximum = (float)Math.Round(PointsMaximum, 1);
    }

    public string PointsString
    {
        get
        {

            if (PointsMinimum == PointsMaximum) return PointsMinimum.ToString();
            return $"{PointsMinimum} - {PointsMaximum}";
        }
    }

    /// <summary>
    /// Expand children of clicked record by adding them to observed collection - <paramref name="target"/>
    /// </summary>
    /// <param name="record"></param>
    /// <param name="allRecords"></param>
    /// <param name="target"></param>
    /// <returns>All records added to observed collection</returns>
    List<ScholarshipCalculatorRecord> ExpandChildren(ScholarshipCalculatorRecord record,
        List<ScholarshipCalculatorRecord> allRecords,
        ObservableCollection<ScholarshipCalculatorRecord> target)
    {
        List<ScholarshipCalculatorRecord> result = new();
        int insertionIndex = target.ToList().FindIndex(x => x.Id == record.Id);
        if (insertionIndex == -1)
        {
            insertionIndex = 0;
        }
        else
        {
            insertionIndex++;
        }
        for (int i = 0; i < allRecords.Count; i++)
        {
            if (allRecords[i].ParentId == record.Id)
            {
                ScholarshipCalculatorRecords.Insert(insertionIndex, allRecords[i]);
                result.Add(allRecords[i]);
                insertionIndex++;
            }
        }
        return result;
    }

    List<ScholarshipCalculatorRecord> HideChildren(ScholarshipCalculatorRecord parent, ObservableCollection<ScholarshipCalculatorRecord> target)
    {
        List<ScholarshipCalculatorRecord> removed = new();
        bool startedRemoving = false;
        for (int i = 0; i < target.Count; i++)
        {
            if (target[i].ParentId == parent.Id)
            {
                startedRemoving = true;
                target[i].IsExpanded = false;
                HideChildren(target[i], target);
                removed.Add(target[i]);
                target.RemoveAt(i);
                i--;
            }
            else if (startedRemoving)
            {
                break;
            }
        }
        return removed;
    }

    public Command<ScholarshipCalculatorRecord> RecordOnClickCommand { get; set; }
    void RecordOnClick(ScholarshipCalculatorRecord record)
    {
        if (record.IsSectionTitle)
        {
            if (record.IsExpanded == false)
            {
                ExpandChildren(record, allScholarshipCalculatorRecords, ScholarshipCalculatorRecords);
            }
            else
            {
                HideChildren(record, ScholarshipCalculatorRecords);
            }
            record.IsExpanded = !record.IsExpanded;
        }
        else
        {
            if (record.IsCheckBoxVisible)
            {
                record.IsCheckBoxChecked = !record.IsCheckBoxChecked;
                if (record.IsCheckBoxChecked)
                {
                    PointsMinimum += record.PointMinimum;
                    PointsMaximum += record.PointMaximum;
                }
                else
                {
                    PointsMinimum -= record.PointMinimum;
                    PointsMaximum -= record.PointMaximum;
                }
            }
            else if (record.HasMultiplier)
            {
                List<string> options = new();
                for (int i = 0; i <= 100; i++)
                {
                    options.Add(i.ToString());
                }
                PickFromListPopup.CreateAndShow(record.Text, options, onPicked: (picked) =>
                {
                    int pickedInt = picked.Index;
                    int diffrence = pickedInt - record.Multiplier;
                    record.Multiplier = pickedInt;
                    PointsMinimum += diffrence * record.PointMinimum;
                    PointsMaximum += diffrence * record.PointMaximum;
                });
            }
        }
    }

    async Task<string> ReadJsonWithScholarshipDataAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("scholarship_calculator_data.json");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

public class MarginConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        double left = System.Convert.ToDouble(values[0]);
        double top = System.Convert.ToDouble(values[1]);
        double right = System.Convert.ToDouble(values[2]);
        double bottom = System.Convert.ToDouble(values[3]);
        return new Thickness(left, top, right, bottom);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}