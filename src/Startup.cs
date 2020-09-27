using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(chunliu.demo.Startup))]

namespace chunliu.demo
{
    class WorkerIdEntity : TableEntity
    {
        public WorkerIdEntity()
        {

        }
        public WorkerIdEntity(string partitionKey, string instanceId)
        {
            PartitionKey = partitionKey;
            RowKey = instanceId;
        }

        public int WorkerId { get; set; }
    }

    public class Startup : FunctionsStartup
    {
        private async Task<int> GetWorkerId()
        {
            int workerId = 0;
            var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            if (instanceId == null)
                return workerId;

            var tableConnStr = Environment.GetEnvironmentVariable($"AzureWebJobsWorkerIdsTable");
            if (string.IsNullOrEmpty(tableConnStr))
            {
                throw new ArgumentNullException("Table storage connection string is missing.");
            }

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(tableConnStr);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable workerIds = tableClient.GetTableReference("WorkerIds");
            await workerIds.CreateIfNotExistsAsync();
            string partitionKey = "workerId";
            TableOperation retrieve = TableOperation.Retrieve<WorkerIdEntity>(partitionKey, instanceId);
            TableResult result = await workerIds.ExecuteAsync(retrieve);
            WorkerIdEntity entity = result.Result as WorkerIdEntity;
            if (entity == null)
            {
                var records = workerIds.ExecuteQuery(new TableQuery<WorkerIdEntity>()).ToList().Count;
                workerId = records;
                WorkerIdEntity newEntity = new WorkerIdEntity(partitionKey, instanceId);
                newEntity.WorkerId = workerId;
                TableOperation insert = TableOperation.InsertOrReplace(newEntity);
                result = await workerIds.ExecuteAsync(insert);
            }
            else
            {
                workerId = entity.WorkerId;
            }

            return workerId;
        }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var workerId = GetWorkerId().GetAwaiter().GetResult();
            var datacenterId = int.Parse(Environment.GetEnvironmentVariable("DATACENTER_ID"));
            builder.Services.AddSingleton<ISnowflakeIdWorker>(
                sp => new SnowflakeIdWorker(workerId, datacenterId)
            );
        }
    }
}