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

    /// <summary>Microsoft Graph Client</summary>
    static GraphServiceClient? client;

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
        try
        {
            // Get the output formatter
            IOutputFormatter formatter = OutputManager.GetFormatter(args.ToList());

            // Switch on the subcommand and dispatch the corresponding action
            string command = args.Length > 0 ? args[0].ToLower() : string.Empty;

            switch (command)
            {
                case "login":
                    await Login(formatter); break;
                case "logout":
                    await Logout(); break;
                case "user":
                    await ShowUser(formatter); break;
                case "list":
                case "lists":
                    await ShowAllLists(args.Skip(1).ToList(), formatter); break;
                case "show":
                case "view":
                    await ShowTasksInList(args.Skip(1).ToList(), formatter); break;
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
        if (client != null && todoListsMap.Count == 0) // Populate map if client is authenticated and map is empty
        {
            await PopulateAllLists();
        }
    }

    // COMMANDS
    // --------

    /// <summary>Authenticate with Microsoft Graph to login</summary>
    static async Task Login(IOutputFormatter formatter)
    {
        client = await AuthManager.Login(APP_DIR, settings);
        var user = await client!.Me.GetAsync();
        Console.WriteLine(formatter.Format(user));
        await PopulateAllLists(); // Populate the map after successful login
    }

    /// <summary>Logout from Microsoft Graph</summary>
    static async Task Logout()
    {
        await AuthManager.Logout(APP_DIR, settings);
        todoListsMap.Clear(); // Clear the map on logout
        Console.WriteLine("Logged out.");
    }

    /// <summary>Show the current user</summary>
    static async Task ShowUser(IOutputFormatter formatter)
    {
        await EnsureAuthentication();
        var user = await client!.Me.GetAsync();
        Console.WriteLine(formatter.Format(user));
    }

    /// <summary>Get all the todo lists and populate the map</summary>
    static async Task PopulateAllLists()
    {
        var lists = await client!.Me.Todo.Lists.GetAsync();
        todoListsMap.Clear();
        foreach (var list in lists!.Value!)
        {
            todoListsMap[list.DisplayName!] = list;
        }
    }

    /// <summary>Show all the todo lists</summary>
    static async Task ShowAllLists(List<string> args, IOutputFormatter formatter)
    {
        // Ensure we are authenticated
        await EnsureAuthentication();

        Console.WriteLine(formatter.Format(todoListsMap.Values));
    }

    /// <summary>Show tasks in a specific todo list</summary>
    static async Task ShowTasksInList(List<string> args, IOutputFormatter formatter)
    {
        // Ensure that we are authenticated
        await EnsureAuthentication();

        // Ensure that we have a list identifier0
        if (args.Count == 0)
        {
            Console.WriteLine("Error: Please provide a list identifier (index or name).");
            return;
        }

        string listIdentifier = args[0];
        TodoTaskList? todoList = Helpers.GetListFromIdentifier(listIdentifier, todoListsMap);

        // Ensure that the list exists
        if (todoList == null)
        {
            Console.WriteLine($"Error: Todo list '{listIdentifier}' not found.");
            return;
        }

        // Parse the limit argument
        int limit = -1; // -1 means no limit
        if (args.Contains("--limit"))
        {
            int limitIndex = args.IndexOf("--limit");
            if (limitIndex + 1 < args.Count && int.TryParse(args[limitIndex + 1], out int parsedLimit))
            {
                limit = parsedLimit;
            }
            else
            {
                Console.WriteLine("Error: --limit requires a numeric value.");
                return;
            }
        }

        // Fetch the tasks
        var tasks = await client!.Me.Todo.Lists[todoList.Id].Tasks.GetAsync(requestConfiguration =>
        {
            if (limit > 0)
            {
                requestConfiguration.QueryParameters.Top = limit;
            }
        });

        // Display the tasks
        Console.WriteLine(formatter.Format(tasks!.Value));
    }

    /// <summary>Show the help message</summary>
    static void ShowHelp()
    {
        Console.WriteLine($"Usage: {NAME} <command> [arguments] [--json]\n");
        Console.WriteLine("A command-line-interface to interact with Microsoft To Do\n");
        Console.WriteLine("Commands:");
        Console.WriteLine("  login       Login with Microsoft Graph Services");
        Console.WriteLine("  logout      Logout from Microsoft Graph Services");
        Console.WriteLine("  user        Show current user information");
        Console.WriteLine("  lists       Show your todo lists");
        Console.WriteLine("  show <list> [--limit <number>] Show tasks in a todo list");
        Console.WriteLine("  add         Add a new task");
        Console.WriteLine("  complete    Complete a task");
        Console.WriteLine("  strike      Complete a task");
        Console.WriteLine("  done        Complete a task");
        Console.WriteLine("  delete      Delete a task");
        Console.WriteLine("  help        Show this help message");
        Console.WriteLine("\nGlobal Options:");
        Console.WriteLine("  --json      Output in JSON format");
    }
}
