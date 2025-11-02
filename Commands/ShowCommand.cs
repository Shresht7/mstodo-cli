using Microsoft.Graph;
using Microsoft.Graph.Models;

public class ShowCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext context)
    {
        var (todoList, _, errorMessage) = await CommandHelpers.GetListAndTask(context, requireTask: false);
        if (errorMessage != null)
        {
            Console.WriteLine(errorMessage);
            return;
        }

        // Parse the limit argument
        int limit = -1; // -1 means no limit
        if (context.Args.Contains("--limit"))
        {
            int limitIndex = context.Args.IndexOf("--limit");
            if (limitIndex + 1 < context.Args.Count && int.TryParse(context.Args[limitIndex + 1], out int parsedLimit))
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
        var tasks = await context.Client!.Me.Todo.Lists[todoList!.Id].Tasks.GetAsync(requestConfiguration =>
        {
            if (limit > 0)
            {
                requestConfiguration.QueryParameters.Top = limit;
            }
        });

        // Display the tasks
        Console.WriteLine(context.Formatter.Format(tasks!.Value));
    }
}
