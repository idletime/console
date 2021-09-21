using Azure.Messaging.ServiceBus;
using Business.Abstractions;
using Business.Db;
using Business.ExchangeRates.Abstractions.Clients;
using Business.ExchangeRates.Abstractions.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Business.ExchangeRates.BackgroundServices
{
    public class CurrenciesService : BackgroundService
    {
        private readonly ILogger<CurrenciesService> logger;
        private ServiceBusProcessor busProc;
        private CurrenciesQueueClient sbClient;
        private CurrenciesRepository dbRepository;

        public CurrenciesService(
            ILogger<CurrenciesService> logger,
            CurrenciesQueueClient sbClient,
            CurrenciesRepository dbRepository
            )
        {
            this.logger = logger;
            this.dbRepository = dbRepository;
            this.sbClient = sbClient;
            busProc = this.sbClient.GetCurrenciesMessageProcessor();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            busProc.ProcessMessageAsync += MessageHandler;
            busProc.ProcessErrorAsync += ErrorHandler;
            await sbClient.StartHubConnection(cancellationToken);
            await base.StartAsync(cancellationToken);
            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            busProc.ProcessMessageAsync -= MessageHandler;
            busProc.ProcessErrorAsync -= ErrorHandler;
            await busProc.StopProcessingAsync();
            await sbClient.HubConnection.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
            await Task.CompletedTask;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(500, stoppingToken);
                await busProc.StartProcessingAsync();
                logger.LogInformation("Currencies worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.CompletedTask;
        }

        async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            logger.LogInformation($"Received: {body}");

            var itemcmd = System.Text.Json.JsonSerializer.Deserialize<CurrencyCommand>(body);
            
            itemcmd.Item.CurrencyId = itemcmd.Item.Id;
            
            if (itemcmd.Command == CommandEnum.Add)
                await dbRepository.CreateItemAsync(itemcmd.Item, itemcmd.Item.Id);
            else if (itemcmd.Command == CommandEnum.Edit)
                await dbRepository.UpdateItemAsync(itemcmd.Item, itemcmd.Item.Id);
                
            await args.CompleteMessageAsync(args.Message);
            await sbClient.HubConnection.SendAsync(
                "SendMessageOnCurrencies",
                "Currencies Done",
                DateTime.Now.ToLongTimeString()
                );
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            logger.LogError(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
