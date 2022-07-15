

using BankSimulation;

namespace BankSimulator.ExternalServices
{
    public delegate void NewTransaction(Transaction transaction);
    public interface ITransactionExternalService
    {
        Task SendNewTransaction(Guid cardNumber, string code, float amount);

        event NewTransaction? OnNewTransaction;

        Task RejectTransaction(Transaction t);
        Task AcceptTransaction(Transaction t);
    }
}