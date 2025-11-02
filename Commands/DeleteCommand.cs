using Microsoft.Graph;
using Microsoft.Graph.Models;
using mstodo_cli.OutputFormatter;

namespace mstodo_cli.Commands
{
    public class DeleteCommand : ICommand
    {
        public async Task ExecuteAsync(CommandContext context)
        {
            var (todoList, targetTask, errorMessage) = await CommandHelpers.GetListAndTask(context);
            if (errorMessage != null)
            {
                Console.WriteLine(errorMessage);
                Console.WriteLine("Usage: delete <list_identifier> <task_identifier>");
                return;
            }

            // Delete the task
            try
            {
                await context.Client!.Me.Todo.Lists[todoList!.Id].Tasks[targetTask!.Id].DeleteAsync();
                if (context.Formatter is JsonFormatter)
                {
                    Console.WriteLine(context.Formatter.Format(targetTask));
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
    }
}