using BankSimulator.ExternalServices;
using BankSimulator.Store;
using Microsoft.Extensions.Logging;

namespace BankSimulator
{
    internal class CustomerUser : BankUser
    {
        private readonly ILogger<CustomerUser> logger;
        private readonly ITransactionExternalService transactionExternalService;
        private readonly BankUserList bankUserList;

        public CustomerUser(ILogger<CustomerUser> logger, ITransactionExternalService transactionExternalService, BankUserList bankUserList)
        {
            this.bankUserList = bankUserList;
            this.logger = logger;
            this.transactionExternalService = transactionExternalService;
        }

        public override void StartTransaction()
        {
            logger.LogInformation("a customer is starting a new transaction");

            var random = new Random();
            var r = random.NextInt64(0, bankUserList.CreditCards.Count());
            var creditCard = bankUserList.CreditCards[(int)r];


            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var amount = random.NextInt64(100, 100000) / 100;
                    if (random.NextInt64(0, 100) < 80)
                    {
                        logger.LogInformation($"Customer is trying with card {creditCard.CardNumber} and the right code");
                        transactionExternalService.SendNewTransaction(creditCard.CardNumber, creditCard.Code, amount);
                        break;
                    }
                    else
                    {
                        var code = random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString();
                        transactionExternalService.SendNewTransaction(creditCard.CardNumber, "0000", amount);
                        logger.LogWarning($"Customer is trying with card {creditCard.CardNumber} and the wrong code");
                    }
                    Thread.Sleep((int)random.NextInt64(0, 100));
                }
            });
        }
    }
}