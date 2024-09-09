using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

public static class ParseValuesFunction
{
    [FunctionName("ParseValues")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
    {
        string values = req.Query["values"];
        string attribute = req.Query["attribute"];

        if (string.IsNullOrEmpty(attribute))
        {
            return new BadRequestObjectResult("Attribute name is necessary");
        }

        var formatted = new[] { $"{attribute} IN ( " }
            .Concat(values.Split(',').Select(v => $"'{v}'"))
            .Concat(new[] { ")" });
        return new OkObjectResult(new { formatted = formatted.ToArray() });
    }
}
