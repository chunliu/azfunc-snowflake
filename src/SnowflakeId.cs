using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Cosmos.Table;

namespace chunliu.demo
{
    public class WorkerIdEntity : TableEntity
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
    public static class SnowflakeId
    {
        [FunctionName("SnowflakeId")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [Table("WorkerIds", "workerId", Connection = "WorkerIdsTable")] CloudTable workerIds,
            ILogger log)
        {
            string partitionKey = "workerId";
            var datacenterId = int.Parse(Environment.GetEnvironmentVariable("DATACENTER_ID"));
            var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            var workerId = 0;
            if (instanceId != null)
            {
                log.LogInformation($"Instance Id: {instanceId}");
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
                log.LogInformation($"Worker Id: {workerId}");
            }
            
            SnowflakeIdWorker worker = new SnowflakeIdWorker(workerId, datacenterId);
            var id = worker.NextId();
            log.LogInformation($"The generated Id: {id}");

            return new OkObjectResult(
                new JObject(
                    new JProperty("datacenterId", datacenterId),
                    new JProperty("workerId", workerId),
                    new JProperty("id", id)
                )
            );
        }
    }
}
