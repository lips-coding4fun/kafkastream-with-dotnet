using BankSimulator.Store;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace BankSimulator.ExternalServices
{
    public class CreditCardExternalService : ICreditCardExternalService
    {
        private readonly IProducer<string, CreditCard> producer;
        private Configuration configuration;

        public CreditCardExternalService(Configuration configuration)
        {
            this.configuration = configuration;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration.BoostrapServers,
                Debug =  configuration.KafkaDebug,
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
                new ProducerBuilder<string, CreditCard>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<CreditCard>(schemaRegistry, avroSerializerConfig))
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
                .ProduceAsync("bank.creditcard", new Message<string, CreditCard> { Key = creditCard.CardNumber.ToString(), Value = creditCard })
                .ContinueWith(task =>
                    {
                        if (!task.IsFaulted)
                        {
                            Console.WriteLine($"produced to: {task.Result.TopicPartitionOffset}");
                            return;
                        }
                        Console.WriteLine($"error producing message: {task?.Exception?.InnerException}");
                    });
        }
    }
}