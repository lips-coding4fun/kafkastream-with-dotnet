using BankSimulation;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;

namespace BankSimulator.ExternalServices
{
    public class TransactionExternalService : ITransactionExternalService, IDisposable
    {
        private readonly Configuration configuration;
        private readonly ILogger<TransactionExternalService> logger;

        public TransactionExternalService(ILogger<TransactionExternalService> logger, Configuration configuration)
        {
            this.logger = logger;
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

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
                new ProducerBuilder<string, Transaction>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<Transaction>(schemaRegistry, avroSerializerConfig))
                    .Build();
            producerResult =
                new ProducerBuilder<string, TransactionResult>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<TransactionResult>(schemaRegistry, avroSerializerConfig))
                    .Build();

            cts = new CancellationTokenSource();
            consumer =
                            new ConsumerBuilder<string, Transaction>(consumerConfig)
                                .SetValueDeserializer(new AvroDeserializer<Transaction>(schemaRegistry).AsSyncOverAsync())
                                .Build();
            var consumeTask = Task.Run(() =>
{

    consumer.Subscribe("bank.transaction");

    try
    {
        while (true)
        {
            try
            {
                var consumeResult = consumer.Consume(cts.Token);
                if (OnNewTransaction != null) OnNewTransaction(consumeResult.Message.Value);
            }
            catch (ConsumeException e)
            {
                logger.LogError($"Consume error: {e.Error.Reason}");
            }
        }
    }
    catch (OperationCanceledException)
    {
        consumer.Close();
    }
});
        }

        private readonly IProducer<string, Transaction> producer;
        private readonly IProducer<string, TransactionResult> producerResult;
        private readonly CancellationTokenSource cts;

        public readonly IConsumer<string, Transaction> consumer;

        public event NewTransaction? OnNewTransaction;

        public async Task SendNewTransaction(Guid cardNumber, string code, float amount)
        {
            var transaction = new Transaction
            {
                transactionID = Guid.NewGuid().ToString(),
                cardNumber = cardNumber.ToString(),
                code = code,
                amount = amount
            };

            await producer
                .ProduceAsync("bank.transaction", new Message<string, Transaction> { Key = transaction.cardNumber, Value = transaction })
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

        public async Task RejectTransaction(Transaction t)
        {
            var transactionResult = new TransactionResult
            {
                transactionID = t.transactionID,
                result = false
            };

            await producerResult
                .ProduceAsync("bank.transaction.result", new Message<string, TransactionResult> { Key = transactionResult.transactionID, Value = transactionResult })
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

        public async Task AcceptTransaction(Transaction t)
        {
            var transactionResult = new TransactionResult
            {
                transactionID = t.transactionID,
                result = true
            };

            await producerResult
                .ProduceAsync("bank.transaction.result", new Message<string, TransactionResult> { Key = transactionResult.transactionID, Value = transactionResult })
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

        public void Dispose()
        {
            cts.Cancel();
        }
    }
}
