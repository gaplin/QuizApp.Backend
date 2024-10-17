using Meziantou.Extensions.Logging.Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using QuizApp.API;
using QuizApp.Infrastructure.DbSettings;
using Testcontainers.MongoDb;
using Xunit.Abstractions;

namespace QuizApp.Tests.Fixtures;

public sealed class QuizApiFixture : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDb = new MongoDbBuilder().WithImage("mongo:8.0.1").Build();

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

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _mongoDb.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _mongoDb.StartAsync();
    }
}