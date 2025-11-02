using Microsoft.Graph;
using Microsoft.Graph.Models;
using mstodo_cli.OutputFormatter;

namespace mstodo_cli.Commands
{
    public class LoginCommand : ICommand
    {
        public async Task ExecuteAsync(CommandContext context)
        {
            context.Client = await AuthManager.Login(context.AppDir, context.Settings);
            var user = await context.Client!.Me.GetAsync();
            Console.WriteLine(context.Formatter.Format(user));
            await context.PopulateAllLists(); // Populate the map after successful login
        }
    }
}