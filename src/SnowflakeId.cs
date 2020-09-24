using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace chunliu.demo
{
    public static class SnowflakeId
    {
        [FunctionName("SnowflakeId")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var datacenterId = int.Parse(Environment.GetEnvironmentVariable("DATACENTER_ID"));
            var instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
            var workerId = int.Parse(instanceId ??= "0");
            
            log.LogInformation(instanceId.ToString());
            SnowflakeIdWorker worker = new SnowflakeIdWorker(workerId, datacenterId);
            var id = worker.NextId();

            var ret = new JObject(
                new JProperty("id", id)
            );
            return new OkObjectResult(ret);
        }
    }
}
