using StudentUsos.Features.Payments.Models;

namespace StudentUsos.Features.Payments.Services;

public interface IPaymentsService
{
    public Task<List<Payment>?> GetPaymentsApiAsync();
}