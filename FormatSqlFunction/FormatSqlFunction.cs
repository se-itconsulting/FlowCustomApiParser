using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FlowCustomApiSapParser 
{
    public static class FormatSqlFunction
    {
        /// <summary>
        /// Processes an HTTP request to format SQL data.
        /// </summary>
        /// <param name="req">The HTTP request data.</param>
        /// /// <param name="executionContext">The function execution context.</param>
        /// <returns>The HTTP response data.</returns>
        [Function("FormatSqlFunction")]
        public static HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("FormatSqlFunction");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            string? table = query?["table"];

            if (string.IsNullOrEmpty(table))
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                badRequestResponse.WriteString("Please provide the 'table' query parameter.");
                return badRequestResponse;
            }

            string requestBody;
            using (var reader = new StreamReader(req.Body))
            {
                requestBody = reader.ReadToEnd();
            }

            var data = JsonSerializer.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(requestBody);

            if (data == null || !data.TryGetValue("data", out var dataList))
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                badRequestResponse.WriteString("Invalid request body.");
                return badRequestResponse;
            }

            // Process dataList and generate the response
            // ...

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.WriteString("Request processed successfully.");
            return response;
        }
    }
}