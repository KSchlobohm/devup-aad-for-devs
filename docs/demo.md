# Demo script

This demo capture changes shown in the Pull Request https://github.com/KSchlobohm/devup-aad-for-devs/pull/1/files

## Pre-requisite

1. You must have access to create an Azure AD App registration in your Azure AD tenant.
1. You must have dotnet 3.1 installed to run the demo from Azure AD quickstarts.

```ps
 dotnet --list-sdks
 ```

## Step 1: Create an MVC frontend App Registration

**Demo**

1. Open the Azure AD Portal and click `Create a new App Registration`

    ```
    DevUpConf2023 TodoListClient
    ```

1. <span style="color:black; background: yellow; padding: 3px">Tip:</span> Diagnosing configuration issue with the Integration Assistant

    - We can see that there should be an owner, but also see that the required configurations are set:
        - RedirectUri
        - Issue id_tokens

1. <span style="color:black; background: yellow; padding: 3px">Tip:</span> Quickstarts
    - Open the quickstart blade on the new App Registration
    - [And even more samples](https://learn.microsoft.com/azure/active-directory/develop/sample-v2-code?tabs=apptype)
1. Choose ASP.NET Core Web App
1. Apply the recommended changes
1. Download the code sample
1. Run the web app and observe (talk about manifest.json while that starts)

- We can see the Quickstart working as expected.

## Step 2: Test your App Registrations without code
I find it's useful to debug things in isolation. Let's view some options to debug APp Registrations.

**Demo**


1. <span style="color:black; background: yellow; padding: 3px">Tip:</span> - Testing Authentication with [authr.dev](https://authr.dev)
- Provide 2 settings and set the redirectUri on the app registration
    - TenantId
    - ClientId

- We can see there are no claims related to group information returned by default.

## Step 3: Introduce the web app

1. Run the web app as described in the README

    - observe CRUD for todo items from in memory dataset.

1. Open the csproj files and observe - no packages, no hidden code changes, all live.

## Step 4: Create an App Registration for the API

1. Define a new App Registration

    ```
    DevUpConf2023 TodoListService
    ```

1. Add two API scopes
    1. `ToDoList.Read` - Allow users to consent
    1. `ToDoList.ReadWrite` - Allow users to consent
1. Update the client to give the Client permission to access the API scopes

1. Use the App Registration from Azure AD and compare it to a known good state to check for differences.

- <span style="color:black; background: yellow; padding: 3px">Tip:</span> Compare App Registration Manifests (use that one that works)

## Step 5: Modify our frontend code to Authenticate users

1. Add the NuGet packages

    ```xml
        <ItemGroup>
            <PackageReference Include="Microsoft.Identity.Web" Version="2.13.2" />
            <PackageReference Include="Microsoft.Identity.Web.DownstreamApi" Version="2.13.2" />
            <PackageReference Include="Microsoft.Identity.Web.UI" Version="2.13.2" />
        </ItemGroup>
    ```

1. Modify the dependency injection

    ```cs
        public void ConfigureServices(IServiceCollection services)
        {
            // The following lines of code adds the ability to authenticate users of this web app.
            // Refer to https://github.com/AzureAD/microsoft-identity-web/wiki/web-apps to learn more
            services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi(
                        Configuration.GetSection("TodoList:Scopes").Get<string[]>()
                     )
                    .AddInMemoryTokenCaches();

            services.AddDownstreamApi("TodoList", Configuration.GetSection("TodoList"));

            services.AddControllersWithViews().AddMicrosoftIdentityUI();
            services.AddScoped<ITodoListService, TodoListDownstreamApi>();

        }
    ```

1. Create a new Class the `TodoListDownstreamApi.cs` 

    - <span style="color:black; background: yellow; padding: 3px">Tip</span> Use the IDownstreamApi interface instead of HttpClient
    -  implement with toolbox/snippet

1. Add the Login/Logout buttons

    ```cshtml
    @if (User.Identity.IsAuthenticated)
    {
        <ul class="navbar-nav navbar-right">
            <li class="navbar-text">Hello @User.GetDisplayName()!</li>
            <li class="nav-link text-dark"><a asp-area="MicrosoftIdentity" asp-controller="Account" asp-action="SignOut">Sign out</a></li>
        </ul>
    }
    else
    {
        <ul class="navbar-nav navbar-right">
            <li><a asp-area="MicrosoftIdentity" asp-controller="Account" asp-action="SignIn">Sign in</a></li>
        </ul>
    }
    ```

1. Modify the host configuration (replace app.UseRouting())

    - <span style="color:black; background: yellow; padding: 3px">Tip</span> is buried here: use this property to see better error messages when debugging your code.

    ```cs
        IdentityModelEventSource.ShowPII = true;
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
    ```

1. Modify the `appsettings.json`

    ```json
    {
    "AzureAd": {
        "Instance": "https://login.microsoftonline.com/",
        "Domain": "kenschlobohm.onmicrosoft.com",
        "TenantId": "562b981e-de62-4d8c-a9d9-1993c19bc572",
        "ClientId": "[from azure portal]",
        "ClientSecret": "[read from user secrets]"
    },
    "TodoList": {
        "Scopes": [ "api://[from azure portal]/ToDoList.Read", "api://[from azure portal]/ToDoList.ReadWrite" ],
        "RelativePath": "api/todolist",
        "BaseUrl": "https://localhost:7129/"
    },
    "Logging": {
        "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*"
    }
    ```

1. Right-click and add User Secrets

    ```json
    {
    "AzureAd:ClientSecret": "[from azure portal]"
    }
    ```

1. Add the authrize attribute to Client `TodoListController : Controller`

    ```cs
    [AuthorizeForScopes(ScopeKeySection = "TodoList:Scopes")]
    public class TodoListController : Controller
    ```

    - can't run the web app just yet... let's keep going

## Step 6: Modify our backend code to Authenticate users

1. Add the NuGet packages

    ```xml
        <ItemGroup>
            <PackageReference Include="Microsoft.Identity.Web" Version="2.13.2" />
        </ItemGroup>
    ```

1. Modify the dependency injection

    ```cs
        public void ConfigureServices(IServiceCollection services)
        {
            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
            services.AddMicrosoftIdentityWebApiAuthentication(Configuration);

            services.AddMvc(options =>
            {
                options.Filters.Add<CustomExceptionFilter>();
            });

            services.AddControllers();
        }
    ```

1. Modify the host configuration (replace app.UseRouting())

    ```cs
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
    ```
1. Modify the `appsettings.json`

    ```json
    "AzureAd": {
        "Instance": "https://login.microsoftonline.com/",
        "Domain": "kenschlobohm.onmicrosoft.com",
        "TenantId": "562b981e-de62-4d8c-a9d9-1993c19bc572",
        "ClientId": "[from azure portal]"
    }
    ```

1. Recreate the TodoList Controller in the API to use account specific settings

    - implement with toolbox/snippet
    

1. Test the code


## Step 7: Add authorization

1. Define a role on the Client App Registration
1. Use the enterprise app registration to edit role memberhship
1. Add myself as a user to the Admin Group
1. Modify the HomeController Index action to require Authorization

```cs
    [Authorize(Roles ="Admin")]
```