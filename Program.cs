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
builder.Services.AddSingleton<PlantService>();
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
    public static class PlantTools
    {
        [McpServerTool, Description("Get a list of plants.")]
        public static async Task<string> GetPlants(PlantService plantService)
        {
            var plants = await plantService.GetPlants();
            return JsonSerializer.Serialize(plants);
        }

        [McpServerTool, Description("Get a plant by name.")]
        public static async Task<string> GetPlant(PlantService plantService, [Description("The name of the plant to get details for")] string name)
        {
            var plant = await plantService.GetPlant(name);
            return JsonSerializer.Serialize(plant);
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

    [McpServerToolType]
    public static class GitHubProfileTools
    {
        [McpServerTool, Description("Gets information about a GitHub profile by username.")]
        public static async Task<string> GetGitHubProfile(
            [Description("The GitHub username to look up.")] string username,
            IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyFirstMCP-Agent");
            var url = $"https://api.github.com/users/{username}";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return $"Could not fetch profile for '{username}'. Status: {response.StatusCode}";
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
    }

    [McpServerToolType]
    public static class ObsidianTools
    {
        private const string DefaultVaultPath = @"C:\\Users\\Noah\\Documents\\stuff";

        [McpServerTool, Description("Searches your Obsidian vault for notes containing a keyword.")]
        public static string SearchObsidianNotes(
            [Description("The keyword to search for")] string keyword)
        {
            return SearchObsidianNotesWithPath(DefaultVaultPath, keyword);
        }

        [McpServerTool, Description("Searches your Obsidian vault for notes containing a keyword (custom path).")]
        public static string SearchObsidianNotesWithPath(
            [Description("The path to your Obsidian vault directory")] string vaultPath,
            [Description("The keyword to search for")] string keyword)
        {
            if (!Directory.Exists(vaultPath))
                return $"Vault directory not found: {vaultPath}";

            var results = new List<string>();
            foreach (var file in Directory.EnumerateFiles(vaultPath, "*.md", SearchOption.AllDirectories))
            {
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        results.Add($"{Path.GetFileName(file)} (line {i + 1}): {lines[i].Trim()}");
                    }
                }
            }
            if (results.Count == 0)
                return $"No notes found containing '{keyword}'.";
            return string.Join("\n", results);
        }

        [McpServerTool, Description("Adds a new note to your Obsidian vault.")]
        public static string AddToObsidianDocs(
            [Description("The note title (filename, without .md)")] string title,
            [Description("The note content")] string content)
        {
            var filePath = Path.Combine(DefaultVaultPath, title + ".md");
            File.WriteAllText(filePath, content);
            return $"Note '{title}' added to Obsidian vault.";
        }
    }

    public class PlantService
    {
        private readonly HttpClient httpClient;
        public PlantService(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient();
        }

        List<Plant> plantList = new();
        public async Task<List<Plant>> GetPlants()
        {
            if (plantList?.Count > 0)
                return plantList;

            var response = await httpClient.GetAsync("https://raw.githubusercontent.com/dariusk/corpora/master/data/plants/plants.json");
            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(jsonDoc);
                plantList = doc.RootElement.GetProperty("plants").EnumerateArray()
                    .Select(p => new Plant {
                        Name = p.TryGetProperty("name", out var n) ? n.GetString() : "Unknown",
                        Species = p.TryGetProperty("species", out var s) ? s.GetString() : null
                    }).ToList();
            }
            plantList ??= [];
            return plantList;
        }

        public async Task<Plant?> GetPlant(string name)
        {
            var plants = await GetPlants();
            return plants.FirstOrDefault(p => p.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
        }
    }

    public partial class Plant
    {
        public string? Name { get; set; }
        public string? Species { get; set; }
    }

    [JsonSerializable(typeof(List<Plant>))]
    internal sealed partial class PlantContext : JsonSerializerContext
    {
    }
}