using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

public static class FormatSqlFunction
{
    [Function("FormatSqlFunction")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("FormatSqlFunction");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string table = query["table"];

        if (string.IsNullOrEmpty(table))
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Please provide the 'tbl' query parameter.");
            return badRequestResponse;
        }

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(requestBody);

        if (data == null || !data.ContainsKey("data"))
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid JSON structure. Expected 'data' array.");
            return badRequestResponse;
        }

        var sqlCommands = new List<string>();
        foreach (var item in data["data"])
        {
            var parameters = new List<string>();
            foreach (var kvp in item)
            {
                parameters.Add($"@{kvp.Key} = N'{EscapeSqlString(kvp.Value)}'");
            }

            string command = $"EXECUTE dbo.[{EscapeSqlIdentifier(table)}] {string.Join(", ", parameters)};";
            sqlCommands.Add(command);
        }

        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteStringAsync(string.Join("\n", sqlCommands));

        return response;
    }

    private static string EscapeSqlString(string input)
    {
        return input.Replace("'", "''");
    }

    private static string EscapeSqlIdentifier(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        return input.Replace("]", "]]");
    }
}
