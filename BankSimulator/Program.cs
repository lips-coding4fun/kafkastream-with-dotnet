using BankSimulator;
using BankSimulator.ExternalServices;
using BankSimulator.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
        services.AddTransient<Simulator>()
        .AddTransient<ThugUser>()
        .AddTransient<CustomerUser>()
        .AddSingleton<Configuration>()
        .AddSingleton<BankUserList>()
        .AddSingleton<ITransactionExternalService, TransactionExternalService>()
        .AddSingleton<ICreditCardExternalService, CreditCardExternalService>())
    .Build();

var simulator = host.Services.GetService<Simulator>();
simulator?.Start();
Console.CancelKeyPress += (o, e) => simulator?.Stop();

await host.RunAsync();