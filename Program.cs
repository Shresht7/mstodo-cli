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
            var rest = args.Skip(1).ToList();


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
                    await ShowAllLists(rest, formatter); break;
                case "show":
                case "view":
                    await ShowTasksInList(rest, formatter); break;
                case "add":
                case "create":
                    await AddTask(rest, formatter); break;
                case "complete":
                case "strike":
                case "done":
                    await CompleteTask(rest, formatter); break;
                case "delete":
                    await DeleteTask(rest, formatter); break;
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

    /// <summary>Add a new task to a specific todo list</summary>
    static async Task AddTask(List<string> args, IOutputFormatter formatter)
    {
        // Ensure that we are authenticated
        await EnsureAuthentication();

        // Ensure that we have a list identifier and a task title
        if (args.Count < 2)
        {
            // TODO: Make the list an option (--list or --to) and default to using the generic "Tasks" list
            Console.WriteLine("Error: Please provide a list identifier and a task title.");
            Console.WriteLine("Usage: add <list_identifier> <task_title>");
            return;
        }

        // Extract the list identifier and task title
        string listIdentifier = args[0];
        string taskTitle = string.Join(" ", args.Skip(1));

        // Find the todo list by identifier
        TodoTaskList? todoList = Helpers.GetListFromIdentifier(listIdentifier, todoListsMap);
        if (todoList == null)
        {
            Console.WriteLine($"Error: Todo list '{listIdentifier}' not found.");
            return;
        }

        // Create the new task
        var newTask = new TodoTask
        {
            Title = taskTitle,
            // TODO: Add other properties as needed
        };

        // Add the task to the list
        try
        {
            var addedTask = await client!.Me.Todo.Lists[todoList.Id].Tasks.PostAsync(newTask);
            if (formatter is JsonFormatter)
            {
                Console.WriteLine(formatter.Format(addedTask));
            }
            else
            {
                Console.WriteLine($"Successfully added task '{addedTask!.Title}' to list '{todoList.DisplayName}'.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding task: {ex.Message}");
        }
    }

    /// <summary>Complete a task in a specific todo list</summary>
    static async Task CompleteTask(List<string> args, IOutputFormatter formatter)
    {
        // Ensure that we are authenticated
        await EnsureAuthentication();

        // Ensure that we have a list identifier and a task identifier
        if (args.Count < 2)
        {
            Console.WriteLine("Error: Please provide a list identifier and a task identifier.");
            Console.WriteLine("Usage: complete <list_identifier> <task_identifier>");
            return;
        }

        // Extract the list identifier and task identifier
        string listIdentifier = args[0];
        string taskIdentifier = string.Join(" ", args.Skip(1));

        // Find the todo list by identifier
        TodoTaskList? todoList = Helpers.GetListFromIdentifier(listIdentifier, todoListsMap);
        if (todoList == null)
        {
            Console.WriteLine($"Error: Todo list '{listIdentifier}' not found.");
            return;
        }

        // Fetch tasks for the list to find the target task
        var tasks = await client!.Me.Todo.Lists[todoList.Id].Tasks.GetAsync();
        TodoTask? targetTask = null;

        // Try to find by index
        if (int.TryParse(taskIdentifier, out int index))
        {
            if (index >= 0 && index < tasks!.Value!.Count)
            {
                targetTask = tasks.Value.ElementAt(index);
            }
        }

        // Try to find by title (case-insensitive)
        if (targetTask == null)
        {
            targetTask = tasks!.Value!.FirstOrDefault(t => t.Title!.Equals(taskIdentifier, StringComparison.OrdinalIgnoreCase));
        }

        if (targetTask == null)
        {
            Console.WriteLine($"Error: Task '{taskIdentifier}' not found in list '{todoList.DisplayName}'.");
            return;
        }

        // Update the task status to completed
        var updatedTask = new TodoTask
        {
            Status = Microsoft.Graph.Models.TaskStatus.Completed
        };

        // Update the task
        try
        {
            var completedTask = await client!.Me.Todo.Lists[todoList.Id].Tasks[targetTask.Id].PatchAsync(updatedTask);
            if (formatter is JsonFormatter)
            {
                Console.WriteLine(formatter.Format(completedTask));
            }
            else
            {
                Console.WriteLine($"Successfully completed task '{completedTask!.Title}' in list '{todoList.DisplayName}'.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error completing task: {ex.Message}");
        }
    }

    static async Task DeleteTask(List<string> args, IOutputFormatter formatter)
    {
        // Ensure that we are authenticated
        await EnsureAuthentication();

        // Ensure that we have a list identifier and a task identifier
        if (args.Count < 2)
        {
            Console.WriteLine("Error: Please provide a list identifier and a task identifier.");
            Console.WriteLine("Usage: delete <list_identifier> <task_identifier>");
            return;
        }

        // Extract the list identifier and task identifier
        string listIdentifier = args[0];
        string taskIdentifier = string.Join(" ", args.Skip(1));

        // Find the todo list by identifier
        TodoTaskList? todoList = Helpers.GetListFromIdentifier(listIdentifier, todoListsMap);
        if (todoList == null)
        {
            Console.WriteLine($"Error: Todo list '{listIdentifier}' not found.");
            return;
        }

        // Fetch tasks for the list to find the target task
        var tasks = await client!.Me.Todo.Lists[todoList.Id].Tasks.GetAsync();
        TodoTask? targetTask = null;

        // Try to find by index
        if (int.TryParse(taskIdentifier, out int index))
        {
            if (index >= 0 && index < tasks!.Value!.Count)
            {
                targetTask = tasks.Value.ElementAt(index);
            }
        }

        // Try to find by title (case-insensitive)
        if (targetTask == null)
        {
            targetTask = tasks!.Value!.FirstOrDefault(t => t.Title!.Equals(taskIdentifier, StringComparison.OrdinalIgnoreCase));
        }

        if (targetTask == null)
        {
            Console.WriteLine($"Error: Task '{taskIdentifier}' not found in list '{todoList.DisplayName}'.");
            return;
        }

        // Delete the task
        try
        {
            await client!.Me.Todo.Lists[todoList.Id].Tasks[targetTask.Id].DeleteAsync();
            if (formatter is JsonFormatter)
            {
                Console.WriteLine(formatter.Format(targetTask));
            }
            else
            {
                Console.WriteLine($"Successfully deleted task '{targetTask!.Title}' from list '{todoList.DisplayName}'.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting task: {ex.Message}");
        }
    }

    /// <summary>Show the help message</summary>
    static void ShowHelp()
    {
        Console.WriteLine($"Usage: {NAME} <command> [arguments] [--json]");
        Console.WriteLine("");

        Console.WriteLine("A command-line-interface to interact with Microsoft To Do");
        Console.WriteLine("");

        Console.WriteLine("Global Options:");
        Console.WriteLine("  --json      Output in JSON format");
        Console.WriteLine("");

        Console.WriteLine("Commands:");
        Console.WriteLine("  login       Login with Microsoft Graph Services");
        Console.WriteLine("  logout      Logout from Microsoft Graph Services");
        Console.WriteLine("  user        Show current user information");
        Console.WriteLine("");

        Console.WriteLine("  lists       Show your todo lists");
        Console.WriteLine("  show <list> [--limit <number>] Show tasks in a todo list");
        Console.WriteLine("  add <list> <title> Add a new task to a specific list");
        Console.WriteLine("  complete <list> <task> Complete a task in a specific list");
        Console.WriteLine("  delete <list> <task>   Delete a task in a specific list");
        Console.WriteLine("");

        Console.WriteLine("  help        Show this help message");
    }
}
