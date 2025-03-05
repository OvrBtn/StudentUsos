using SkiaSharp.Extended.UI.Controls;
using StudentUsos.Controls;
using StudentUsos.Features.SatisfactionSurveys.Models;

namespace StudentUsos.Features.SatisfactionSurveys.Views;

public partial class FillSatisfactionSurveyPage : CustomContentPageNotAnimated
{
    public SKLottieView SkSuccessLottieView { get; init; }
    public SKLottieView SkFailLottieView { get; init; }
    FillSatisfactionSurveyViewModel fillSatisfactionSurveyViewModel;
    public FillSatisfactionSurveyPage(FillSatisfactionSurveyViewModel fillSatisfactionSurveyViewModel)
    {
        InitializeComponent();
        SkSuccessLottieView = successLottieAnimation;
        SkFailLottieView = failLottieAnimation;
        BindingContext = this.fillSatisfactionSurveyViewModel = fillSatisfactionSurveyViewModel;

    }

    public async Task InitAsync(SatisfactionSurvey satisfactionSurvey, Action onSuccess)
    {
        await fillSatisfactionSurveyViewModel.InitAsync(satisfactionSurvey, this, filledProgressBar, onSuccess);
    }
}