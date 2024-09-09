using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Globalization;

public static class ParseValueFunction
{
    [FunctionName("ParseValue")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
    {
        string requestBody = new StreamReader(req.Body).ReadToEnd();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var input = JsonSerializer.Deserialize<ParseValueInput>(requestBody, options);

            if (input == null || string.IsNullOrWhiteSpace(input.Value) || string.IsNullOrWhiteSpace(input.Type))
            {
                return new BadRequestObjectResult("Invalid input. Both 'value' and 'type' are required.");
            }

            object result = ParseValueByType(input.Value, input.Type);

            return new OkObjectResult(new { ParsedValue = result });
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Invalid JSON format.");
        }
        catch (FormatException ex)
        {
            return new BadRequestObjectResult($"Parsing error: {ex.Message}");
        }
        catch
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private static object ParseValueByType(string value, string type)
    {
        return type.ToLowerInvariant() switch
        {
            "int" or "integer" => int.Parse(value, CultureInfo.InvariantCulture),
            "double" or "float" => double.Parse(value, CultureInfo.InvariantCulture),
            "decimal" => decimal.Parse(value, CultureInfo.InvariantCulture),
            "datetime" => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal),
            "bool" or "boolean" => bool.Parse(value),
            "string" => value,
            _ => throw new ArgumentException($"Unsupported type: {type}"),
        };
    }
}

public record ParseValueInput
{
    public string? Value { get; set; }
    public string? Type { get; set; }
}
