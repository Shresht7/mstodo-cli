// Library
using Microsoft.Graph;
using Microsoft.Graph.Models;

// -------
// PROGRAM
// -------

class Program
{
    /// <summary>Name of the application</summary>
    static readonly string NAME = "mstodo-cli";

    /// <summary>Path to the application data folder</summary>
    static readonly string APP_DIR = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        NAME
    );

    /// <summary>Application settings</summary>
    static Settings settings = Settings.Load();

    /// <summary>Cache for todo lists: DisplayName -> TodoTaskList</summary>
    static Dictionary<string, TodoTaskList> todoListsMap = new Dictionary<string, TodoTaskList>();

    // MAIN
    // ----

    static async Task Main(string[] args)
    {
        // Set the output encoding to UTF-8
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        try
        {
            // Get the output formatter
            IOutputFormatter formatter = OutputManager.GetFormatter(args.ToList());

            // Create the command context
            CommandContext context = new CommandContext(settings, formatter, args.Skip(1).ToList(), APP_DIR, todoListsMap);

            // Switch on the subcommand and dispatch the corresponding action
            string command = args.Length > 0 ? args[0].ToLower() : string.Empty;

            switch (command)
            {
                case "login":
                    await new LoginCommand().ExecuteAsync(context); break;
                case "logout":
                    await new LogoutCommand().ExecuteAsync(context); break;
                case "user":
                    await new UserCommand().ExecuteAsync(context); break;

                case "lists":
                    await new ListsCommand().ExecuteAsync(context); break;
                case "list":
                case "show":
                case "view":
                    await new ShowCommand().ExecuteAsync(context); break;
                case "add":
                case "create":
                    await new AddCommand().ExecuteAsync(context); break;
                case "complete":
                case "strike":
                case "done":
                    await new CompleteCommand().ExecuteAsync(context); break;
                case "delete":
                    await new DeleteCommand().ExecuteAsync(context); break;
                case "help":
                case "--help":
                case "-h":
                default:
                    ShowHelp(); break;

            }
        }
        catch (Exception ex)
        {
            ErrorHandler.HandleException(ex, APP_DIR);
        }
    }

    static void ShowHelp()
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
        Console.WriteLine("              [--skip <number>]      Skip a number of tasks");
        Console.WriteLine("              [--filter <string>]    Filter tasks (e.g., \"status eq 'completed'\")");
        Console.WriteLine("              [--search <string>]    Search for tasks by title and description");
        Console.WriteLine("              [--orderby <string>]   Sort tasks (e.g., \"createdDateTime desc\")");
        Console.WriteLine("              [--important]          Show only high importance tasks");
        Console.WriteLine("  add <list> <title>                 Add a new task to a specific list");
        Console.WriteLine("  complete <list> <task>             Complete a task in a specific list");
        Console.WriteLine("  delete <list> <task>               Delete a task in a specific list");
        Console.WriteLine("");

        Console.WriteLine("  help        Show this help message");
    }
}
