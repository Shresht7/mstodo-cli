using Microsoft.Graph;
using Microsoft.Graph.Models;
using mstodo_cli.OutputFormatter;

namespace mstodo_cli.Commands
{
    public class ListsCommand : ICommand
    {
        public async Task ExecuteAsync(CommandContext context)
        {
            await context.EnsureAuthentication();
            Console.WriteLine(context.Formatter.Format(context.TodoListsMap.Values));
        }
    }
}