using CommunityToolkit.Maui.Core.Extensions;
using StudentUsos.Features.SatisfactionSurveys.Models;
using StudentUsos.Services.ServerConnection;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace StudentUsos.Features.SatisfactionSurveys.Services;

public class SatisfactionSurveysService : ISatisfactionSurveysService
{
    IServerConnectionManager serverConnectionManager;
    ILogger? logger;
    public SatisfactionSurveysService(IServerConnectionManager serverConnectionManager,
        ILogger? logger = null)
    {
        this.serverConnectionManager = serverConnectionManager;
        this.logger = logger;
    }

    public async Task<ObservableCollection<SatisfactionSurvey>?> GetSurveysToFillFromApiAsync()
    {
        Dictionary<string, string> arguments = new()
        {
            { "fields", "id|survey_type|name|headline_html|start_date|end_date|can_i_fill_out|did_i_fill_out|group|groups_conducted|lecturer|faculty|programme|filled_out_count|entitled_count|can_i_view_results|why_cant_i_view_results|questions|has_final_comment" }
        };
        var json = await serverConnectionManager.SendRequestToUsosAsync("services/surveys/surveys_to_fill2", arguments);
        if (json is null)
        {
            return null;
        }
        var deserialized = JsonSerializer.Deserialize(json, SatisfactionSurveyJsonContext.Default.ListSatisfactionSurvey);
        if (deserialized is null)
        {
            return null;
        }
        foreach (var survey in deserialized)
        {
            foreach (var question in survey.Questions)
            {
                question.SatisfactionSurvey = survey;
                foreach (var answer in question.PossibleAnswers)
                {
                    answer.QuestionId = question.Id;
                    answer.SatisfactionSurveyQuestion = question;
                }
            }
        }
        return deserialized.ToObservableCollection();
    }

    public async Task<SendResult> SendAsync(SatisfactionSurvey satisfactionSurvey)
    {
        try
        {
            string json = satisfactionSurvey.BuildJsonWithAnswers();

            Dictionary<string, string> arguments = new()
            {
                { "survey_id", satisfactionSurvey.Id },
                { "answers", json }
            };
            var result = await serverConnectionManager.SendRequestToUsosAsync("services/surveys/fill_out2", arguments);
            if (result == null) return SendResult.ApiConnectionError;
            string resultString = result.ToString();
            Logger.Default?.Log(LogLevel.Info, resultString);
            //lenght < 5 since in case of success API will return nothing or just "{}" ; when error is thrown the response is always longer than 5 characters
            if (resultString.Length < 5) return SendResult.Success;
            return SendResult.RuntimeError;
        }
        catch (Exception ex)
        {
            logger?.LogCatchedException(ex);
            return SendResult.RuntimeError;
        }
    }
}