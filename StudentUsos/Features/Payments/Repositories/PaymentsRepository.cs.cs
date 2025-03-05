using StudentUsos.Features.Payments.Models;

namespace StudentUsos.Features.Payments.Repositories;

public class PaymentsRepository : IPaymentsRepository
{
    ILocalDatabaseManager localDatabaseManager;
    public PaymentsRepository(ILocalDatabaseManager localDatabaseManager)
    {
        this.localDatabaseManager = localDatabaseManager;
    }

    public List<Payment> GetAllPayments()
    {
        var payments = localDatabaseManager.GetAll<Payment>();
        return payments;
    }

    public void ClearAndSave(List<Payment> payments)
    {
        localDatabaseManager.ClearTable<Payment>();
        localDatabaseManager.InsertAll(payments);
    }

}