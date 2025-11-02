// Library
using Microsoft.Graph;
using Microsoft.Graph.Models;
using mstodo_cli.Commands;

// -------
// PROGRAM
// -------

class Program
{
    /// <summary>Name of the application</summary>
    static readonly string NAME = "mstodo-cli";

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

            // Create the command context
            CommandContext context = new CommandContext(settings, formatter, args.Skip(1).ToList(), APP_DIR, todoListsMap);

            // Switch on the subcommand and dispatch the corresponding action
            string command = args.Length > 0 ? args[0].ToLower() : string.Empty;

            switch (command)
            {
                case "login":
                    await Login(formatter); break;
                case "logout":
                    await Logout(); break;
                case "user":
                    await new UserCommand().ExecuteAsync(context); break;

                case "lists":
                    await new ListsCommand().ExecuteAsync(context); break;
                case "list":
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

    // COMMANDS
    // --------

    /// <summary>Authenticate with Microsoft Graph to login</summary>
    static async Task Login(IOutputFormatter formatter)
    {
        // Create a temporary context for login, as the main context might not be fully initialized yet
        // and login needs to happen before other commands.
        CommandContext tempContext = new CommandContext(settings, formatter, new List<string>(), APP_DIR, todoListsMap);

        tempContext.Client = await AuthManager.Login(tempContext.AppDir, tempContext.Settings);
        var user = await tempContext.Client!.Me.GetAsync();
        Console.WriteLine(tempContext.Formatter.Format(user));
        await tempContext.PopulateAllLists(); // Populate the map after successful login
    }

    /// <summary>Logout from Microsoft Graph</summary>
    static async Task Logout()
    {
        await AuthManager.Logout(APP_DIR, settings);
        todoListsMap.Clear(); // Clear the map on logout
        Console.WriteLine("Logged out.");
    }

    /// <summary>Show tasks in a specific todo list</summary>
    static async Task ShowTasksInList(List<string> args, IOutputFormatter formatter)
    {
        var (todoList, _, errorMessage) = await CommandHelpers.GetListAndTask(context, requireTask: false);
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

    /// <summary>Show the help message</summary>
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
        Console.WriteLine("  add <list> <title>                 Add a new task to a specific list");
        Console.WriteLine("  complete <list> <task>             Complete a task in a specific list");
        Console.WriteLine("  delete <list> <task>               Delete a task in a specific list");
        Console.WriteLine("");

        Console.WriteLine("  help        Show this help message");
    }
}
