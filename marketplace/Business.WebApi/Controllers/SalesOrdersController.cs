﻿using Business.SalesOrders.Abstractions.Clients;
using Business.SalesOrders.Abstractions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Business.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    public class SalesOrdersController : ControllerBase
    {
        SalesOrdersHttpApiClient queriesClient;
        SalesOrdersQueueClient commandsClient;

        public SalesOrdersController(SalesOrdersHttpApiClient queriesClient, SalesOrdersQueueClient commandsClient)
        {
            this.queriesClient = queriesClient;
            this.commandsClient = commandsClient;
        }

        [HttpGet("api/[controller]")]
        public async Task<IEnumerable<SalesOrder>> Get()
        {
            return await queriesClient.GetAsync();
        }

        [HttpGet("api/[controller]/{id}")]
        public async Task<SalesOrder> Get(string id)
        {
            return await queriesClient.GetFromIdAsync(id);
        }

        [HttpPost("api/[controller]")]
        public async Task Post([FromBody] SalesOrder salesOrder)
        {
            await commandsClient.CreateAsync(salesOrder);
        }

        [HttpPut("api/[controller]/{id}")]
        public async Task Put([FromBody] SalesOrder salesOrder)
        {
            await commandsClient.UpdateAsync(salesOrder);
        }

        //[HttpDelete("api/[controller]/{id}")]
        //public async Task Delete(string id)
        //{
        //    var item = await queriesClient.GetFromIdAsync(id);
        //    item.IsDeleted = true;
        //    await commandsClient.UpdateAsync(item);
        //}
    }
}
