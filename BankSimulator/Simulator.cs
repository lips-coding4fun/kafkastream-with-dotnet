using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BankSimulator.ExternalServices;
using BankSimulator.Store;

namespace BankSimulator
{
    public class Simulator
    {
        private readonly ILogger<Simulator> logger;
        private readonly IServiceProvider provider;
        private readonly ITransactionExternalService transactionExternalService;
        private readonly BankUserList bankUserList;

        public Simulator(ILogger<Simulator> logger, IServiceProvider provider, ITransactionExternalService transactionExternalService, BankUserList bankUserList)
        {
            this.bankUserList = bankUserList;
            logger.LogInformation("Initializing simulator");
            this.logger = logger;
            this.provider = provider;
            this.transactionExternalService = transactionExternalService;
        }

        public void Start()
        {
            Random random = new Random();
            logger.LogInformation("Starting simulator ...");
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep((int)random.NextInt64(1, 100));
                    BankUser? user = random.NextInt64(1, 100) > 98 ?
                        provider.GetService<ThugUser>() :
                        provider.GetService<CustomerUser>();
                    user?.StartTransaction();
                }
            });

            transactionExternalService.OnNewTransaction += async (t) =>
            {
                if (null == bankUserList.CreditCards.FirstOrDefault(x => x.CardNumber.ToString() == t.cardNumber && x.Code == t.code))
                    await transactionExternalService.RejectTransaction(t);
                else
                    await transactionExternalService.AcceptTransaction(t);
            };
        }
        public void Stop()
        {
            logger.LogInformation("Stopping simulator ...");
        }
    }
}