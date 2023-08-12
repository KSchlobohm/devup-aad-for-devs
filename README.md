# Azure AD for devs - a primer
Learn about Azure AD - Microsoft's cloud-scale identity platform. It can be tricky to figure out how auth works, but once you know it's a powerful tool in your toolbox. In this repository we'll learn how it's done in a .NET web app. We'll discover what app registrations are, why we need one and how to wire up auth between a web app and an API backend.

## Getting Started
Run the app and observe the CRUD operations for TodListClient a .NET 7 ASP.NET Core MVC web app with a Web API backend that uses an in-memory dataset.

1. Set multiple startup projects

    ![#image showing that two start up projects should be selected](./docs/images/SetMultipleStartupProjects.png)

    https://learn.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects?view=vs-2022

1. Start the solution and view the TodoList page

    ![#image of the todolist page](./docs/images/ImageOfTheTodoListPage.png)

    You are now ready to use the demo script to learn about Azure AD App Registrations and the code changes needed to bring auth to your soltuion.


## Additional resources


### *Tip #1* Use Azure AD Integration Assistant to check your settings
   - [Best practices for the Microsoft identity platform - Microsoft Entra | Microsoft Learn](https://learn.microsoft.com/azure/active-directory/develop/identity-platform-integration-checklist)
   - [Configure your Azure AD application with Integration assistant - Waldek Mastykarz](https://blog.mastykarz.nl/configure-azure-ad-application-integration-assistant/)

   ![#image of the Azure AD integration assistant](./docs/images/ImageOfTheIntegrationAssistant.png)

### *Tip #2* Sample code for all your scenarios

   ![#image of Azure AD Quickstart](./docs/images/ImageOfTheQuickstartForAspNetCore.png)

   And other Azure AD Sample projects
   - [Code samples for Microsoft identity platform authentication and authorization - Microsoft Entra | Microsoft Learn](https://learn.microsoft.com/azure/active-directory/develop/sample-v2-code?tabs=apptype)
   - Reliable Web App Patterns
      - [Reliable web app pattern for .NET Azure Reference Architectures | Microsoft Learn](https://learn.microsoft.com/en-us/azure/architecture/web-apps/guides/reliable-web-app/dotnet/plan-implementation)
      - [Reliable web app pattern for Java - Azure Reference Architectures | Microsoft Learn](https://learn.microsoft.com/en-us/azure/architecture/web-apps/guides/reliable-web-app/java/plan-implementation)

### *Tip #3* Debug App Registrations with [authr.dev](https://authr.dev/)

   ![#image of the authr.dev website](./docs/images/ImageOfTheAuthrDevPage.png)

### *Tip #4* Compare App Registration Manifests (use that one that works)

   ![#image of app registration manifest](./docs/images/AppRegistrationManifest.png)

### *Tip #5* Use IDownstreamAPI to make API calls without managing tokens

   ![#image of the IDownstreamAPI in visual studio](./docs/images/UsingIDownstreamApiToAccessApis.png)

### *Tip #6* `IdentityModelEventSource.ShowPII` might show a more helpful error message

   ![#image of the IdentityModelEventSource property in code](./docs/images/ImageOfIdentityModelEventSource.png)

### *Tip #7* Use host name preservation with reverse proxies
   
   - [Host name preservation - Azure Architecture Center | Microsoft Learn](https://learn.microsoft.com/en-us/azure/architecture/best-practices/host-name-preservation)
   - [Deploying web apps to App Services as Linux containers · (github.com)](https://github.com/AzureAD/microsoft-identity-web/wiki/Deploying-Web-apps-to-App-services-as-Linux-containers)

   ![#image of invalid redirect issue related to host name preservatio](./docs/images/HostNamePreservationInvalidRedirectSample.png)

### *Tip #8* Sharing my experience
   - Use App Roles when you can
   - Azure AD changes are asynchronous – give it a minute to propagate
   - [Manage JSON Web Tokens in development with dotnet user-jwts | Microsoft Learn](https://learn.microsoft.com/aspnet/core/security/authentication/jwt-authn?view=aspnetcore-7.0&tabs=windows)


## Troubleshooting
Tips for dealing with issues.

**SocketException: No connection could be made...**
![#image of runtime connection error](./docs/images/SocketExceptionNoConnectionCouldBeMade.png)

- Configure the solution to set both web apps as startup projects.