class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Show help message if no arguments were provided, and return early
            if (args.Length == 0)
            {
                Console.WriteLine("ShowHelp");
                return;
            }

            // Switch on the subcommand and dispatch the corresponding action
            string command = args[0];
            switch (command.ToLower())
            {
                case "list":
                case "lists":
                    Console.WriteLine("ShowLists");
                    break;
                case "show":
                case "view":
                    Console.WriteLine("ShowTask");
                    break;
                case "add":
                case "create":
                    Console.WriteLine("CreateTask");
                    break;
                case "complete":
                case "strike":
                case "done":
                    Console.WriteLine("CompleteTask");
                    break;
                case "delete":
                    Console.WriteLine("DeleteTask");
                    break;
                default:
                    Console.WriteLine("ShowHelp");
                    break;

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
