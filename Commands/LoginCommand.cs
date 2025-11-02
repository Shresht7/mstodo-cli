using Microsoft.Graph;
using Microsoft.Graph.Models;
using mstodo_cli.OutputFormatter;
using Microsoft.Identity.Client;

namespace mstodo_cli.Commands
{
    public class LoginCommand : ICommand
    {
        public async Task ExecuteAsync(CommandContext context)
        {
            try
            {
                context.Client = await AuthManager.Login(context.AppDir, context.Settings);
                var user = await context.Client!.Me.GetAsync();
                Console.WriteLine(context.Formatter.Format(user));
                await context.PopulateAllLists(); // Populate the map after successful login
            }
            catch (MsalException ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
                Console.WriteLine("Please ensure your appsettings.json is correctly configured and you have granted the necessary permissions.");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex, context.AppDir);
            }
        }
    }
}