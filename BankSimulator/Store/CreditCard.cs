using BankSimulator.ExternalServices;
using RandomNameGeneratorLibrary;

namespace BankSimulator.Store
{
    public class CreditCard
    {
        public Guid CardNumber { get; }
        public String Code { get; }
        public string Owner { get; }
        public float InitialMoney { get; }

        public CreditCard()
        {
            var random = new Random(DateTime.Now.Millisecond);
            CardNumber = Guid.NewGuid();
            Code = random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString() + random.NextInt64(-1, 10).ToString();
            Owner = new PersonNameGenerator().GenerateRandomFirstAndLastName();
            InitialMoney = 0F;

        }
    }
}