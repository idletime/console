﻿using Business.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Db.Abstractions
{
    public abstract class AzureStorageTableRepository<T> : ITableRepository<T> where T : BaseModel
    {
        private const string AzureTableStorageConnectionString = "Azure:TableStorage:ConnectionString";
        
        private CloudStorageAccount cloudStorageAccount;
        private CloudTableClient cloudTableClient;

        protected CloudTable cloudTable;
        protected ILogger<AzureStorageTableRepository<T>> Logger { get; set; }
        protected string ResourceName { get; set; }
        
        public AzureStorageTableRepository(IConfiguration configuration, ILogger<AzureStorageTableRepository<T>> logger)
        {
            var connectionString = configuration.GetSection(AzureTableStorageConnectionString).Value;
            cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            this.Logger = logger;
        }

        public AzureStorageTableRepository()  { } // required for http trigggers

        private async Task<T> UpsertItem(T item, TableOperation operation, string rowId = null, string partitionKey = null)
        {
            try
            {
                item.PartitionKey ??= partitionKey ?? ResourceName;
                item.RowKey = rowId ?? item.Id;
                //item.ETag ??= ResourceName;
                item.Timestamp = DateTime.UtcNow;
                cloudTable ??= cloudTableClient.GetTableReference(ResourceName);
                var result = await cloudTable.ExecuteAsync(operation);
                T insertItem = result.Result as T;
                return insertItem;
            }
            catch (StorageException ex)
            {
                Logger.LogError($"{ex.RequestInformation.ExtendedErrorInformation}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}");
            }
            finally { }
            return item;
        }

        public async Task<T> CreateItemAsync(T item, string rowId = null, string partitionKey = null) => await UpsertItem(item, TableOperation.InsertOrReplace(item), rowId, partitionKey);

        public async Task<T> UpdateItemAsync(T item, string rowId = null, string partitionKey = null) => await UpsertItem(item, TableOperation.InsertOrReplace(item), rowId, partitionKey);

        public Task<T> DeleteItemAsync(T item, string rowId = null, string partitionKey = null) => throw new NotImplementedException();
       
        
    }
}
