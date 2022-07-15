namespace BankSimulator
{
    public class Configuration
    {
        Dictionary<string,string> configurations;

        public Configuration()
        {
            configurations = new Dictionary<string, string>
            {
                {"BOOTSTRAP_SERVERS","localhost:9092"},
                {"SCHEMA_REGISTRY","http://localhost:8081/"},
                {"KAFKA_DEBUG",null},
                {"KAFKA_GROUP_ID","BankDev"}
            };

            foreach(var kv in configurations)
            {
                var v = Environment.GetEnvironmentVariable(kv.Key);
                if (v != null) configurations[kv.Key] = v;
            }
        }

        public string BoostrapServers { get => configurations["BOOTSTRAP_SERVERS"]; }
        public string KafkaDebug { get => configurations["KAFKA_DEBUG"]; }
        public string SchemaRegistry { get => configurations["SCHEMA_REGISTRY"]; }
        public string KafkaGroupId{ get => configurations["KAFKA_GROUP_ID"]; }
    }
}