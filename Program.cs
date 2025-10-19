using Azure.Identity;
using Microsoft.Graph;

class Program
{
    /// <summary>Microsoft Graph Client</summary>
    static GraphServiceClient? client;

    /// <summary>Application Client ID</summary>
    const string CLIENT_ID = "2157a77b-da98-48e8-8240-2d26d1dbe0b4";

    // ----
    // MAIN
    // ----

    static async Task Main(string[] args)
    {
        try
        {
            // Show help message if no arguments were provided, and return early
            if (args.Length == 0)
            {
                Console.WriteLine("ShowHelp");
                return;
            }

            // Initialize Microsoft Graph Client and login as user
            await InitMicrosoftGraph();

            // Switch on the subcommand and dispatch the corresponding action
            string command = args[0];
            switch (command.ToLower())
            {
                case "list":
                case "lists":
                    await ShowList(); break;
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
            Console.WriteLine($"\n\nError:");
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>Initialize the Microsoft Graph Client and login as the user</summary>
    static async Task InitMicrosoftGraph()
    {
        // Define the interactive browser credentials
        var credentials = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
        {
            ClientId = CLIENT_ID,
            TenantId = "common",
            RedirectUri = new Uri("http://localhost"),
        });

        // Initialize the client
        client = new GraphServiceClient(credentials, ["Tasks.ReadWrite", "User.Read"]);

        // Get the user's profile
        var me = await client.Me.GetAsync();
        Console.WriteLine($"☑️ Logged in as {me?.DisplayName}");
    }

    static async Task ShowList()
    {
        // Get all the todo lists
        var lists = await client!.Me.Todo.Lists.GetAsync();

        Console.WriteLine("\n📋 Your Lists:\n");
        foreach (var list in lists!.Value!)
        {
            Console.WriteLine($" - {list.Id}\t{list.DisplayName}");
        }
    }
}
