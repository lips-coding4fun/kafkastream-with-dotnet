using BankSimulator.Store;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;

namespace BankSimulator.ExternalServices
{
    public class CreditCardExternalService : ICreditCardExternalService
    {
        private readonly IProducer<string, BankSimulation.CreditCard> producer;
        private Configuration configuration;
        private readonly ILogger<CreditCardExternalService> logger;

        public CreditCardExternalService(ILogger<CreditCardExternalService> logger, Configuration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration.BoostrapServers,
                Debug = configuration.KafkaDebug,
                GroupId = configuration.KafkaGroupId,
            };

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = configuration.BoostrapServers,
                Debug = configuration.KafkaDebug
            };

            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = configuration.SchemaRegistry
            };

            var avroSerializerConfig = new AvroSerializerConfig
            {
                BufferBytes = 100
            };

            var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
            producer =
                new ProducerBuilder<string, BankSimulation.CreditCard>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<BankSimulation.CreditCard>(schemaRegistry, avroSerializerConfig))
                    .Build();
        }

        public async Task PublishCreditCard(CreditCard creditCard)
        {
            var creditCardEvent = new BankSimulation.CreditCard
            {
                code = creditCard.Code,
                owner = creditCard.Owner,
                initialMoney = creditCard.InitialMoney,
                cardNumber = creditCard.CardNumber.ToString()
            };

            await producer
                .ProduceAsync("bank_creditcard", new Message<string, BankSimulation.CreditCard> { Key = creditCard.CardNumber.ToString(), Value = creditCardEvent })
                .ContinueWith(task =>
                    {
                        if (!task.IsFaulted)
                        {
                            logger.LogDebug($"produced to: {task.Result.TopicPartitionOffset}");
                            return;
                        }
                        logger.LogError($"error producing message: {task?.Exception?.InnerException}");
                    });
        }
    }
}