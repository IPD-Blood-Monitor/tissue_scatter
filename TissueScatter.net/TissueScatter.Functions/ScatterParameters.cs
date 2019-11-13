using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TissueScatter.Core;

namespace TissueScatter.Functions
{
    public static class ScatterParameters
    {
        [FunctionName("ScatterParameters")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("ScatterParameter called");

            string name = req.Query["name"];

            uint wavelength = Convert.ToUInt32(req.Query["wavelength"]);
            double distanceToDetector1 = Convert.ToDouble(req.Query["distanceTo1"]);
            double distanceToDetector2 = Convert.ToDouble(req.Query["distanceTo2"]);
            double width = Convert.ToDouble(req.Query["halfWidth"]);
            double thicknessSkin = Convert.ToDouble(req.Query["thicknessSkin"]);
            double thicknessMuscle = Convert.ToDouble(req.Query["thicknessMuscle"]);
            double thicknessBone = Convert.ToDouble(req.Query["thicknessBone"]);
            double concentrationBlood = Convert.ToDouble(req.Query["concentrationBlood"]);
            double ratio = Convert.ToDouble(req.Query["ratio"]);

            if (req.Method == "POST")
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var body = JsonConvert.DeserializeObject<Core.ScatterParameters>(requestBody);

                try
                {
                    var data = Scatter.Scatterlight(body, context.FunctionAppDirectory);
                    return new OkObjectResult(data);
                }
                catch (Exception e)
                {
                    log.LogError(e, "Excpetion occured in ScatterLight function");
                    return new ExceptionResult(e, false);
                }
            }
            else
            {
                try
                {
                    var data = Scatter.Scatterlight(wavelength, distanceToDetector1, distanceToDetector2, width,
                        thicknessSkin, thicknessMuscle, thicknessBone, concentrationBlood, ratio, context.FunctionAppDirectory);
                    return new OkObjectResult(data);
                }
                catch (Exception e)
                {
                    log.LogError(e, "Excpetion occured in ScatterLight function");
                    return new ExceptionResult(e, false);
                }
            }
        }
    }
}
