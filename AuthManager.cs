// Library
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions.Authentication;

// ------------
// AUTH MANAGER
// ------------

class AuthManager
{
    /// <summary>Name of the token cache file</summary>
    static readonly string TOKEN_FILE = ".token.bin";

    /// <summary>
    /// Initialize the Microsoft Graph Client and login as the user
    /// </summary>
    public static async Task<GraphServiceClient> InitMicrosoftGraph(string appDir, Settings settings)
    {
        // Path to the token cache file
        string tokenCachePath = Path.Combine(appDir, TOKEN_FILE);

        // Instantiate client application
        var app = PublicClientApplicationBuilder.Create(settings.ClientId)
            .WithAuthority(settings.AuthorityUrl)
            .WithRedirectUri(settings.RedirectUri)
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
            result = await app.AcquireTokenSilent(settings.Scopes, accounts.FirstOrDefault()).ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            // ... otherwise, fallback to interactive browser flow
            result = await app.AcquireTokenInteractive(settings.Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();
        }

        // Plug token in graph
        var provider = new BaseBearerTokenAuthenticationProvider(
            new TokenProvider(result.AccessToken)
        );

        // Instantiate the Microsoft Graph client
        return new GraphServiceClient(provider);
    }

    public static async Task<GraphServiceClient> Login(string appDir, Settings settings)
    {
        return await InitMicrosoftGraph(appDir, settings);
    }

    /// <summary>
    /// Log out the user by clearing the token cache
    /// </summary>
    public static async Task Logout(string appDir, Settings settings)
    {
        string tokenCachePath = Path.Combine(appDir, TOKEN_FILE);

        // Delete the token cache file
        if (File.Exists(tokenCachePath))
        {
            File.Delete(tokenCachePath);
        }

        // Clear accounts from the MSAL cache
        var app = PublicClientApplicationBuilder.Create(settings.ClientId)
            .Build();
        var accounts = await app.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await app.RemoveAsync(account);
        }
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
