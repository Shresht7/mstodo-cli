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

    static Settings settings = Settings.Load();

    // MAIN
    // ----

    static async Task Main(string[] args)
    {
        try
        {
            // Switch on the subcommand and dispatch the corresponding action
            string command = args.Length > 0 ? args[0].ToLower() : string.Empty;

            switch (command)
            {
                case "login":
                    await Login(); break;
                case "logout":
                    await Logout(); break;
                case "user":
                    await ShowUser(); break;
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
                case "help":
                case "--help":
                case "-h":
                default:
                    ShowHelp();
                    break;

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error:");
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>Ensure that we are authenticated, and prompt to login if we are not</summary>
    private static async Task EnsureAuthentication()
    {
        client ??= await AuthManager.Login(APP_DIR, settings);
    }

    // COMMANDS
    // --------

    /// <summary>Authenticate with Microsoft Graph to login</summary>
    static async Task Login()
    {
        client = await AuthManager.Login(APP_DIR, settings);
        var user = await client!.Me.GetAsync();
        Console.WriteLine($"Login successful! User: {user!.DisplayName}");
    }

    /// <summary>Logout from Microsoft Graph</summary>
    static async Task Logout()
    {
        await AuthManager.Logout(APP_DIR, settings);
        Console.WriteLine("Logged out.");
    }

    /// <summary>Show the current user</summary>
    static async Task ShowUser()
    {
        await EnsureAuthentication();
        var user = await client!.Me.GetAsync();
        Console.WriteLine($"\n👤 User: {user!.DisplayName} ({user.UserPrincipalName})\n");
    }

    /// <summary>Show all the todo lists</summary>
    static async Task ShowList()
    {
        // Ensure we're logged in
        await EnsureAuthentication();

        // Get all the todo lists
        var lists = await client!.Me.Todo.Lists.GetAsync();

        Console.WriteLine("\n📋 Your Lists:\n");
        foreach (var list in lists!.Value!)
        {
            Console.WriteLine($" - {list.Id}\t{list.DisplayName}");
        }
    }

    /// <summary>Show the help message</summary>
    static void ShowHelp()
    {
        Console.WriteLine($"Usage: {NAME} <command>\n");
        Console.WriteLine("A command-line-interface to interact with Microsoft To Do\n");
        Console.WriteLine("Commands:");
        Console.WriteLine("  login       Login with Microsoft Graph Services");
        Console.WriteLine("  logout      Logout from Microsoft Graph Services");
        Console.WriteLine("  user        Show current user information");
        Console.WriteLine("  lists       Show your todo lists");
        Console.WriteLine("  show        Show tasks in a todo list");
        Console.WriteLine("  add         Add a new task");
        Console.WriteLine("  complete    Complete a task");
        Console.WriteLine("  strike      Complete a task");
        Console.WriteLine("  done        Complete a task");
        Console.WriteLine("  delete      Delete a task");
        Console.WriteLine("  help        Show this help message");
    }
}
