using Microsoft.Graph;
using Microsoft.Graph.Models;

public class ListsCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext context)
    {
        await context.EnsureAuthentication();
        Console.WriteLine(context.Formatter.Format(context.TodoListsMap.Values));
    }
}
