using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace SqliteDb
{
    public class QueryTables
    {
        private readonly ILogger<QueryTables> _logger;

        public QueryTables(ILogger<QueryTables> log)
        {
            _logger = log;
        }

        [FunctionName("query-tables")]
        [OpenApiOperation(operationId: "GetQueryTablesResult", tags: new[] { "phrase", "count" })]
        [OpenApiParameter(name: "phrase", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Search Phrase** parameter")]
        [OpenApiParameter(name: "count", In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "The **Count Limit** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> GetQueryTablesResult(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string phrase = req.Query["phrase"];
            //string count = req.Query["count"];
            //var limit = int.TryParse(count, out int c) ? c : 5;
            var limit = req.Query["count"];

            var sqliteConnection = new SQLiteConnection(@"Data Source=Data\amazon_sample.db; Version = 3; New = True; Compress = True;");
            try
            {
                sqliteConnection.Open();
            }
            catch(Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            var response = new List<SearchTablesResult>();

            var sqliteCommand = sqliteConnection.CreateCommand();
            sqliteCommand.CommandText = $"SELECT Category, ProductName, Image FROM amazon_sample WHERE Category LIKE '%{phrase}%' LIMIT {limit}";
            var sqliteDataReader = await sqliteCommand.ExecuteReaderAsync();
            while(sqliteDataReader.HasRows)
            {
                var searchTablesResultItem = new SearchTablesResult();
                while(sqliteDataReader.Read())
                {
                    searchTablesResultItem.CategoryName = sqliteDataReader.GetString(0);
                    searchTablesResultItem.ProductName = sqliteDataReader.GetString(1);
                    searchTablesResultItem.ImageUrl = sqliteDataReader.GetString(2);
                    response.Add(searchTablesResultItem);
                }
                
            }
                        
            return new OkObjectResult(response);
        }
    }
}

