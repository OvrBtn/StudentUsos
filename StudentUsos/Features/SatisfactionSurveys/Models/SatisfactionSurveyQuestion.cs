using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.SatisfactionSurveys.Models
{
    [JsonSerializable(typeof(SatisfactionSurveyQuestion))]
    internal partial class SatisfactionSurveyQuestionJsonContext : JsonSerializerContext
    { }

    public class SatisfactionSurveyQuestion
    {
        public SatisfactionSurvey SatisfactionSurvey { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public string DisplayText { get => Utilities.GetLocalizedStringFromJson(DisplayTextJson); }
        [JsonPropertyName("display_text_html"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string DisplayTextJson { get; set; }
        [JsonPropertyName("possible_answers")]
        public ObservableCollection<SatisfactionSurveyAnswer> PossibleAnswers { get; set; } = new();
        public SatisfactionSurveyAnswer? Checked { get => PossibleAnswers.Where(x => x.IsChecked).FirstOrDefault(defaultValue: null); }
        public bool IsChecked { get => PossibleAnswers.Any(x => x.IsChecked); }
        [JsonPropertyName("number"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string Number { get; set; }
        //public string AllowCommentString { get; set; }
        [JsonPropertyName("allow_comment")]
        public bool AllowComment { get; set; } = false;
        public string Comment { get; set; }
        public bool HasContent { get => AllowComment || PossibleAnswers?.Count > 0; }
        public bool Seen { get; set; } = false;
        [JsonPropertyName("comment_length"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string CommentMaxLenghtString { get; set; } = "0";
        public int CommentMaxLength { get => int.TryParse(CommentMaxLenghtString, out int parsed) ? parsed : 0; }

        public SatisfactionSurveyQuestion()
        {

        }
    }
}
