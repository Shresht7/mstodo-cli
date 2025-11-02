using System.Text.Json;

public interface IOutputFormatter
{
    string Format<T>(T data);
}

public class PlainTextFormatter : IOutputFormatter
{
    public string Format<T>(T data)
    {
        // This formatter will be handled case by case in the commands
        // It will not be used directly to format generic data
        return data?.ToString() ?? string.Empty;
    }
}

public class JsonFormatter : IOutputFormatter
{
    public string Format<T>(T data)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(data, options);
    }
}
