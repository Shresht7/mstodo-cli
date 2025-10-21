// Library
using Microsoft.Graph;

// -------
// PROGRAM
// -------

class Program
{
    /// <summary>Name of the application</summary>
    static readonly string NAME = "mstodo-cli";

    /// <summary>Microsoft Graph Client</summary>
    static GraphServiceClient? client;

    /// <summary>Path to the application data folder</summary>
    static readonly string APP_DIR = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        NAME
    );

    // MAIN
    // ----

    static async Task Main(string[] args)
    {
        try
        {
            // Show help message if no arguments were provided, and return early
            if (args.Length == 0)
            {
                Console.WriteLine("ShowHelp");
                return;
            }

            // Load the AppSettings
            var settings = Settings.Load();

            // Initialize Microsoft Graph Client and login as user
            client = await AuthManager.InitMicrosoftGraph(APP_DIR, settings);

            // Switch on the subcommand and dispatch the corresponding action
            string command = args[0];
            switch (command.ToLower())
            {
                case "list":
                case "lists":
                    await ShowList(); break;
                case "show":
                case "view":
                    Console.WriteLine("ShowTask");
                    break;
                case "add":
                case "create":
                    Console.WriteLine("CreateTask");
                    break;
                case "complete":
                case "strike":
                case "done":
                    Console.WriteLine("CompleteTask");
                    break;
                case "delete":
                    Console.WriteLine("DeleteTask");
                    break;
                default:
                    Console.WriteLine("ShowHelp");
                    break;

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error:");
            Console.WriteLine(ex.ToString());
        }
    }

    // COMMANDS
    // --------

    static async Task ShowList()
    {
        // Get all the todo lists
        var lists = await client!.Me.Todo.Lists.GetAsync();

        Console.WriteLine("\n📋 Your Lists:\n");
        foreach (var list in lists!.Value!)
        {
            Console.WriteLine($" - {list.Id}\t{list.DisplayName}");
        }
    }
}
