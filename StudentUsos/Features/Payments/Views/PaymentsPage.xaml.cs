using StudentUsos.Controls;

namespace StudentUsos.Features.Payments.Views
{
    public partial class PaymentsPage : CustomContentPageNotAnimated
    {
        PaymentsViewModel paymentsViewModel;
        public PaymentsPage(PaymentsViewModel paymentsViewModel)
        {
            InitializeComponent();
            BindingContext = this.paymentsViewModel = paymentsViewModel;
        }

        bool isViewModelSet = false;
        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            if (isViewModelSet) return;
            Dispatcher.Dispatch(async () =>
            {
                isViewModelSet = true;
                await paymentsViewModel.InitAsync();
            });

        }
    }
}