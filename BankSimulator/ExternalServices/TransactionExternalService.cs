using BankSimulation;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace BankSimulator.ExternalServices
{
    public class TransactionExternalService : ITransactionExternalService, IDisposable
    {
        private readonly Configuration configuration;

        public TransactionExternalService(Configuration configuration)
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
                new ProducerBuilder<string, Transaction>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<Transaction>(schemaRegistry, avroSerializerConfig))
                    .Build();
            producerResult =
                new ProducerBuilder<string, TransactionResult>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<TransactionResult>(schemaRegistry, avroSerializerConfig))
                    .Build();

            cts = new CancellationTokenSource();
            var consumeTask = Task.Run(() =>
{
    consumer =
                    new ConsumerBuilder<string, Transaction>(consumerConfig)
                        .SetValueDeserializer(new AvroDeserializer<Transaction>(schemaRegistry).AsSyncOverAsync())
                        .Build();

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
                Console.WriteLine($"Consume error: {e.Error.Reason}");
            }
        }
    }
    catch (OperationCanceledException)
    {
        consumer.Close();
    }
});
        }

        private IProducer<string, Transaction> producer;
        private IProducer<string, TransactionResult> producerResult;
        private readonly CancellationTokenSource cts;

        public IConsumer<string, Transaction> consumer;

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
                            Console.WriteLine($"produced to: {task.Result.TopicPartitionOffset}");
                            return;
                        }
                        Console.WriteLine($"error producing message: {task?.Exception?.InnerException}");
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
                            Console.WriteLine($"produced to: {task.Result.TopicPartitionOffset}");
                            return;
                        }
                        Console.WriteLine($"error producing message: {task?.Exception?.InnerException}");
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
                            Console.WriteLine($"produced to: {task.Result.TopicPartitionOffset}");
                            return;
                        }
                        Console.WriteLine($"error producing message: {task?.Exception?.InnerException}");
                    });
        }

        public void Dispose()
        {
            cts.Cancel();
        }
    }
}
