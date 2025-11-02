using Microsoft.Graph.Models;

public static class Helpers
{
    /// <summary>Get a TodoTaskList by its identifier (index or display name)</summary>
    public static TodoTaskList? GetListFromIdentifier(string identifier, Dictionary<string, TodoTaskList> todoListsMap)
    {
        // Try to parse as an index
        if (int.TryParse(identifier, out int index))
        {
            if (index >= 0 && index < todoListsMap.Count)
            {
                return todoListsMap.Values.ElementAt(index);
            }
        }

        // Try to find by display name (case-insensitive, or ends with to handle emojis)
        foreach (var list in todoListsMap.Values)
        {
            if (list.DisplayName!.Equals(identifier, StringComparison.OrdinalIgnoreCase) ||
                list.DisplayName.EndsWith(identifier, StringComparison.OrdinalIgnoreCase))
            {
                return list;
            }
        }

        return null;
    }
}
