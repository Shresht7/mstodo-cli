using Microsoft.Graph;
using Microsoft.Graph.Models;

public class AddCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext context)
    {
        var (todoList, _, errorMessage) = await CommandHelpers.GetListAndTask(context, requireTask: false);
        if (errorMessage != null)
        {
            Console.WriteLine(errorMessage);
            Console.WriteLine("Usage: add <list_identifier> <task_title>");
            return;
        }

        // Extract the task title (it's the second argument onwards)
        string taskTitle = string.Join(" ", context.Args.Skip(1));

        // Create the new task
        var newTask = new TodoTask
        {
            Title = taskTitle,
            // TODO: Add other properties as needed
        };

        // Add the task to the list
        try
        {
            var addedTask = await context.Client!.Me.Todo.Lists[todoList!.Id].Tasks.PostAsync(newTask);
            if (context.Formatter is JsonFormatter)
            {
                Console.WriteLine(context.Formatter.Format(addedTask));
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
}
