using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

public static class FormatSqlFunction
{
    [FunctionName("FormatSql")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        string table = req.Query["table"];
        if (string.IsNullOrEmpty(table))
        {
            return new BadRequestObjectResult("Table name is required");
        }

        string requestBody = new StreamReader(req.Body).ReadToEnd();
        using JsonDocument doc = JsonDocument.Parse(requestBody);
        JsonElement root = doc.RootElement;

        if (!root.TryGetProperty("data", out JsonElement dataArray) || dataArray.ValueKind != JsonValueKind.Array)
        {
            return new BadRequestObjectResult("Invalid JSON structure. Expected 'data' array.");
        }

        var sqlCommands = new List<string>();
        foreach (JsonElement item in dataArray.EnumerateArray())
        {
            var parameters = new List<string>();
            foreach (JsonProperty prop in item.EnumerateObject())
            {
                string columnName = prop.Name;
                string columnValue = prop.Value.GetString() ?? string.Empty;
                parameters.Add($"@{columnName} = N'{EscapeSqlString(columnValue)}'");
            }

            string command = $"EXECUTE dbo.[{EscapeSqlIdentifier(table)}] {string.Join(", ", parameters)};";
            sqlCommands.Add(command);
        }

        return new OkObjectResult(string.Join(Environment.NewLine, sqlCommands));
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
