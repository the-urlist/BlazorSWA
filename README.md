# The Urlist - Blazor Static Web App rewrite

[![BuiltWithDot.Net shield](https://builtwithdot.net/project/391/the-urlist-blazor/badge)](https://builtwithdot.net/project/391/the-urlist-blazor)

The Urlist is an application that lets you create lists of url’s that you can share with others. Get it? A list of URL’s? The Urlist? Listen, naming things is hard and all the good domains are already taken.

The original version of this site was [built in 2019 using Azure Storage, Azure Functions, Azure Front Door, and Vue](https://dev.to/azure/the-urlist-an-application-study-in-serverless-and-azure-2jk1). We originally decided to try a rewrite of this using modern Static Web Apps and Blazor when Twitter authentication became unreliable, trying this in Blazor because it's a new fun challenge. You can [watch us](https://aka.ms/burke-learns-blazor) live stream the effort on Fridays.

See the work in progress here 👉 [https://victorious-forest-0ccd7d90f.3.azurestaticapps.net](https://victorious-forest-0ccd7d90f.3.azurestaticapps.net)

## Project planning

We're tracking our work on [this GitHub project](https://github.com/orgs/the-urlist/projects/2).

## We take pull requests!

We'd love pull requests! Please file an issue for anything new, and communicate in advance before doing any major work. We'd rather not duplicate effort or have you work on something we can't use.

## Setup of the project in local environment

### Visual Studio 2022

Once you clone the project, open the solution in the latest release of [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) with the Azure workload installed., and follow these steps:

1. Right-click on the solution and select **Set Startup Projects...**.

1. Select **Multiple startup projects** and set the following actions for each project:

   - _Api_ - **Start**
   - _Client_ - **Start**
   - _Shared_ - None

1. Press **F5** to launch both the client application and the Functions API app.

### Visual Studio Code with Azure Static Web Apps CLI for a better development experience (Optional)

1. Install the [Azure Static Web Apps CLI](https://www.npmjs.com/package/@azure/static-web-apps-cli) and [Azure Functions Core Tools CLI](https://www.npmjs.com/package/azure-functions-core-tools).

2. Open the folder in Visual Studio Code.

3. Azure Static Web Apps CLI enables the user to a mock authentication by using the route "/.auth/login/`<provider>`". This project has configured 3 providers to make the login:

   - Google
   - GitHub
   - Twitter (X)

   In order to use one of them or all of them, you will have to comment the section with the provider name of your choice in the file `staticwebapp.config.json`, because otherwise the application will search for a proper provider CLIENT_ID.

4. In the VS Code terminal, run the following command to start the Static Web Apps CLI, along with the Blazor WebAssembly client application and the Functions API app:

   ```bash
   swa start
   ```

   The Static Web Apps CLI (`swa`) starts a proxy on port 4280 that will forward static site requests to the Blazor server on port 5000 and requests to the `/api` endpoint to the Functions server.

5. Open a browser and navigate to the Static Web Apps CLI's address at `http://localhost:4280`. You'll be able to access both the client application and the Functions API app in this single address. When you navigate to the "Fetch Data" page, you'll see the data returned by the Functions API app.

6. Enter Ctrl-C to stop the Static Web Apps CLI.

### NOTE

In order to test the application properly, a few extra steps must be done:

1. Install [CosmosDB Emulator](https://docs.azure.cn/en-us/cosmos-db/how-to-develop-emulator?tabs=windows%2Ccsharp&pivots=api-nosql) on your machine. After the installation, you can run the software and a browser should be opened or you can open it, and navigate to the url `https://localhost:8081/_explorer/index.html`. This will open the home page of the Emulator
2. Setup the following environment variables in the `Api` project:
   - COSMOSDB_ENDPOINT --> The URI field you will find in the home page of the CosmosDB Emulator once started
   - COSMOSDB_KEY --> The PrimaryKey field you will find in the home page of the CosmosDB Emulator once started
   - COSMOSDB_DATABASE --> Whatever you want (eg. "UrlList")
   - COSMOSDB_CONTAINER --> Whatever you want (eg. "Links")
   - HASHER_KEY --> Whatever you want
   - HASHER_SALT --> Whatever you want

After the execution of the above steps, when the application will run it will create a database and a container with the specified parameters.

## Template Structure

- **Client**: The Blazor WebAssembly sample application
- **Api**: A C# Azure Functions API, which the Blazor application will call
- **Shared**: A C# class library with a shared data model between the Blazor and Functions application

## Deploy to Azure Static Web Apps

This application can be deployed to [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps), to learn how, check out [our quickstart guide](https://aka.ms/blazor-swa/quickstart).
