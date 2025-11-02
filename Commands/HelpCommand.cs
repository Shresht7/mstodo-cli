using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace mstodo_cli.Commands
{
    public class HelpCommand : ICommand
    {
        /// <summary>Name of the application</summary>
        static readonly string NAME = "mstodo-cli";

        public async Task ExecuteAsync(CommandContext context)
        {
            Console.WriteLine($"Usage: {NAME} <command> [arguments] [--json]");
            Console.WriteLine("");

            Console.WriteLine("A command-line-interface to interact with Microsoft To Do ☑️");
            Console.WriteLine("");

            Console.WriteLine("Global Options:");
            Console.WriteLine("  --json      Output in JSON format");
            Console.WriteLine("");

            Console.WriteLine("Commands:");
            Console.WriteLine("  login       Login with Microsoft Graph Services");
            Console.WriteLine("  logout      Logout from Microsoft Graph Services");
            Console.WriteLine("  user        Show current user information");
            Console.WriteLine("");

            Console.WriteLine("  lists                              Show your todo lists");
            Console.WriteLine("  show <list> [--limit <number>]     Show tasks in a todo list");
            Console.WriteLine("  add <list> <title>                 Add a new task to a specific list");
            Console.WriteLine("  complete <list> <task>             Complete a task in a specific list");
            Console.WriteLine("  delete <list> <task>               Delete a task in a specific list");
            Console.WriteLine("");

            Console.WriteLine("  help        Show this help message");
        }
    }
}