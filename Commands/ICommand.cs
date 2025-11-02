using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace mstodo_cli.Commands
{
    public interface ICommand
    {
        Task ExecuteAsync(CommandContext context);
    }
}