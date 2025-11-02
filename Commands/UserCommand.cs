using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace mstodo_cli.Commands
{
    public class UserCommand : ICommand
    {
        /// <summary>Show the current user</summary>
        public async Task ExecuteAsync(CommandContext context)
        {
            await context.EnsureAuthentication();
            var user = await context.Client!.Me.GetAsync();
            Console.WriteLine(context.Formatter.Format(user));
        }
    }
}
