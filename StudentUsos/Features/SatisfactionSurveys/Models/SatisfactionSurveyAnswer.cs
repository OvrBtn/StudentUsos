using System.Text.Json.Serialization;

namespace StudentUsos.Features.SatisfactionSurveys.Models
{
    [JsonSerializable(typeof(SatisfactionSurveyAnswer))]
    internal partial class SatisfactionSurveyAnswerJsonContext : JsonSerializerContext
    { }

    public class SatisfactionSurveyAnswer
    {
        public SatisfactionSurveyQuestion SatisfactionSurveyQuestion { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public string DisplayText { get => Utilities.GetLocalizedStringFromJson(DisplayTextJson); }
        [JsonPropertyName("display_text_html"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string DisplayTextJson { get; set; }
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                AnswerCheckedAsync();
            }
        }
        bool isChecked = false;

        public SatisfactionSurveyAnswer()
        {

        }

        bool isCheckedFirstTime = true;
        async void AnswerCheckedAsync()
        {
            await Task.Delay(50);
            if (isCheckedFirstTime && IsChecked)
            {
                isCheckedFirstTime = false;
                SatisfactionSurveyQuestion.SatisfactionSurvey.ViewModel.NextQuestion();
            }
        }
    }
}
