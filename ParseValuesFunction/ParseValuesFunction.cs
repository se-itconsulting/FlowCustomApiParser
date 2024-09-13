using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Linq;
using System.Text;
using System.Net;

public static class ParseValuesFunction
{
    [Function("ParseValuesFunction")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("ParseValuesFunction");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        // Step 1: Get the attribute from the query string
        var query = HttpUtility.ParseQueryString(req.Url.Query);
        var attribute = query["attribute"] ?? string.Empty;

        // Step 2: Read the values from the request body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var values = requestBody.Trim();

        // Step 3: Validate input
        if (string.IsNullOrEmpty(values) || string.IsNullOrEmpty(attribute))
        {
            return await CreateBadRequestResponse(req);
        }

        // Step 4: Format the result
        var formattedResult = FormatResult(attribute, values);

        // Step 5: Create and send the response
        return await CreateSuccessResponse(req, formattedResult);
    }

    private static async Task<HttpResponseData> CreateBadRequestResponse(HttpRequestData req)
    {
        var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
        await badRequestResponse.WriteStringAsync("Please provide both 'values' in the request body and 'attribute' as a query parameter.");
        return badRequestResponse;
    }

    private static string FormatResult(string attribute, string values)
    {
        var sb = new StringBuilder();
        sb.Append("{\"formatted\":[");

        sb.Append($"\"{attribute} IN ( \",");

        var valueArray = values.Split(',').Select(v => v.Trim()).ToArray();
        for (int i = 0; i < valueArray.Length; i++)
        {
            sb.Append($"\"'{valueArray[i]}'\"");
            if (i < valueArray.Length - 1)
            {
                sb.Append(",\", \",");
            }
        }

        sb.Append(",\")\"]}");
        return sb.ToString();
    }

    private static async Task<HttpResponseData> CreateSuccessResponse(HttpRequestData req, string content)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(content);
        return response;
    }
}
