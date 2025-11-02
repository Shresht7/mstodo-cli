using Microsoft.Graph;
using Microsoft.Graph.Models;

public static class CommandHelpers
{
    /// <summary>Retrieves a TodoTaskList and optionally a TodoTask based on provided arguments.</summary>
    /// <param name="context">The command context.</param>
    /// <param name="requireTask">If true, a task identifier is expected and a TodoTask will be searched for.</param>
    /// <param name="listIdentifierIndex">The index in 'args' where the list identifier is expected.</param>
    /// <param name="taskIdentifierIndex">The index in 'args' where the task identifier is expected (if 'requireTask' is true).</param>
    /// <returns>A tuple containing the TodoTaskList, TodoTask (null if not found or not required), and an error message (null if successful).</returns>
    public static async Task<(TodoTaskList? todoList, TodoTask? targetTask, string? errorMessage)> GetListAndTask(
        CommandContext context, bool requireTask = true, int listIdentifierIndex = 0, int taskIdentifierIndex = 1)
    {
        // Ensure that we are authenticated
        await context.EnsureAuthentication();

        // Validate argument count for list
        if (context.Args.Count <= listIdentifierIndex)
        {
            return (null, null, "Error: Please provide a list identifier (index or name).");
        }

        string listIdentifier = context.Args[listIdentifierIndex];
        TodoTaskList? todoList = Helpers.GetListFromIdentifier(listIdentifier, context.TodoListsMap);
        if (todoList == null)
        {
            return (null, null, $"Error: Todo list '{listIdentifier}' not found.");
        }

        TodoTask? targetTask = null;
        if (requireTask)
        {
            // Validate argument count for task
            if (context.Args.Count <= taskIdentifierIndex)
            {
                return (null, null, "Error: Please provide a task identifier (index or name).");
            }

            string taskIdentifier = string.Join(" ", context.Args.Skip(taskIdentifierIndex));

            // Fetch tasks for the list to find the target task
            var tasks = await context.Client!.Me.Todo.Lists[todoList.Id].Tasks.GetAsync();

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
}
