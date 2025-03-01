using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp.Extended.UI.Controls;
using StudentUsos.Controls;
using StudentUsos.Features.SatisfactionSurveys.Models;
using StudentUsos.Features.SatisfactionSurveys.Services;
using StudentUsos.Resources.LocalizedStrings;
using System.Globalization;

namespace StudentUsos.Features.SatisfactionSurveys.Views
{
    public partial class FillSatisfactionSurveyViewModel : BaseViewModel
    {
        [ObservableProperty] SatisfactionSurvey satisfactionSurvey;
        public CustomProgressBar ProgressBar { get; set; }
        public FillSatisfactionSurveyPage Page { get; set; }
        public Action OnSuccess { get; private set; }
        ISatisfactionSurveysService satisfactionSurveysService;
        IApplicationService applicationService;
        INavigationService navigationService;
        public FillSatisfactionSurveyViewModel(
            ISatisfactionSurveysService satisfactionSurveysService,
            IApplicationService applicationService,
            INavigationService navigationService)
        {
            this.satisfactionSurveysService = satisfactionSurveysService;
            this.applicationService = applicationService;
            this.navigationService = navigationService;

            NextQuestionCommand = new(NextQuestion);
            PreviousQuestionCommand = new(PreviousQuestion);
            SendButtonClickedCommand = new(SendButtonClickedAsync);
            TryAgainCommand = new(SendButtonClickedAsync);
        }

        public async Task InitAsync(SatisfactionSurvey satisfactionSurvey, FillSatisfactionSurveyPage page, CustomProgressBar progressBar, Action onSuccess)
        {
            Page = page;
            ProgressBar = progressBar;
            OnSuccess = onSuccess;

            satisfactionSurvey.ViewModel = this;

            await Task.Delay(100);
            SatisfactionSurvey = satisfactionSurvey;
            if (SatisfactionSurvey != null && SatisfactionSurvey.Questions.Count > 0)
            {
                CurrentQuestion = SatisfactionSurvey.Questions[0];
            }
            MainStateKey = StateKey.Loaded;
        }

        public static class AdditionalStateKey
        {
            public const string Success = nameof(Success);
        }

        [ObservableProperty] string mainStateKey = StateKey.Loading;
        [ObservableProperty] SatisfactionSurveyQuestion currentQuestion;
        [ObservableProperty] Command nextQuestionCommand;
        [ObservableProperty] Command previousQuestionCommand;
        public int CurrentQuestionIndex = 0;
        [ObservableProperty] bool isNextButtonVisible = true;
        [ObservableProperty] bool isSendButtonVisible = false;
        [ObservableProperty] Command sendButtonClickedCommand;

        [ObservableProperty, NotifyPropertyChangedFor(nameof(IsAnyLottieVisible))]
        bool isSuccessLottieVisible = false;
        [ObservableProperty, NotifyPropertyChangedFor(nameof(IsAnyLottieVisible))]
        bool isFailLottieVisible = false;
        [ObservableProperty] bool isTryAgainButtonVisible = false;
        [ObservableProperty] Command tryAgainCommand;

        public bool IsAnyLottieVisible { get => IsSuccessLottieVisible || IsFailLottieVisible; }

        async void SendButtonClickedAsync()
        {
            //IsContentVisible = !IsContentVisible;

            IsSuccessLottieVisible = false;
            IsFailLottieVisible = false;
            IsTryAgainButtonVisible = false;

            MainStateKey = StateKey.Loading;
            await Task.Delay(10);

            var result = await satisfactionSurveysService.SendAsync(SatisfactionSurvey);

            if (result == SendResult.Success)
            {
                IsSuccessLottieVisible = true;
                Page.SkSuccessLottieView.Progress = TimeSpan.Zero;
                await Task.Delay(2000);
                OnSuccess?.Invoke();
                await navigationService.PopAsync();
            }
            else
            {
                IsFailLottieVisible = true;
                Page.SkFailLottieView.Progress = TimeSpan.Zero;
                if (result == SendResult.RuntimeError)
                {
                    applicationService.ShowToast(LocalizedStrings.Errors_Error);
                }
                else
                {
                    applicationService.ShowToast(LocalizedStrings.Errors_USOSAPIConnectionError);
                }
                await Task.Delay(2000);
                IsTryAgainButtonVisible = true;
            }

        }

        public void NextQuestion()
        {
            if (CurrentQuestionIndex + 1 >= SatisfactionSurvey.Questions.Count)
            {
                return;
            }
            CurrentQuestion.Seen = true;
            CurrentQuestionIndex++;
            CurrentQuestion = SatisfactionSurvey.Questions[CurrentQuestionIndex];
            CurrentQuestion.Seen = true;
            if (CurrentQuestionIndex + 1 >= SatisfactionSurvey.Questions.Count)
            {
                IsNextButtonVisible = !IsNextButtonVisible;
                IsSendButtonVisible = !IsSendButtonVisible;
            }
            SatisfactionSurvey.UpdatePercentage();
        }

        public void PreviousQuestion()
        {
            if (CurrentQuestionIndex == SatisfactionSurvey.Questions.Count - 1)
            {
                IsNextButtonVisible = !IsNextButtonVisible;
                IsSendButtonVisible = !IsSendButtonVisible;
            }
            if (CurrentQuestionIndex - 1 < 0) return;
            CurrentQuestionIndex--;
            CurrentQuestion = SatisfactionSurvey.Questions[CurrentQuestionIndex];
        }
    }

    public class SkLottieImageSourceConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            return SKLottieImageSource.FromFile(value.ToString()!) as SKLottieImageSource;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
