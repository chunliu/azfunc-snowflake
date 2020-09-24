using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace chunliu.demo
{
    public static class SnowflakeId
    {
        [FunctionName("SnowflakeId")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            SnowflakeIdWorker worker = new SnowflakeIdWorker(1, 1);
            var id = worker.NextId();

            return new OkObjectResult($"{{'id': {id} }}");
        }
    }
}
