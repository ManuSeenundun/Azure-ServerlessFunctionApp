using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Linq;

namespace ManuTestFunctionApp
{
    public static class ManuTestFunctionApp
    {
        [FunctionName("ManuTestFunctionApp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Function invoked");

            // We only support POST requests
            if (req.Method == "GET")
                return new BadRequestResult();

            // grab the key and URI from the portal config
            var visionKey = Environment.GetEnvironmentVariable("VisionKey");
            var visionEndpoint = Environment.GetEnvironmentVariable("VisionEndpoint");

            // create a client and request Tags for the image submitted
            var vsc = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionKey))
            {
                Endpoint = visionEndpoint
            };

            ImageDescription result = null;

            // We read the content as a byte array and assume it's an image
            if (req.Method == "POST")
            {
                try
                {
                    result = await vsc.DescribeImageInStreamAsync(req.Body);
                }
                catch { }
            }

            // if we didn't get a result from the service, return a 400
            if (result == null)
                return new BadRequestResult();

            var bestResult = result.Captions.OrderByDescending(c => c.Confidence).FirstOrDefault()?.Text;
            return new OkObjectResult(bestResult
                        ?? "I'm at a loss for words... I can't describe this image!");
        }
    }
}
