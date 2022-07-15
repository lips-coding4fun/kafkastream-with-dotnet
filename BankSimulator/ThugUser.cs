using BankSimulator.ExternalServices;
using BankSimulator.Store;
using Microsoft.Extensions.Logging;

namespace BankSimulator
{
    public class ThugUser : BankUser
    {
        private readonly ILogger<ThugUser> logger;
        private readonly ITransactionExternalService transactionExternalService;
        private readonly BankUserList bankUserList;

        public ThugUser(ILogger<ThugUser> logger, ITransactionExternalService transactionExternalService, BankUserList bankUserList)
        {
            this.bankUserList = bankUserList;
            this.logger = logger;
            this.transactionExternalService = transactionExternalService;
        }

        public override void StartTransaction()
        {
            logger.LogWarning("a thug is starting a new transaction !");
            var random = new Random();
            var r = random.NextInt64(0, bankUserList.CreditCards.Count());
            var creditCardNumber = bankUserList.CreditCards[(int)r];

            var nbTrials = random.NextInt64(0, 10);

            Task.Factory.StartNew(() =>
            {
                var amount = random.NextInt64(100, 100000) / 100;
                for (int i = 1; i <= nbTrials; i++)
                {
                    var code = random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString();
                    logger.LogWarning($"Thug is trying with card {creditCardNumber.CardNumber} and code {code}");
                    transactionExternalService.SendNewTransaction(creditCardNumber.CardNumber, code, amount);
                    Thread.Sleep((int)random.NextInt64(0, 100));
                }

            });
        }
    }
}