using Microsoft.Graph;
using Microsoft.Graph.Models;

public class LogoutCommand : ICommand
{
    public async Task ExecuteAsync(CommandContext context)
    {
        await AuthManager.Logout(context.AppDir, context.Settings);
        context.TodoListsMap.Clear(); // Clear the map on logout
        Console.WriteLine("Logged out.");
    }
}
