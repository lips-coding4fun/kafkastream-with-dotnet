using BankSimulator.ExternalServices;

namespace BankSimulator.Store
{
    public class BankUserList
    {
        public List<CreditCard> CreditCards {get;} = new  List<CreditCard>();
        public BankUserList(ICreditCardExternalService creditCardExternalService)
        {
            for(int i=0;i<=1000;i++)
            {
                var creditCard = new CreditCard();
                CreditCards.Add(creditCard);
                creditCardExternalService.PublishCreditCard(creditCard).Wait();
            }
                
        }
    }
}