using Azure.Storage.Queues;
using Microsoft.EntityFrameworkCore;
using Orders.Events;
using Orders.Messaging;

namespace Orders;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        IServiceCollection services = builder.Services;

        services.AddDbContext<OrdersDbContext>(ConfigureDbContextOptions);
        services.AddSingleton(CreateStorageQueueBus);
        services.AddSingleton<IBus<PaymentApproved>>(CreateStorageQueueBus);
        services.AddSingleton<IAsyncObservable<PaymentApproved>>(CreateStorageQueueBus);
        
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();
        
        PaymentApprovedEventHandler.Listen(app.Services);
        
        app.Run();
    }

    private static StorageQueueBus CreateStorageQueueBus(IServiceProvider provider)
    {
        IConfiguration config = provider.GetRequiredService<IConfiguration>();
        QueueClient client = new (
            connectionString: config["Storage:ConnectionString"], 
            queueName: config["Storage:Queues:PaymentApproved"]);
        return new StorageQueueBus(client);
    }

    private static void ConfigureDbContextOptions(
        IServiceProvider provider,
        DbContextOptionsBuilder options)
    {
        IConfiguration config = provider.GetRequiredService<IConfiguration>();
        options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
    }
}
