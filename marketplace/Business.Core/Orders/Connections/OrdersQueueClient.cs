﻿using Azure.Messaging.ServiceBus;
using Business.Core.Orders.Models;
using Business.Shared.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Business.Core.Orders.Connections
{
    public class OrdersQueueClient : BaseQueueClient<Order>
    {
        public OrdersQueueClient(
            IConfiguration configuration, 
            ILogger<OrdersQueueClient> logger
            ) : base(configuration, logger)
        {
            resourceName = "salesorders";
            sender = client.CreateSender(resourceName);
        }

        public ServiceBusProcessor GetSalesOrderMessageProcessor()
            => client.CreateProcessor(resourceName, new ServiceBusProcessorOptions());
    }
}
