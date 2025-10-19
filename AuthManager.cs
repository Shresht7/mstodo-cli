// Library
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;

// ------------
// AUTH MANAGER
// ------------

class AuthManager
{
    /// <summary>Application Client ID</summary>
    const string CLIENT_ID = "2157a77b-da98-48e8-8240-2d26d1dbe0b4";

    /// <summary>Authority URL</summary>
    const string AUTHORITY = "https://login.microsoftonline.com/common";

    /// <summary>Permission scopes requested by the application</summary>
    static readonly string[] SCOPES = ["User.Read", "Tasks.ReadWrite"];

    /// <summary>Name of the token cache file</summary>
    static readonly string TOKEN_FILE = ".token.bin";

    /// <summary>
    /// Initialize the Microsoft Graph Client and login as the user
    /// </summary>
    public static async Task<GraphServiceClient> InitMicrosoftGraph(string appDir)
    {
        // Path to the token cache file
        string tokenCachePath = Path.Combine(appDir, TOKEN_FILE);

        // Instantiate client application
        var app = PublicClientApplicationBuilder.Create(CLIENT_ID)
            .WithAuthority(AUTHORITY)
            .WithRedirectUri("http://localhost")
            .Build();

        // Configure persistent token cache
        app.UserTokenCache.SetBeforeAccess(args =>
        {
            // Load the token from the cache file if it exists
            if (File.Exists(tokenCachePath))
            {
                args.TokenCache.DeserializeMsalV3(File.ReadAllBytes(tokenCachePath));
            }
        });
        app.UserTokenCache.SetAfterAccess(args =>
        {
            if (args.HasStateChanged)
            {
                // Create the app data folder if it does not exist yet
                if (appDir != null && !Directory.Exists(appDir))
                {
                    Directory.CreateDirectory(appDir);
                }
                // Save the token to the cache file
                File.WriteAllBytes(tokenCachePath, args.TokenCache.SerializeMsalV3());
            }
        });        // Acquire Token
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
        return new GraphServiceClient(provider);
    }
}

// --------------
// TOKEN PROVIDER
// --------------

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
