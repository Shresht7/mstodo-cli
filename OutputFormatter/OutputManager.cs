public static class OutputManager
{
    public static IOutputFormatter GetFormatter(List<string> args)
    {
        if (args.Contains("--json"))
        {
            return new JsonFormatter();
        }
        return new TextFormatter();
    }
}
