# `mstodo-cli`

A Command-Line-Interface for Microsoft To Do.

---

## 💡 Usage

```pwsh
mstodo help
```

**Commands:**

-   `login`: Authenticate with Microsoft Graph Services.
-   `logout`: Log out from Microsoft Graph Services.
-   `user`: Show current user information.
    *   Example: `mstodo user`
    *   Example (JSON): `mstodo user --json`
-   `lists`: Show all your To Do lists.
    *   Example: `mstodo lists`
    *   Example (JSON): `mstodo lists --json`
-   `show <list_identifier> [--limit <number>]`: Show tasks in a specific To Do list.
    *   `<list_identifier>` can be the list's display name (e.g., "Tasks", "Ideas") or its 0-based index.
    *   `--limit <number>`: Optionally limit the number of tasks displayed.
    *   Example: `mstodo show Tasks`
    *   Example: `mstodo show Ideas --limit 5`
    *   Example (JSON): `mstodo show Tasks --json`
-   `add`: Add a new task (Not yet implemented).
-   `complete`: Complete a task (Not yet implemented).
-   `delete <list_identifier> <task_identifier>`: Delete a task in a specific To Do list.
    *   `<list_identifier>` can be the list's display name (e.g., "Tasks", "Ideas") or its 0-based index.
    *   `<task_identifier>` can be the task's title or its 0-based index within the list.
    *   Example: `mstodo delete Tasks "My Task"`
-   `help`: Show this help message.

---

## 🚀 Getting Started

Follow these steps to set up and run your own version of `mstodo-cli`.

### Prerequisites

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later.
-   A Microsoft account with access to Microsoft To Do.
-   An Microsoft Entra ID (Azure Active Directory) application registration (see next section).

### Clone the Repository

```pwsh
git clone https://github.com/your-repo/mstodo-cli.git
cd mstodo-cli
```

### Microsoft Entra ID (Azure Active Directory) Application Registration

To allow `mstodo-cli` to interact with your Microsoft To Do, you need to register an application in Microsoft Entra ID (previously Azure Active Directory).

1.  Go to the [Azure portal](https://portal.azure.com/).
2.  Select **App registrations** from the left-hand menu, then click **New registration**.
3.  **Name:** Enter a name for your application (e.g., `mstodo-cli`).
4.  **Supported account types:** Choose "Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)" to allow both work/school and personal Microsoft accounts.
5.  **Redirect URI:** Select `Public client/native (mobile & desktop)` and set the URI to `http://localhost`.
6.  Click **Register**.
7.  Once registered, note down the **Application (client) ID**. You will need this for configuration.
8.  Navigate to **API permissions** from the left-hand menu.
9.  Click **Add a permission**, then select **Microsoft Graph**.
10. Choose **Delegated permissions** and add the following permissions:
    *   `User.Read`
    *   `Tasks.ReadWrite`

### Configuration

Create a file named `appsettings.json` in the root directory of the project with the following content. Replace `<YOUR_CLIENT_ID>` with the Application (client) ID you obtained from Azure AD.

```json
{
  "AzureAd": {
    "ClientId": "<YOUR_CLIENT_ID>",
    "Authority": "https://login.microsoftonline.com/common",
    "RedirectUri": "http://localhost",
    "Scopes": [
        "User.Read",
        "Tasks.ReadWrite"
    ],
  }
}
```

For local development or overriding settings, you can create `appsettings.dev.json` or `appsettings.ovr.json`. These files are ignored by Git (as per `.gitignore`) and will override settings in `appsettings.json`.

### Build and Run

1.  **Build the project:**

```pwsh
dotnet build
```

2.  **Run the application:**

```pwsh
dotnet run -- <command> [arguments]
```

Add the built executable to `PATH`.

---

## 📄 License

This project is licensed under the [MIT License](./LICENSE)
