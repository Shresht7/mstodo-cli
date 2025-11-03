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

        // Parse the filter argument
        string filter = string.Empty;
        if (context.Args.Contains("--filter"))
        {
            int filterIndex = context.Args.IndexOf("--filter");
            if (filterIndex + 1 < context.Args.Count)
            {
                filter = context.Args[filterIndex + 1];
            }
            else
            {
                Console.WriteLine("Error: --filter requires a string value.");
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
            if (!string.IsNullOrEmpty(filter))
            {
                requestConfiguration.QueryParameters.Filter = filter;
            }
        });

        // Display the tasks
        Console.WriteLine(context.Formatter.Format(tasks!.Value));
    }
}
