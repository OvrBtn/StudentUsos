using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using StudentUsos.Features.Payments.Models;
using StudentUsos.Features.Payments.Repositories;
using StudentUsos.Features.Payments.Services;

namespace StudentUsos.Features.Payments.Views
{
    public partial class PaymentsViewModel : BaseViewModel
    {
        IPaymentsService paymentsService;
        IPaymentsRepository paymentsRepository;
        public PaymentsViewModel(IPaymentsService paymentsService, IPaymentsRepository paymentsRepository)
        {
            this.paymentsService = paymentsService;
            this.paymentsRepository = paymentsRepository;
        }

        public async Task InitAsync()
        {
            await Task.Delay(100);
            var paymentsLocal = paymentsRepository.GetAllPayments();
            if (paymentsLocal != null)
            {
                Payments = paymentsLocal.ToObservableCollection();
                if (paymentsLocal.Count > 0)
                {
                    MainStateKey = StateKey.Loaded;
                }
            }

            var paymentsApi = await paymentsService.GetPaymentsApiAsync();
            if (paymentsApi == null)
            {
                if (MainStateKey == StateKey.Loading) MainStateKey = StateKey.Empty;
                return;
            }
            Payments = paymentsApi.ToObservableCollection();
            if (paymentsApi.Count == 0)
            {
                MainStateKey = StateKey.Empty;
            }
            else
            {
                MainStateKey = StateKey.Loaded;
            }
            paymentsRepository.ClearAndSave(paymentsApi);
        }

        [ObservableProperty] string mainStateKey = StateKey.Loading;
        [ObservableProperty] ObservableCollection<Payment> payments = new();
    }
}
