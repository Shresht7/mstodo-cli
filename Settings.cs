// Library
using Microsoft.Extensions.Configuration;

public class Settings
{
    /// <summary>The Application Client ID</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>The Tenant ID</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>The Redirect URI</summary>
    public string RedirectUri { get; set; } = "http://localhost";

    /// <summary>The permission scopes</summary>
    public IEnumerable<string> Scopes { get; set; } = [];

    /// <summary>The Authority URL</summary>
    public string AuthorityUrl => $"https://login.microsoftonline.com/{TenantId}";

    /// <summary>
    /// Loads the application settings from the `appsettings.json` file
    /// </summary>
    /// <returns>The application settings</returns>
    /// <exception cref="Exception"></exception>
    public static Settings Load()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")    // Required
            .AddJsonFile("appsettings.dev.json", optional: true)    // Optional. Overrides base settings
            .AddJsonFile("appsettings.ovr.json", optional: true)    // Optional. Overrides all other settings
            .Build();
        return config.Get<Settings>() ?? throw new Exception("Failed to load application settings!");
    }
}
