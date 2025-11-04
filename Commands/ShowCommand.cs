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

        // Parse the skip argument
        int skip = -1; // -1 means no skip
        if (context.Args.Contains("--skip"))
        {
            int skipIndex = context.Args.IndexOf("--skip");
            if (skipIndex + 1 < context.Args.Count && int.TryParse(context.Args[skipIndex + 1], out int parsedSkip))
            {
                skip = parsedSkip;
            }
            else
            {
                Console.WriteLine("Error: --skip requires a numeric value.");
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

        // Parse the search argument
        string search = string.Empty;
        if (context.Args.Contains("--search"))
        {
            int searchIndex = context.Args.IndexOf("--search");
            if (searchIndex + 1 < context.Args.Count)
            {
                search = context.Args[searchIndex + 1];
            }
            else
            {
                Console.WriteLine("Error: --search requires a string value.");
                return;
            }
        }

        // Combine filter and search
        if (!string.IsNullOrEmpty(search))
        {
            string searchFilter = $"contains(title,'{search}') or contains(body/content,'{search}')";
            if (!string.IsNullOrEmpty(filter))
            {
                filter = $"{filter} and ({searchFilter})";
            }
            else
            {
                filter = searchFilter;
            }
        }

        // Parse the orderby argument
        string orderby = string.Empty;
        if (context.Args.Contains("--orderby"))
        {
            int orderbyIndex = context.Args.IndexOf("--orderby");
            if (orderbyIndex + 1 < context.Args.Count)
            {
                orderby = context.Args[orderbyIndex + 1];
            }
            else
            {
                Console.WriteLine("Error: --orderby requires a string value.");
                return;
            }
        }

        // Parse the important argument
        bool important = context.Args.Contains("--important");

        // Combine filter, search and important
        if (important)
        {
            string importantFilter = "importance eq 'high'";
            if (!string.IsNullOrEmpty(filter))
            {
                filter = $"{filter} and {importantFilter}";
            }
            else
            {
                filter = importantFilter;
            }
        }

        // Fetch the tasks
        var tasks = await context.Client!.Me.Todo.Lists[todoList!.Id].Tasks.GetAsync(requestConfiguration =>
        {
            if (limit > 0)
            {
                requestConfiguration.QueryParameters.Top = limit;
            }
            if (skip > 0)
            {
                requestConfiguration.QueryParameters.Skip = skip;
            }
            if (!string.IsNullOrEmpty(filter))
            {
                requestConfiguration.QueryParameters.Filter = filter;
            }
            if (!string.IsNullOrEmpty(orderby))
            {
                requestConfiguration.QueryParameters.Orderby = new[] { orderby };
            }
        });

        // Display the tasks
        Console.WriteLine(context.Formatter.Format(tasks!.Value));
    }
}
