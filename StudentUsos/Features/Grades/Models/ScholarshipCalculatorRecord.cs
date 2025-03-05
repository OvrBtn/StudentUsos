using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.Grades.Models;

[JsonSerializable(typeof(ScholarshipCalculatorRecord)), JsonSerializable(typeof(List<ScholarshipCalculatorRecord>))]
internal partial class ScholarshipCalculatorRecordContext : JsonSerializerContext
{

}

public partial class ScholarshipCalculatorRecord : BaseViewModel
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }
    [JsonPropertyName("ParentId")]
    public int ParentId { get; set; }
    [JsonPropertyName("Depth")]
    public int Depth { get; set; }
    [JsonPropertyName("PointMinimum")]
    public int PointMinimum { get; set; }
    [JsonPropertyName("PointMaximum")]
    public int PointMaximum { get; set; }
    [JsonPropertyName("HasMultiplier")]
    public bool HasMultiplier { get; set; }
    [JsonPropertyName("IsSectionTitle")]
    public bool IsSectionTitle { get; set; }
    [JsonPropertyName("Text"), JsonConverter(typeof(JsonObjectToStringConverter))]
    public string TextJson { get; set; }
    [JsonIgnore]
    public string Text
    {
        get => Utilities.GetLocalizedStringFromJson(TextJson);
    }

    public string PointsMinimumMaximum
    {
        get
        {
            if (PointMinimum != PointMaximum)
            {
                return PointMinimum + " - " + PointMaximum;
            }
            return PointMinimum.ToString();
        }
    }

    public bool IsCheckBoxVisible
    {
        get
        {
            return IsSectionTitle == false && HasMultiplier == false;
        }
    }
    [ObservableProperty] bool isCheckBoxChecked = false;
    [ObservableProperty] int multiplier = 0;


    //public bool IsVisible { get; set; } = false;
    public bool IsExpanded { get; set; } = false;
    const int SingleMargin = 25;
    public int Margin
    {
        get => SingleMargin * Depth;
    }

    public ScholarshipCalculatorRecord()
    {

    }

    public static List<ScholarshipCalculatorRecord>? DeserializeJson(string data)
    {
        return JsonSerializer.Deserialize(data, ScholarshipCalculatorRecordContext.Default.ListScholarshipCalculatorRecord);
    }
}