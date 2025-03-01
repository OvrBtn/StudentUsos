using StudentUsos.Features.SatisfactionSurveys.Models;
using System.Collections.ObjectModel;

namespace StudentUsos.Features.SatisfactionSurveys.Services
{
    public interface ISatisfactionSurveysService
    {
        public Task<ObservableCollection<SatisfactionSurvey>?> GetSurveysToFillFromApiAsync();

        public Task<SendResult> SendAsync(SatisfactionSurvey satisfactionSurvey);
    }
}
