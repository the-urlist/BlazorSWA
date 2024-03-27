# The Urlist - Blazor Static Web App rewrite

[![BuiltWithDot.Net shield](https://builtwithdot.net/project/391/the-urlist-blazor/badge)](https://builtwithdot.net/project/391/the-urlist-blazor)

The Urlist is an application that lets you create lists of url’s that you can share with others. Get it? A list of URL’s? The Urlist? Listen, naming things is hard and all the good domains are already taken.

The original version of this site was [built in 2019 using Azure Storage, Azure Functions, Azure Front Door, and Vue](https://dev.to/azure/the-urlist-an-application-study-in-serverless-and-azure-2jk1). We originally decided to try a rewrite of this using modern Static Web Apps and Blazor when Twitter authentication became unreliable, trying this in Blazor because it's a new fun challenge. You can [watch us](https://aka.ms/burke-learns-blazor) live stream the effort on Fridays.

See the work in progress here 👉 [https://victorious-forest-0ccd7d90f.3.azurestaticapps.net](https://victorious-forest-0ccd7d90f.3.azurestaticapps.net)

## Project planning

We're tracking our work on [this GitHub project](https://github.com/orgs/the-urlist/projects/2).

## We take pull requests!

We'd love pull requests! Please file an issue for anything new, and communicate in advance before doing any major work. We'd rather not duplicate effort or have you work on something we can't use.

### Visual Studio 2022 setup

Once you clone the project, open the solution in the latest release of [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the Azure workload installed., and follow these steps:

1. Right-click on the solution and select **Set Startup Projects...**.

1. Select **Multiple startup projects** and set the following actions for each project:
    - *Api* - **Start**
    - *Client* - **Start**
    - *Shared* - None

1. Press **F5** to launch both the client application and the Functions API app.

### Visual Studio Code with Azure Static Web Apps CLI for a better development experience (Optional)

1. Install the [Azure Static Web Apps CLI](https://www.npmjs.com/package/@azure/static-web-apps-cli) and [Azure Functions Core Tools CLI](https://www.npmjs.com/package/azure-functions-core-tools).

1. Open the folder in Visual Studio Code.

1. Delete file `Client/wwwroot/appsettings.Development.json`

1. In the VS Code terminal, run the following command to start the Static Web Apps CLI, along with the Blazor WebAssembly client application and the Functions API app:

    ```bash
    swa start http://localhost:5000 --api-location http://localhost:7071
    ```

    The Static Web Apps CLI (`swa`) starts a proxy on port 4280 that will forward static site requests to the Blazor server on port 5000 and requests to the `/api` endpoint to the Functions server. 

1. Open a browser and navigate to the Static Web Apps CLI's address at `http://localhost:4280`. You'll be able to access both the client application and the Functions API app in this single address. When you navigate to the "Fetch Data" page, you'll see the data returned by the Functions API app.

1. Enter Ctrl-C to stop the Static Web Apps CLI.

## Template Structure

- **Client**: The Blazor WebAssembly sample application
- **Api**: A C# Azure Functions API, which the Blazor application will call
- **Shared**: A C# class library with a shared data model between the Blazor and Functions application

## Deploy to Azure Static Web Apps

This application can be deployed to [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps), to learn how, check out [our quickstart guide](https://aka.ms/blazor-swa/quickstart).
