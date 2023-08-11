Configuration Demo script

This demo capture changes shown in the Pull Request https://github.com/KSchlobohm/devup-aad-for-devs/pull/1/files

## Pre-requisite

1. You must have access to create an Azure AD App registration in your Azure AD tenant.

## Step 1: Create an MVC frontend App Registration

**Demo**

1. Open the Azure AD Portal and click `Create a new App Registration`
    1. The name is `DevUpConf2023 TodoListClient`

1. Tip #1 Quickstarts
    - Open the quickstart blade on the new App Registration
1. Choose ASP.NET Core Web App
1. Apply the recommended changes
1. Download the code sample
1. Run the web app and observe

- We can see the Quickstart working as expected.

## Step 2: Test your App Registrations without code
I find it's useful to debug things in isolation. Let's view some options to debug APp Registrations.

**Demo**

1. Tip #2 - Diagnosing configuration issue with the Integration Assistant

- We can see that there should be an owner, but also see that the required configurations are set:
    - RedirectUri
    - Issue id_tokens

1. Tip #3 - Testing Authentication with [authr.dev](https://authr.dev)
- Provide 2 settings and set the redirectUri on the app registration
    - TenantId
    - ClientId

- We can see there are no claims related to group information returned by default.

## Step 3: Introduce the web app

1. Run the web app as described in the README

    - observe CRUD for todo items from inmemory dataset

1. Open the csproj files and observe - no packages, no hidden code changes, all live.

## Step 4: Create an App Registration for the API

1. Define a new App Registration `DevUpConf2023 TodoListService`
1. Add two API scopes
    1. `ToDoList.Read` - Allow users to consent
    1. `ToDoList.ReadWrite` - Allow users to consent
1. Update the client to give the Client permission to access the API scopes

## Step 5: Modify our frontend code to Authenticate users

1. Add the NuGet packages
1. Modify the dependency injection
1. Modify the host configuration
1. Modify the `appsettings.json`

    ```json
    {
    "AzureAd": {
        "Instance": "https://login.microsoftonline.com/",
        "Domain": "kenschlobohm.onmicrosoft.com",
        "TenantId": "562b981e-de62-4d8c-a9d9-1993c19bc572",
        "ClientId": "d0a9907a-33c2-492b-a593-b6bbec705942",
        "ClientSecret": "[read from user secrets]"
    },
    "TodoList": {
        "Scopes": [ "api://ba629276-ae6c-4bed-a05e-d372509a2e4f/ToDoList.Read", "api://ba629276-ae6c-4bed-a05e-d372509a2e4f/ToDoList.ReadWrite" ],
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
    }```

1. Right-click and add User Secrets

    ```json
    {
    "AzureAd:ClientSecret": "[Value from Azure AD portal]"
    }
    ```

1. Create a new Class the `TodoListDownstreamApi.cs` 

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
                options => options.RelativePath = $"api/todolist/{id}");
        }

        public Task<IEnumerable<Todo>> GetTodoItems()
        {
            return _downstreamApi.GetForUserAsync<IEnumerable<Todo>>("TodoList");
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
1. Modify the dependency injection
1. Modify the host configuration
1. Modify the `appsettings.json`

## Step 7: Add authorization

1. Define a role on the Client App Registration
1. Use the enterprise app registration to edit role memberhship
1. Add myself as a user to the Admin Group
1. Modify the HomeController Index action to require Authorization

```cs
    [Authorize(Roles ="Admin")]
```

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
Back to slides