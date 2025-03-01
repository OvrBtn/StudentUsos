using StudentUsos.Features.Payments.Models;

namespace StudentUsos.Features.Payments.Repositories
{
    public interface IPaymentsRepository
    {
        public List<Payment> GetAllPayments();

        public void ClearAndSave(List<Payment> payments);

    }
}
