using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QuizApp.API;
using QuizApp.Infrastructure.DbSettings;
using Testcontainers.MongoDb;

namespace QuizApp.Tests.Fixtures;

public sealed class QuizApiFixture : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDb = new MongoDbBuilder("mongo:8.0.1").Build();

    public ITestOutputHelper? OutputHelper { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(opts =>
        {
            opts.ClearProviders();

            opts.Services.AddSingleton<ILoggerProvider>(_ => OutputHelper is null ? NullLoggerProvider.Instance : new XUnitLoggerProvider(OutputHelper));
        });

        builder.ConfigureTestServices(services =>
        {
            services.Configure<MongoDbSettings>(opts =>
            {
                opts.ConnectionString = _mongoDb.GetConnectionString();
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoDb.DisposeAsync();
        await base.DisposeAsync();
    }

    public async ValueTask InitializeAsync()
    {
        await _mongoDb.StartAsync();
    }
}