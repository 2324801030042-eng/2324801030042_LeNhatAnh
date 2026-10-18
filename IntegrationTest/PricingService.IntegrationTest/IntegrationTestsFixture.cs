using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alba;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PricingService.Api.Commands;
using PricingService.Commands;
using PricingService.Data;
using PricingService.Domain;
using Steeltoe.Discovery.Client;
using Testcontainers.PostgreSql;
using Xunit;

namespace PricingService.IntegrationTest;

public class IntegrationTestsFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer pgSqlContainer = new PostgreSqlBuilder()
        .WithDatabase("lab_netmicro_pricing")
        .WithCleanUp(true)
        .Build();

    public IAlbaHost SystemUnderTest { get; private set; }

    public async Task InitializeAsync()
    {
        await pgSqlContainer.StartAsync();

        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureServices(services =>
                {
                    var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            ["spring:application:name"] = "PricingService",
                            ["eureka:client:serviceUrl"] = "http://localhost:8761/eureka",
                            ["eureka:instance:port"] = "5040",
                            ["ConnectionStrings:DefaultConnection"] = pgSqlContainer.GetConnectionString()
                        }).Build();

                    services.AddDiscoveryClient(config);

                    services.AddControllers()
                        .AddNewtonsoftJson();

                    services.AddMediatR(cfg =>
                    {
                        cfg.RegisterServicesFromAssembly(typeof(CalculatePriceHandler).Assembly);
                        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                    });

                    services.AddTransient<IValidator<CalculatePriceCommand>, CalculatePriceCommandValidator>();

                    services.AddSingleton<InMemoryDataStore>(sp =>
                    {
                        var store = new InMemoryDataStore();
                        store.Tariffs["TRI"] = TariffFactory.Travel();
                        store.Tariffs["HSI"] = TariffFactory.House();
                        store.Tariffs["FAI"] = TariffFactory.Farm();
                        store.Tariffs["CAR"] = TariffFactory.Car();
                        return store;
                    });
                    services.AddSingleton<IDataStore>(sp => sp.GetRequiredService<InMemoryDataStore>());
                })
                .ConfigureAppConfiguration((ctx, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ConnectionStrings:DefaultConnection"] = pgSqlContainer.GetConnectionString(),
                        ["spring:application:name"] = "PricingService",
                        ["eureka:client:serviceUrl"] = "http://localhost:8761/eureka",
                        ["eureka:instance:port"] = "5040"
                    });
                })
                .Configure(app =>
                {
                    var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                    if (env.IsDevelopment())
                    {
                        app.UseDeveloperExceptionPage();
                    }
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
            });

        SystemUnderTest = new AlbaHost(hostBuilder);
    }

    public async Task DisposeAsync()
    {
        await SystemUnderTest.DisposeAsync();
        await pgSqlContainer.DisposeAsync();
    }

    protected virtual void SetupServices(HostBuilderContext ctx, IServiceCollection services)
    {
    }

    protected Task SetupData()
    {
        return Task.CompletedTask;
    }
}