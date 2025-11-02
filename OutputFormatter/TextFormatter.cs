using Microsoft.Graph.Models;
using System.Text;

public class TextFormatter : IOutputFormatter
{
    public string Format<T>(T data)
    {
        if (data is IEnumerable<TodoTaskList> todoLists)
        {
            return FormatTodoTaskLists(todoLists);
        }
        else if (data is IEnumerable<TodoTask> todoTasks)
        {
            return FormatTodoTasks(todoTasks);
        }
        else if (data is User user)
        {
            return FormatUser(user);
        }
        // Default or fallback formatting
        return data?.ToString() ?? string.Empty;
    }

    private string FormatTodoTaskLists(IEnumerable<TodoTaskList> todoLists)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var list in todoLists)
        {
            sb.AppendLine($"{list.DisplayName}");
        }
        return sb.ToString();
    }

    private string FormatTodoTasks(IEnumerable<TodoTask> todoTasks)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var task in todoTasks)
        {
            string status = task.Status == Microsoft.Graph.Models.TaskStatus.Completed ? "[x]" : "[ ]";
            string importance = task.Importance == Importance.High ? "🌟" : " ";
            sb.AppendLine($"- {status} {task.Title} {importance}");
        }
        return sb.ToString();
    }

    private string FormatUser(User user)
    {
        return $"\n👤 User: {user.DisplayName} ({user.UserPrincipalName})\n";
    }
}
