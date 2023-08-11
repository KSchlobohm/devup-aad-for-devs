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

    1. The name is `DevUpConf2023 TodoListClient`

1. Tip #1: Diagnosing configuration issue with the Integration Assistant

    - We can see that there should be an owner, but also see that the required configurations are set:
        - RedirectUri
        - Issue id_tokens

1. Tip #2: Quickstarts
    - Open the quickstart blade on the new App Registration
1. Choose ASP.NET Core Web App
1. Apply the recommended changes
1. Download the code sample
1. Run the web app and observe

- We can see the Quickstart working as expected.

## Step 2: Test your App Registrations without code
I find it's useful to debug things in isolation. Let's view some options to debug APp Registrations.

**Demo**


1. Tip #3 - Testing Authentication with [authr.dev](https://authr.dev)
- Provide 2 settings and set the redirectUri on the app registration
    - TenantId
    - ClientId

- We can see there are no claims related to group information returned by default.

## Step 3: Introduce the web app

1. Run the web app as described in the README

    - observe CRUD for todo items from in memory dataset.

1. Open the csproj files and observe - no packages, no hidden code changes, all live.

## Step 4: Create an App Registration for the API

1. Define a new App Registration `DevUpConf2023 TodoListService`
1. Add two API scopes
    1. `ToDoList.Read` - Allow users to consent
    1. `ToDoList.ReadWrite` - Allow users to consent
1. Update the client to give the Client permission to access the API scopes

1. Use the App Registration from Azure AD and compare it to a known good state to check for differences.


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

1. Create a new Class the `TodoListDownstreamApi.cs` 

    -  Tip #5: Use the IDownstreamApi interface instead of HttpClient
    -  @todo - use snippet for this large change
    
    <br>

    ```cs
    public class TodoListDownstreamApi : ITodoListService
    {
        private IDownstreamApi _downstreamApi;

        public TodoListDownstreamApi(IDownstreamApi todoListService)
        {
            _downstreamApi = todoListService;
        }

        public Task CreateTodoItem(Todo todo)
        {
            return _downstreamApi.PostForUserAsync("TodoList", todo);
        }

        public Task DeleteTodoItem(int id)
        {
            return _downstreamApi.DeleteForUserAsync("TodoList", new Todo(),
                options => options.RelativePath = $"api/todolist/{id}");
        }

        public Task<Todo> GetTodoItem(int id)
        {
            return _downstreamApi.GetForUserAsync<Todo>(
                "TodoList",
                options => options.RelativePath = $"api/todolist/{id}")!;
        }

        public Task<IEnumerable<Todo>> GetTodoItems()
        {
            return _downstreamApi.GetForUserAsync<IEnumerable<Todo>>("TodoList")!;
        }

        public Task UpdateTodoItem(Todo data)
        {
            return _downstreamApi.CallApiForUserAsync<Todo, Todo>(
                "TodoList", data,
                options => { options.RelativePath = $"api/todolist/{data.Id}"; options.HttpMethod = HttpMethod.Patch; });
        }
    }
    ```

