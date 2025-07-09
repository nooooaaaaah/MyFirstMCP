# MyFirstMCP

A Model Context Protocol (MCP) server implementation with various tools for demonstration and utility purposes.

## Features

This MCP server provides the following tools:

### Echo Tools
- **Echo** - Echoes back the provided message
- **ReverseEcho** - Echoes back the provided message in reverse

### Fun Tools
- **SpencerIsIdiot** - Tells Spencer he's an idiot (returns a JSON response)

### Monkey Tools
- **GetMonkeys** - Returns a list of monkeys
- **GetMonkey** - Gets details about a specific monkey by name

### PowerShell Tools
- **InvokeCmdlet** - Executes PowerShell cmdlets and returns the output

## Setup and Configuration

### Prerequisites
- .NET 8.0 SDK
- Visual Studio Code with MCP extension

### Environment Configuration
Create a `.env` file in the root directory with the following settings:
```
ENABLED_FEATURES=EchoTool,ReverseEcho,SpencerIsIdiot,MonkeyTools,PowerShellTools
LOG_LEVEL=Info
```

### VS Code Configuration
Ensure your `.vscode/mcp.json` file contains:
```json
{
    "commands": {
        "backend": "dotnet run"
    }
}
```

## Building and Running

1. Install required NuGet packages:
   ```powershell
   dotnet add package Microsoft.PowerShell.SDK
   ```

2. Build the project:
   ```powershell
   dotnet build
   ```

3. Run the MCP server:
   ```powershell
   dotnet run
   ```

## Tool Usage Examples

### Echo
```json
{
  "message": "Hello, world!"
}
```

### ReverseEcho
```json
{
  "message": "Hello, world!"
}
```
Response: `!dlrow ,olleH`

### GetMonkeys
Returns a list of all monkeys.

### GetMonkey
```json
{
  "name": "Capuchin"
}
```

### InvokeCmdlet
```json
{
  "cmdlet": "Get-Process",
  "arguments": "-Name explorer"
}
```

