using Microsoft.Graph;
using Microsoft.Graph.Models;

public interface ICommand
{
    Task ExecuteAsync(CommandContext context);
}
