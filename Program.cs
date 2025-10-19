// Library
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;

class Program
{
    /// <summary>Name of the application</summary>
    static readonly string NAME = "mstodo-cli";

    /// <summary>Microsoft Graph Client</summary>
    static GraphServiceClient? client;

    /// <summary>Application Client ID</summary>
    const string CLIENT_ID = "2157a77b-da98-48e8-8240-2d26d1dbe0b4";

    const string AUTHORITY = "https://login.microsoftonline.com/common";

    /// <summary>Permission scopes requested by the application</summary>
    static readonly string[] SCOPES = ["User.Read", "Tasks.ReadWrite"];

    /// <summary>Path to the application data folder</summary>
    static readonly string APP_DIR = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        NAME
    );

    /// <summary>Path to the token cache file</summary>
    static readonly string TOKEN_CACHE_PATH = Path.Combine(APP_DIR, ".token.bin");

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
        // Instantiate client application
        var app = PublicClientApplicationBuilder.Create(CLIENT_ID)
            .WithAuthority(AUTHORITY)
            .WithRedirectUri("http://localhost")
            .Build();

        // Configure persistent token cache
        app.UserTokenCache.SetBeforeAccess(args =>
        {
            // Load the token from the cache file if it exists
            if (File.Exists(TOKEN_CACHE_PATH))
            {
                args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(TOKEN_CACHE_PATH));
            }
        });
        app.UserTokenCache.SetAfterAccess(args =>
        {
            if (args.HasStateChanged)
            {
                // Create the app data folder if it does not exist yet
                if (APP_DIR != null && !Directory.Exists(APP_DIR))
                {
                    Directory.CreateDirectory(APP_DIR);
                }
                // Save the token to the cache file
                File.WriteAllBytes(TOKEN_CACHE_PATH, args.TokenCache.SerializeMsalV3());
            }
        });

        // Acquire Token
        AuthenticationResult result;
        var accounts = await app.GetAccountsAsync();
        try
        {
            // Try silent sign-in first...
            result = await app.AcquireTokenSilent(SCOPES, accounts.FirstOrDefault()).ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            // ... otherwise, fallback to interactive browser flow
            result = await app.AcquireTokenInteractive(SCOPES)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
        }

        Console.WriteLine($"☑️ Logged in as user: {result.Account.Username}");

        // Plug token in graph
        var provider = new BaseBearerTokenAuthenticationProvider(
            new TokenProvider(result.AccessToken)
        );

        // Instantiate the Microsoft Graph client
        client = new GraphServiceClient(provider);
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

// Simple wrapper class implementing IAccessTokenProvider
class TokenProvider : IAccessTokenProvider
{
    private string _token;
    public TokenProvider(string token) => _token = token;

    public Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_token);
    }

    public AllowedHostsValidator AllowedHostsValidator => new();
}
