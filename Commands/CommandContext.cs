using Microsoft.Graph;
using Microsoft.Graph.Models;
using mstodo_cli.OutputFormatter;

namespace mstodo_cli.Commands
{
    public class CommandContext
    {
        /// <summary>Microsoft Graph Client</summary>
        public GraphServiceClient? Client { get; set; }
        /// <summary>Application settings</summary>
        public Settings Settings { get; }
        /// <summary>Path to the application data folder</summary>
        public string AppDir { get; }
        /// <summary>Cache for todo lists: DisplayName -> TodoTaskList</summary>
        public Dictionary<string, TodoTaskList> TodoListsMap { get; }
        public List<string> Args { get; }
        public IOutputFormatter Formatter { get; }

        public CommandContext(Settings settings, IOutputFormatter formatter, List<string> args, string appDir, Dictionary<string, TodoTaskList> todoListsMap)
        {
            Settings = settings;
            Formatter = formatter;
            Args = args;
            AppDir = appDir;
            TodoListsMap = todoListsMap;
        }

        /// <summary>Ensure that we are authenticated, and prompt to login if we are not</summary>
        public async Task EnsureAuthentication()
        {
            if (Client == null)
            {
                Client = await AuthManager.Login(AppDir, Settings);
                if (Client == null)
                {
                    throw new InvalidOperationException("Authentication failed: GraphServiceClient could not be initialized.");
                }
            }
            if (TodoListsMap.Count == 0) // Populate map if client is authenticated and map is empty
            {
                await PopulateAllLists();
            }
        }

        /// <summary>Get all the todo lists and populate the map</summary>
        public async Task PopulateAllLists()
        {
            var lists = await Client!.Me.Todo.Lists.GetAsync();
            TodoListsMap.Clear();
            foreach (var list in lists!.Value!)
            {
                TodoListsMap[list.DisplayName!] = list;
            }
        }
    }
}
