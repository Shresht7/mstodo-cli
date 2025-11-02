using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace mstodo_cli.Commands
{
    public class CompleteCommand : ICommand
    {
        public async Task ExecuteAsync(CommandContext context)
        {
            var (todoList, targetTask, errorMessage) = await CommandHelpers.GetListAndTask(context);
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
                var completedTask = await context.Client!.Me.Todo.Lists[todoList!.Id].Tasks[targetTask!.Id].PatchAsync(updatedTask);
                if (context.Formatter is JsonFormatter)
                {
                    Console.WriteLine(context.Formatter.Format(completedTask));
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
    }
}