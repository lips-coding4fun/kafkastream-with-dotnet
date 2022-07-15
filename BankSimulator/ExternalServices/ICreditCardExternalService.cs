namespace BankSimulator.ExternalServices
{
    public interface ICreditCardExternalService
    {
         Task PublishCreditCard(Store.CreditCard creditCard);
    }
}