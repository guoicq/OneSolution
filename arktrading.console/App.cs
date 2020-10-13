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
using ArkTrading.Service;

namespace ArkTrading.ConsoleApp
{
    class App
    {
        private readonly IConfiguration configuration;
        private readonly IArkTradeService processor;

        public App(IConfiguration configuration, IArkTradeService processor)
        {
            this.configuration = configuration;
            this.processor = processor;
        }

        public async Task Run(string[] args)
        {
            await processor.ProcessFiles();
            //await processor.ProcessYearToDate();
            //await processor.ProcessDate(new DateTime(2020, 8, 26));
            //await processor.ProcessMonth(new DateTime(2020, 8, 1));
        }
    }

}