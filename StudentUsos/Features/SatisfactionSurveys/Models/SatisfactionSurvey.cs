using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Features.Groups.Models;
using StudentUsos.Features.Person.Models;
using StudentUsos.Features.SatisfactionSurveys.Views;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json.Serialization;

namespace StudentUsos.Features.SatisfactionSurveys.Models
{
    [JsonSerializable(typeof(List<SatisfactionSurvey>))]
    internal partial class SatisfactionSurveyJsonContext : JsonSerializerContext
    { }

    public partial class SatisfactionSurvey : ObservableObject
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public string Name { get => Utilities.GetLocalizedStringFromJson(NameJson); }
        [JsonPropertyName("name"), JsonConverter(typeof(JsonObjectToStringConverter))]
        public string NameJson { get; set; }
        [JsonPropertyName("has_final_comment")]
        public bool HasFinalComment { get; set; }
        //[JsonPropertyName("has_final_comment"), JsonConverter(typeof(RawJSONConverter))]
        //public string HasFinalCommentString { get; set; }
        //public bool HasFinalComment { get => bool.TryParse(HasFinalCommentString, out bool parsed) ? parsed : false; }
        [JsonPropertyName("questions")]
        public ObservableCollection<SatisfactionSurveyQuestion> Questions { get; set; } = new();
        [JsonPropertyName("group")]
        public Group Group { get; set; }
        [JsonPropertyName("lecturer")]
        public Lecturer Lecturer { get; set; }

        public FillSatisfactionSurveyViewModel ViewModel;

        [ObservableProperty]
        float filledPercentage = 0f;

        public SatisfactionSurveysViewModel SatisfactionSurveysViewModel { get; set; }

        public SatisfactionSurvey()
        {

        }

        [RelayCommand]
        async Task ClickedAsync()
        {
            FillSatisfactionSurveyPage page = await App.ServiceProvider.GetService<INavigationService>()!.PushAsync<FillSatisfactionSurveyPage>();
            await page.InitAsync(this, OnSuccess);
        }

        void OnSuccess()
        {
            if (SatisfactionSurveysViewModel == null) return;
            SatisfactionSurveysViewModel.Surveys.Remove(this);
        }

        public void UpdatePercentage()
        {
            var seenQuestions = Questions.Where(x => x.Seen).ToList();
            ViewModel?.ProgressBar.ProgressTo(1.0f * seenQuestions.Count / Questions.Count, 1000, Easing.CubicInOut);
        }

        public string BuildJsonWithAnswers()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("{");
            for (int i = 0; i < Questions.Count; i++)
            {
                if (Questions[i].HasContent == false) continue;
                string serializedQuestion = SerializeQuestion(Questions[i]);
                stringBuilder.Append(serializedQuestion);
                if (i < Questions.Count - 1)
                {
                    stringBuilder.Append(",");
                }
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        string SerializeQuestion(SatisfactionSurveyQuestion question)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("\"" + question.Id + "\":{");
            var checkedAnswer = question.Checked;
            stringBuilder.Append("\"answers\": [");
            if (checkedAnswer != null)
            {
                stringBuilder.Append("\"" + checkedAnswer.Id + "\"");
            }
            stringBuilder.Append("]");
            stringBuilder.Append(",");
            if (question.AllowComment)
            {
                stringBuilder.Append("\"comment\": \"" + question.Comment + "\"");
            }
            else
            {
                stringBuilder.Append("\"comment\": null");
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
