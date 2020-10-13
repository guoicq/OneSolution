using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using OneSolution.Gmail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneSolution.Core.Log;
using ArkTrading.Service;
using OneSolution.Storage.Table;
using ArkTrading.DataAccess;

namespace ArkTrading.ConsoleApp
{
    class Program
    {
        public static IConfigurationRoot configuration;
        private static ServiceProvider serviceProvider;
        private static ILogger logger;

        static void Main(string[] args)
        {
            // Setup our DI
            serviceProvider = BuildServiceProvider();

            //configure console logging
            var logFactory = serviceProvider.GetService<ILoggerFactory>();
            Log.Init(logFactory, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            logger = logFactory.CreateLogger<Program>();
            logger.LogDebug("Starting application");

            try
            {
                serviceProvider.GetService<App>().Run(args).Wait();
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred: {ex}");
                Console.ReadLine();
                Environment.Exit(-1);
            }
            logger.LogInformation("Press ENTER to exit...");
            Console.ReadLine();
        }

        private static ServiceProvider BuildServiceProvider()
        {
            // Setup configuration
            configuration = BuildConfigiration();

            var services = new ServiceCollection();
            services
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                });

            ConfigureServices(services);

            return services.BuildServiceProvider();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {

            services.AddScoped<App>();
            services.AddScoped<IArkTradeService, ArkTradeService>();
            services.AddScoped<IArkTradeDataAccess, ArkTradeDataAccess>();
            services.AddScoped<IArkTradeExcelDataAccess, ArkTradeExcel97DataAccess>();
            services.AddScoped<IAzureTableRepository<ArkTradingEntity>>(p => new AzureTableRepository<ArkTradingEntity>(GetAzureTableSetting("ArkTrading")));
            return services;
        }

        private static AzureTableSetting GetAzureTableSetting(string key)
        {
            return new AzureTableSetting
            {
                AccountKey = configuration.GetValue<string>($"{key}.AccountKey"),
                AccountName = configuration.GetValue<string>($"{key}.AccountName"),
                TableName = configuration.GetValue<string>($"{key}.TableName"),
            };
        }


        private static IConfigurationRoot BuildConfigiration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true);
            return builder.Build();
        }


    }

}