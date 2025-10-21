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
        var (todoList, _, errorMessage) = await GetListAndTask(args, requireTask: false);
        if (errorMessage != null)
        {
            Console.WriteLine(errorMessage);
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
        var tasks = await client!.Me.Todo.Lists[todoList!.Id].Tasks.GetAsync(requestConfiguration =>
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
        var (todoList, _, errorMessage) = await GetListAndTask(args, requireTask: false);
        if (errorMessage != null)
        {
            Console.WriteLine(errorMessage);
            Console.WriteLine("Usage: add <list_identifier> <task_title>");
            return;
        }

        // Extract the task title (it's the second argument onwards)
        string taskTitle = string.Join(" ", args.Skip(1));

        // Create the new task
        var newTask = new TodoTask
        {
            Title = taskTitle,
            // TODO: Add other properties as needed
        };

        // Add the task to the list
        try
        {
            var addedTask = await client!.Me.Todo.Lists[todoList!.Id].Tasks.PostAsync(newTask);
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
        var (todoList, targetTask, errorMessage) = await GetListAndTask(args);
        if (errorMessage != null)
        {
            Console.WriteLine(errorMessage);
            Console.WriteLine("Usage: complete <list_identifier> <task_identifier>");
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
            var completedTask = await client!.Me.Todo.Lists[todoList!.Id].Tasks[targetTask!.Id].PatchAsync(updatedTask);
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
        var (todoList, targetTask, errorMessage) = await GetListAndTask(args);
        if (errorMessage != null)
        {
            Console.WriteLine(errorMessage);
            Console.WriteLine("Usage: delete <list_identifier> <task_identifier>");
            return;
        }

        // Delete the task
        try
        {
            await client!.Me.Todo.Lists[todoList!.Id].Tasks[targetTask!.Id].DeleteAsync();
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

    /// <summary>Retrieves a TodoTaskList and optionally a TodoTask based on provided arguments.</summary>
    /// <param name="args">The list of command-line arguments.</param>
    /// <param name="requireTask">If true, a task identifier is expected and a TodoTask will be searched for.</param>
    /// <param name="listIdentifierIndex">The index in 'args' where the list identifier is expected.</param>
    /// <param name="taskIdentifierIndex">The index in 'args' where the task identifier is expected (if 'requireTask' is true).</param>
    /// <returns>A tuple containing the TodoTaskList, TodoTask (null if not found or not required), and an error message (null if successful).</returns>
    private static async Task<(TodoTaskList? todoList, TodoTask? targetTask, string? errorMessage)> GetListAndTask(
        List<string> args, bool requireTask = true, int listIdentifierIndex = 0, int taskIdentifierIndex = 1)
    {
        // Ensure that we are authenticated
        await EnsureAuthentication();

        // Validate argument count for list
        if (args.Count <= listIdentifierIndex)
        {
            return (null, null, "Error: Please provide a list identifier (index or name).");
        }

        string listIdentifier = args[listIdentifierIndex];
        TodoTaskList? todoList = Helpers.GetListFromIdentifier(listIdentifier, todoListsMap);
        if (todoList == null)
        {
            return (null, null, $"Error: Todo list '{listIdentifier}' not found.");
        }

        TodoTask? targetTask = null;
        if (requireTask)
        {
            // Validate argument count for task
            if (args.Count <= taskIdentifierIndex)
            {
                return (null, null, "Error: Please provide a task identifier (index or name).");
            }

            string taskIdentifier = string.Join(" ", args.Skip(taskIdentifierIndex));

            // Fetch tasks for the list to find the target task
            var tasks = await client!.Me.Todo.Lists[todoList.Id].Tasks.GetAsync();

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
                return (todoList, null, $"Error: Task '{taskIdentifier}' not found in list '{todoList.DisplayName}'.");
            }
        }

        return (todoList, targetTask, null);
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
