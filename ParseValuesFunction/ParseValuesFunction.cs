using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

public static class ParseValuesFunction
{
    [Function("ParseValuesFunction")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("ParseValuesFunction");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string values = query["values"];
        string attribute = query["attribute"];

        if (string.IsNullOrEmpty(values) || string.IsNullOrEmpty(attribute))
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Please provide both 'values' and 'attr' query parameters.");
            return badRequestResponse;
        }

        var formatted = new[] { $"{attribute} IN ( " }
            .Concat(values.Split(',').Select(v => $"'{v}'"))
            .Concat(new[] { ")" });

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { formatted = formatted.ToArray() });

        return response;
    }
}
