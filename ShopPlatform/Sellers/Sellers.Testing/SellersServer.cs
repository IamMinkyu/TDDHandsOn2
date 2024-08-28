using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Sellers;

public sealed class SellersServer: TestServer
{
  private const string  ConnectionString = "Server=127.0.0.1;Port=5432;Database=Sellers_Unit;User Id=postgres;Password=mysecretpassword;";
  
  public SellersServer(
    IServiceProvider services,
    IOptions<TestServerOptions> optionsAccessor)
    : base(services, optionsAccessor)
  {
  }

  public static SellersServer Create()
  {
    SellersServer server = (SellersServer) new Factory().Server;
    lock(typeof(SellersDbContext))
    {
      using IServiceScope scope = server.Services.CreateScope();
      SellersDbContext context = scope.ServiceProvider.GetRequiredService<SellersDbContext>();
      context.Database.Migrate();
    }
    return server;
  }

  private sealed class Factory: WebApplicationFactory<Program>
  {
    protected override IHost CreateHost(IHostBuilder builder)
    {
      builder.ConfigureServices(services =>
      {
        services.AddSingleton<IServer, SellersServer>();
      });

      builder.ConfigureAppConfiguration(config =>
      {
        config.AddInMemoryCollection(new Dictionary<string, string>
        {
          ["ConnectionStrings:DefaultConnection"] = ConnectionString,
          ["Storage:Queues:PaymentApproved"] = "payment-approved-unittests",
        });
      });
      return base.CreateHost(builder);
    }
  }
}


