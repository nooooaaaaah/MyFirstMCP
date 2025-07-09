# MyFirstMCP

This project is a Model Context Protocol (MCP) server written in C#. It provides a set of tools and APIs for interacting with plant data, PowerShell, GitHub profiles, and your Obsidian notes.

## Features

- **Plant Tools**: Query a large list of plants by name or get the full list. Plant data is fetched from a public JSON source and includes common and scientific names. You can search for specific plants, get all apples, or find the plant with the longest name.
- **PowerShell Tools**: Run PowerShell cmdlets and get the output directly from the MCP server. Useful for automation and system management tasks.
- **GitHub Profile Tools**: Fetch public GitHub profile information by username, including details like bio, repositories, and more.
- **Obsidian Tools**: Search and add notes to your Obsidian vault (default path: `C:\Users\Noah\Documents\stuff`). You can add new notes, search for keywords, or automate documentation workflows.
- **Echo Tools**: Simple echo and reverse-echo utilities for testing, fun, or string manipulation.

## Usage

1. Clone the repository and open in Visual Studio or VS Code.
2. Run the server with:
   ```
   dotnet run
   ```
3. Use the MCP client or your own integration to call the available tools. You can interact with the server using JSON-RPC or any compatible client.

## Example Plant Query

- Get all plants:
  - `GetPlants`
- Get a plant by name:
  - `GetPlant("Apple")`
- Find all apples:
  - Search for plants with "apple" in the name.
- Find the plant with the longest name:
  - Use a custom query or iterate through the plant list.

## Example Obsidian Note

- Add a note:
  - `AddToObsidianDocs("note-title", "note content")`
- Search notes:
  - `SearchObsidianNotes("keyword")`

## PowerShell Example

- Run a command:
  - `InvokeCmdlet("Get-Process")`

## GitHub Example

- Get a profile:
  - `GetGitHubProfile("octocat")`

## Requirements
- .NET 8.0 or later
- Internet access for plant and GitHub data
- Obsidian vault at `C:\Users\Noah\Documents\stuff` (for Obsidian tools; you can change the path in the code if needed)

## License
MIT


