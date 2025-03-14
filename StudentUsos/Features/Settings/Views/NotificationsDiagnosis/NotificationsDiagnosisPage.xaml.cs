using StudentUsos.Controls;

namespace StudentUsos.Features.Settings.Views.NotificationsDiagnosis;

public partial class NotificationsDiagnosisPage : CustomContentPageNotAnimated
{
    NotificationsDiagnosisViewModel viewModel;
    public NotificationsDiagnosisPage(NotificationsDiagnosisViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    bool isViewModelSet = false;
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (isViewModelSet)
        {
            return;
        }
        isViewModelSet = true;
        Dispatcher.Dispatch(() =>
        {
            viewModel.Init();
        });
    }
}