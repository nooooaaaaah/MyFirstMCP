using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using myfirstmcp;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Management.Automation;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<MonkeyService>();
await builder.Build().RunAsync();

namespace myfirstmcp
{

    [McpServerToolType]
    public static class EchoTool
    {
        [McpServerTool, Description("Echoes the message back to the client.")]
        public static string Echo(string message) => $"Hello from C#: {message}";

        [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
        public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());

        [McpServerTool, Description("Tells Spencer he's an idiot.")]
        public static string SpencerIsIdiot(string message) => "Spencer, you're an idiot!";
    }

    [McpServerToolType]
    public static class MonkeyTools
    {
        [McpServerTool, Description("Get a list of monkeys.")]
        public static async Task<string> GetMonkeys(MonkeyService monkeyService)
        {
            var monkeys = await monkeyService.GetMonkeys();
            return JsonSerializer.Serialize(monkeys);
        }

        [McpServerTool, Description("Get a monkey by name.")]
        public static async Task<string> GetMonkey(MonkeyService monkeyService, [Description("The name of the monkey to get details for")] string name)
        {
            var monkey = await monkeyService.GetMonkey(name);
            return JsonSerializer.Serialize(monkey);
        }
    }

    [McpServerToolType]
    public static class PowerShellTools
    {
        [McpServerTool, Description("Executes a PowerShell cmdlet and returns the output.")]
        public static string InvokeCmdlet(
            [Description("The PowerShell cmdlet to execute, e.g. 'Get-Process'")] string cmdlet,
            [Description("Optional arguments for the cmdlet, as a single string")] string? arguments = null)
        {
            using var ps = PowerShell.Create();
            var command = string.IsNullOrWhiteSpace(arguments) ? cmdlet : $"{cmdlet} {arguments}";
            ps.AddScript(command);
            var results = ps.Invoke();
            if (ps.Streams.Error.Count > 0)
            {
                return string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
            }
            return string.Join("\n", results.Select(r => r.ToString()));
        }
    }

    public class MonkeyService
    {
        private readonly HttpClient httpClient;
        public MonkeyService(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient();
        }

        List<Monkey> monkeyList = new();
        public async Task<List<Monkey>> GetMonkeys()
        {
            if (monkeyList?.Count > 0)
                return monkeyList;

            var response = await httpClient.GetAsync("https://www.montemagno.com/monkeys.json");
            if (response.IsSuccessStatusCode)
            {
                monkeyList = await response.Content.ReadFromJsonAsync(MonkeyContext.Default.ListMonkey) ?? [];
            }

            monkeyList ??= [];

            return monkeyList;
        }

        public async Task<Monkey?> GetMonkey(string name)
        {
            var monkeys = await GetMonkeys();
            return monkeys.FirstOrDefault(m => m.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
        }
    }

    public partial class Monkey
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? Details { get; set; }
        public string? Image { get; set; }
        public int Population { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    [JsonSerializable(typeof(List<Monkey>))]
    internal sealed partial class MonkeyContext : JsonSerializerContext
    {

    }
}