1. Register with dependency injection
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
        "ClientId": "ba629276-ae6c-4bed-a05e-d372509a2e4f"
    },
    "TodoList": {
        "Scopes": [ "api://ba629276-ae6c-4bed-a05e-d372509a2e4f/ToDoList.Read", "api://ba629276-ae6c-4bed-a05e-d372509a2e4f/ToDoList.ReadWrite" ],
        "RelativePath": "api/todolist",
        "BaseUrl": "https://localhost:7129/"
    },
    ```

1. Modify the TodoList Controller in the API to use account specific settings

    @todo - should use snippet for this large change

    ```cs
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        // In-memory TodoList
        private static readonly Dictionary<int, Todo> TodoStore = new Dictionary<int, Todo>();

        // This is needed to get access to the internal HttpContext.User, if available.
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// We store the object id of the user/app derived from the presented Access token
        /// </summary>
        private string _currentPrincipalId = string.Empty;

        public TodoListController(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;

            // We seek the details of the user/app represented by the access token presented to this API, This can be empty unless authN succeeded
            // If a user signed-in, the value will be the unique identifier of the user.
            _currentPrincipalId = GetCurrentClaimsPrincipal()?.GetObjectId()!;
            
            // Pre-populate with sample data
            if (TodoStore.Count == 0)
            {
                TodoStore.Add(1, new Todo() { Id = 1, Owner = $"{_currentPrincipalId}", Title = "Pick up groceries" });
                TodoStore.Add(2, new Todo() { Id = 2, Owner = $"{_currentPrincipalId}", Title = "Finish invoice report" });
            }
        }

        /// <summary>
        /// returns the current claimsPrincipal (user/Client app) dehydrated from the Access token
        /// </summary>
        /// <returns></returns>
        private ClaimsPrincipal GetCurrentClaimsPrincipal()
        {
            // Irrespective of whether a user signs in or not, the AspNet security middle-ware dehydrates the claims in the
            // HttpContext.User.Claims collection

            if (_contextAccessor.HttpContext != null && _contextAccessor.HttpContext.User != null)
            {
                return _contextAccessor.HttpContext.User;
            }

            return null;
        }

        [HttpGet()]
        public IEnumerable<Todo> Get()
        {
            // this is a request for all ToDo list items of a certain user.
            return TodoStore.Values.Where(x => x.Owner == _currentPrincipalId);
        }

        [HttpGet("{id}", Name = "Get")]
        public Todo Get(int id)
        {
            return TodoStore.Values.FirstOrDefault(todo => todo.Id == id && todo.Owner == _currentPrincipalId)!;
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            // only delete if the ToDo list item belonged to this user
            if (TodoStore.Values.Any(todo => todo.Id == id && todo.Owner == _currentPrincipalId))
            {
                TodoStore.Remove(id);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            // The signed-in user becomes the owner
            todo.Owner = _currentPrincipalId;
            int nextid = TodoStore.Values.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;

            todo.Id = nextid;
            TodoStore.Add(nextid, todo);
            return Created($"/todo/{nextid}", todo);
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            Todo existingToDo = TodoStore.Values.FirstOrDefault(todo => todo.Id == id)!;

            if (id != todo.Id || existingToDo == null)
            {
                return NotFound();
            }

            // a user can only modify their own ToDos
            if (existingToDo.Owner != _currentPrincipalId)
            {
                return Unauthorized();
            }

            // Overwrite ownership, just in case
            todo.Owner = _currentPrincipalId;

            TodoStore.Remove(id);
            TodoStore.Add(id, todo);
            return Ok(todo);
        }
    }
    ```

1. Test the code

    - might need to hit the "logout" url: https://localhost:44321/signout-oidc
    - "no login hint" - Tip #

## Step 7: Add authorization

1. Define a role on the Client App Registration
1. Use the enterprise app registration to edit role memberhship
1. Add myself as a user to the Admin Group
1. Modify the HomeController Index action to require Authorization

```cs
    [Authorize(Roles ="Admin")]
```

<!-- 
1. Let's talk about Tips
    1. Tip #4 - You only get roles, not parent/child membership. only the claims you selected, and group membership isn't hierarchical.
    1. Tip #5 - If you try to use SecurityGroups try to filter groups to the ones you know are relevant or use [Dynamic Groups](https://techcommunity.microsoft.com/t5/microsoft-entra-azure-ad-blog/create-quot-nested-quot-groups-with-azure-ad-dynamic-groups/ba-p/3118024)

## Step 8: Let's talk about Host Name Preservation
Let's talk about Tip #6 - learning about Host Name Preservation
One of the common problems for OpenID apps is setting the correct Redirect URI when behind a Reverse Proxy.

Here's what the problem looks like. [Host name preservation - Azure Architecture Center | Microsoft Learn](https://learn.microsoft.com/en-us/azure/architecture/best-practices/host-name-preservation#incorrect-redirect-urls)

1. Don't do this: https://github.com/Azure/reliable-web-app-pattern-dotnet/blob/3391894cb907df3971acd561af1ec83d0b4dca23/src/Relecloud.Web/Startup.cs#L202

1. Instead we want to just set one setting - ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

## Step 9: Closing out
Back to slides -->