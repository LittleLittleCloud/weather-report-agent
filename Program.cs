using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
var weatherReportTool = new WeatherReportTool();
var weatherReportToolMiddleware = new FunctionCallMiddleware(
    functions: [weatherReportTool.GetWeatherReportFunctionContract],
    functionMap: new Dictionary<string, Func<string, Task<string>>>
    {
        [nameof(weatherReportTool.GetWeatherReport)] = weatherReportTool.GetWeatherReportWrapper
    });

using var client = new HttpClient(new CustomHttpClientHandler("http://localhost:11434"));
var option = new OpenAIClientOptions(OpenAIClientOptions.ServiceVersion.V2024_04_01_Preview)
{
    Transport = new HttpClientTransport(client),
};
// api-key is not required for local server
// so you can use any string here
var openAIClient = new OpenAIClient("api-key", option);
var model = "llama3.1";
var agent = new OpenAIChatAgent(
    openAIClient: openAIClient,
    name: "assistant",
    modelName: model,
    systemMessage: "You are weather assistant.",
    seed: 0)
    .RegisterMessageConnector()
    .RegisterMiddleware(weatherReportToolMiddleware)
    .RegisterPrintMessage()
    .RegisterMiddleware(async (msgs, option, agent, ct) => {
        var reply = await agent.GenerateReplyAsync(msgs, option, ct);
        // send tool call message back to agent to get further response
        if (reply is ToolCallAggregateMessage)
        {
            var chatHistory = msgs.Append(reply);
            return await agent.GenerateReplyAsync(chatHistory, option, ct);
        }

        return reply;
    });

var task = """
What is the weather in New York?
""";

await agent.SendAsync(task);
            
public sealed class CustomHttpClientHandler : HttpClientHandler
{
    private string _modelServiceUrl;

    public CustomHttpClientHandler(string modelServiceUrl)
    {
        _modelServiceUrl = modelServiceUrl;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.RequestUri = new Uri($"{_modelServiceUrl}{request.RequestUri.PathAndQuery}");

        return base.SendAsync(request, cancellationToken);
    }
}


public partial class WeatherReportTool
{
    /// <summary>
    /// Get the weather report for the given city
    /// </summary>
    /// <param name="city">city</param>
    /// <returns>weather report</returns>
    [Function]
    public async Task<string> GetWeatherReport(string city)
    {
        return """
        {
            "city": "New York",
            "temperature": "25°C",
            "weather": "Sunny"
        }
        """;
    }
}