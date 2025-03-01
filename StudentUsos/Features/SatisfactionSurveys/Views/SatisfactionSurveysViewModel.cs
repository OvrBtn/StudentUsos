using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.SatisfactionSurveys.Models;
using StudentUsos.Features.SatisfactionSurveys.Services;

namespace StudentUsos.Features.SatisfactionSurveys.Views
{
    public partial class SatisfactionSurveysViewModel : BaseViewModel
    {
        ISatisfactionSurveysService satisfactionSurveysService;
        public SatisfactionSurveysViewModel(ISatisfactionSurveysService satisfactionSurveysService)
        {
            this.satisfactionSurveysService = satisfactionSurveysService;
            InitAsync();
        }

        [ObservableProperty] string mainStateKey = StateKey.Loading;
        [ObservableProperty] ObservableCollection<SatisfactionSurvey> surveys = new();

        async void InitAsync()
        {
            MainStateKey = StateKey.Loading;
            var surveysFromApi = await satisfactionSurveysService.GetSurveysToFillFromApiAsync();
            if (surveysFromApi == null)
            {
                MainStateKey = StateKey.ConnectionError;
                return;
            }
            else if (surveysFromApi.Count == 0)
            {
                MainStateKey = StateKey.Empty;
                return;
            }
            foreach (var item in surveysFromApi)
            {
                item.SatisfactionSurveysViewModel = this;
            }
            Surveys = surveysFromApi;
            MainStateKey = StateKey.Loaded;
        }
    }
}
