using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace chunliu.demo
{
    public class SnowflakeId
    {
        private ISnowflakeIdWorker worker = null;

        public SnowflakeId(ISnowflakeIdWorker worker)
        {
            this.worker = worker;
        }

        [FunctionName("SnowflakeId")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {   
            var id = worker.NextId();
            log.LogInformation($"The generated Id: {id}");

            return new OkObjectResult(
                new JObject(
                    new JProperty("datacenterId", worker.DatacenterId),
                    new JProperty("workerId", worker.WorkerId),
                    new JProperty("id", id)
                )
            );
        }
    }
}
