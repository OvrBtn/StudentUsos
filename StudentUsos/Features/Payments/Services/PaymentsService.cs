using StudentUsos.Features.Payments.Models;
using StudentUsos.Services.ServerConnection;
using System.Text.Json;

namespace StudentUsos.Features.Payments.Services
{
    public class PaymentsService : IPaymentsService
    {
        IServerConnectionManager serverConnectionManager;
        ILogger? logger;
        public PaymentsService(IServerConnectionManager serverConnectionManager, ILogger? logger = null)
        {
            this.serverConnectionManager = serverConnectionManager;
            this.logger = logger;
        }

        public async Task<List<Payment>?> GetPaymentsApiAsync()
        {
            try
            {
                var arguments = new Dictionary<string, string>();
                var response = await serverConnectionManager.SendRequestToUsosAsync("services/payments/user_payments", arguments);
                if (response == null)
                {
                    return null;
                }
                var deserialized = JsonSerializer.Deserialize(response, PaymentJsonContext.Default.ListPaymentJson);
                if (deserialized is null)
                {
                    return null;
                }

                List<Payment> payments = new();
                foreach (var item in deserialized)
                {
                    var payment = item.ToPayment();
                    payments.Add(payment);
                }
                return payments;
            }
            catch (Exception ex)
            {
                logger?.LogCatchedException(ex);
                return null;
            }
        }
    }
}
