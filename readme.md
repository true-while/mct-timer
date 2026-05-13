## Project description

The "MCT Timer" project is an innovative tool designed to assist the Microsoft Certified Trainer (MCT) community. It simplifies the process of setting up and managing timers for classroom sessions directly from a web page. This project aims to enhance the teaching and learning experience by providing a seamless way to notify learners when classes resume.

### Project Hosting
The MCT Timer is hosted on a robust and reliable platform, the Microsoft Azure Web App. This ensures that the service is scalable, secure, and always available for users. The project is managed and maintained by the Microsoft Trainers Team, who bring their expertise and dedication to ensure its smooth operation and continuous improvement.

One of the core principles of the MCT Timer project is accessibility. The website is publicly accessible, meaning anyone can use it without any restrictions. Moreover, the service is provided free of charge, making it an invaluable resource for the MCT community and beyond.

### Features and Benefits
- Easy Setup: The web interface is user-friendly, allowing trainers to quickly set up timers with minimal effort.
- Real-Time Notifications: Learners are promptly informed when classes will resume, ensuring that everyone stays on schedule.
- Public Access: The site is accessible to all, fostering a collaborative and inclusive learning environment.
- Cost-Free: The service is provided at no cost, removing any financial barriers to its use.
- Background customization: The service will allowed to upload personal background for different timers and generate custom background with Open AI model DALE
- Spotify playlist support: Trainers can attach a public Spotify playlist to a timer session and play it from the countdown page.


## Architecture

The project are contains from following resources deploy in Azure.  
- Azure App Services used for hosting ASP core MVC project. 
- Azure Cosmos DB is used to persist metadata and information about customized settings and user profiles and user generated backgrounds.
- Azure Storage account will be used for persisting customized images. 
- Azure Function will be used for compression and conversion of customized images. 
- Azure Keyvault is used to persist cryptography keys for encrypt user's sensitive information.
- Azure Open AI service provisioned DALE3 model that used for image generation.

![schema](schena.png)

## Spotify Playlist Support

Timer setup includes an optional **Spotify Playlist** field. Paste a public Spotify playlist URL, Spotify playlist URI, Spotify embed URL, or raw playlist ID before starting a timer. The timer stores only the normalized playlist embed URL in the timer session URL; it does not use Spotify credentials, OAuth, private APIs, or paid API features.

Supported input formats:

- `https://open.spotify.com/playlist/{playlistId}`
- `https://open.spotify.com/playlist/{playlistId}?si=abc123`
- `spotify:playlist:{playlistId}`
- `https://open.spotify.com/embed/playlist/{playlistId}`
- `{playlistId}`

The countdown page shows a Spotify embedded playlist player when a playlist is provided. Music does not autoplay on page load; use **Play playlist** and then the Spotify play button inside the embedded player. Use the countdown page controls to show or hide the player, stop playback, or clear the playlist from the timer session. When the timer reaches zero, the embedded player is reset so music does not continue unnoticed.


## Configuration

For local execution the configuration file should be provided with following template:

```JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConfigMng": {
    "OpenAIEndpoint": "https://<your service>.openai.azure.com/",
    "OpenAIKey": "<your key copied from portal>",
    "OpenAIModel": "<name of the dale model>",
    "StorageAccountName": "<your storage account name>",
    "ContainerName": "<blob container name for images, usually $web>",
    "JWT": "<generated token to encrypt jwt>",
    "KeyVault": "<keyvault address>",
    "PssKey": "<key name>",
    "FileSizeLimit": "max file size in bite (int)",
    "WebCDN": "url of static website created for storage account",
    "CosmosDBEndpoint": "https://<your cosmos acc name>.documents.azure.com:443/",
    "TenantID": "<your tenant guid>",
    "SubscriptionID": "<your subscription guid>",
    "PwdResetRequestUrl": "https://..."          

  },
  "ApplicationInsights": {
    "ConnectionString": "<connection string copied from Application Insights>"
  },
  "AllowedHosts": "*"
}
````

## Deploy to your Azure subscription with GitHub Actions

This repository includes Bicep infrastructure in `infra/` and a GitHub Actions workflow in `.github/workflows/azure-webapps-dotnet-core.yml`. The workflow uses GitHub OpenID Connect (OIDC) with Microsoft Entra ID, so you do not need to store an Azure publish profile or Azure password in GitHub.

The workflow provisions:

- Azure App Service for the ASP.NET Core web app
- Application Insights and Log Analytics
- Azure Cosmos DB for NoSQL database `webapp` and container `Users`
- Azure Storage static website container for uploaded/generated backgrounds
- Azure Key Vault with an RSA key for password encryption
- Managed identity and RBAC assignments for the web app

### 1. Create the Microsoft Entra app registration

Run these commands from Azure Cloud Shell or a local terminal with Azure CLI. Replace `<owner>` and `<repo>` with your GitHub owner and repository name, and keep the environment name aligned with the GitHub environment used by the workflow, for example `dev`.

```bash
az login
az account set --subscription "<your-subscription-id>"

SUBSCRIPTION_ID=$(az account show --query id -o tsv)
TENANT_ID=$(az account show --query tenantId -o tsv)
APP_ID=$(az ad app create --display-name "mct-timer-github" --query appId -o tsv)
az ad sp create --id "$APP_ID"
SP_OBJECT_ID=$(az ad sp show --id "$APP_ID" --query id -o tsv)
```

### 2. Add the federated identity credential

Create a file named `credential.json`:

```json
{
  "name": "github-dev-environment",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<owner>/<repo>:environment:dev",
  "description": "Allow GitHub Actions from the dev environment to deploy MCT Timer",
  "audiences": [
    "api://AzureADTokenExchange"
  ]
}
```

Then attach it to the app registration:

```bash
az ad app federated-credential create \
  --id "$APP_ID" \
  --parameters credential.json
```

If you use a different GitHub environment name, update both the workflow environment and the `subject` value. If you deploy from multiple environments, add one federated credential per environment.

### 3. Assign Azure roles to the GitHub deployment identity

The workflow creates a resource group, provisions Bicep resources, and creates RBAC assignments for the web app managed identity. For a fully automated first deployment, assign these roles at subscription scope:

```bash
SCOPE="/subscriptions/$SUBSCRIPTION_ID"

az role assignment create \
  --assignee-object-id "$SP_OBJECT_ID" \
  --assignee-principal-type ServicePrincipal \
  --role "Contributor" \
  --scope "$SCOPE"

az role assignment create \
  --assignee-object-id "$SP_OBJECT_ID" \
  --assignee-principal-type ServicePrincipal \
  --role "Role Based Access Control Administrator" \
  --scope "$SCOPE"
```

After the first deployment, you can reduce scope to the created resource group if you do not need the workflow to create new resource groups.

### 4. Configure GitHub secrets and variables

In your GitHub repository, create these **Actions secrets**:

| Secret | Value |
|--------|-------|
| `AZURE_CLIENT_ID` | `$APP_ID` from the app registration |
| `AZURE_TENANT_ID` | `$TENANT_ID` |
| `AZURE_SUBSCRIPTION_ID` | `$SUBSCRIPTION_ID` |
| `JWT_SECRET` | Strong random value used by the app for JWT/ALTCHA signing |
| `AZURE_OPENAI_ENDPOINT` | Optional Azure OpenAI endpoint |
| `AZURE_OPENAI_KEY` | Optional Azure OpenAI key |
| `PWD_RESET_REQUEST_URL` | Optional password reset email endpoint |

Create these **Actions variables**:

| Variable | Value |
|----------|-------|
| `AZURE_ENV_NAME` | `dev` or another short environment name |
| `AZURE_LOCATION` | Azure region, for example `eastus2` |
| `APP_SERVICE_SKU_NAME` | App Service Plan SKU, defaults to `S1`; examples: `S1`, `P0v3`, `B1` |
| `AZURE_OPENAI_MODEL` | Optional image model deployment name, for example `dall-e-3` |

### 5. Run the deployment workflow

Push to `main`, or run **Provision and deploy MCT Timer** from the GitHub Actions tab. The manual workflow lets you override `environmentName` and `location`.

The workflow runs tests, publishes the .NET 10 app, provisions Bicep infrastructure, and deploys the published output to the provisioned Azure Web App.

### App Service quota errors

If the provisioning step fails with `SubscriptionIsOverQuotaForSku`, the selected App Service Plan SKU is not available in that subscription and region. The workflow defaults to `S1` to avoid subscriptions that have `Basic VMs` quota set to 0.

Fix it by doing one of the following:

- Run the workflow in another Azure region by changing `AZURE_LOCATION`.
- Choose a SKU with available quota by setting `APP_SERVICE_SKU_NAME`, for example `S1`, `P0v3`, or `P1v3`.
- Request quota for the blocked SKU family in the Azure portal.

## .NET Aspire local orchestration

The solution includes an Aspire AppHost project in `mct-timer.AppHost/`. Use it for local orchestration and Aspire dashboard support:

```bash
dotnet run --project ./mct-timer.AppHost/mct-timer.AppHost.csproj
```

Production deployment remains Azure App Service because this application is a single ASP.NET Core MVC web app and does not need a container-orchestrated production topology.

## Security Config

The project configured to use *System assigned Managed Identity* with *DefaultAzureCredentialOptions*. 
Local runs should be implemented behalf of the VS login user. Alternatively you can use App registration and configure local environment variables described in the following [link](https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/local-development-service-principal?tabs=azure-cli%2Cwindows%2Ccommand-line#4---set-application-environment-variables)

For Keyvault connection you have to configure role 'Key Vault Crypto User' for the web site account and test account. 
For cosmos DB we recommend use RBAC assignment as explained in the following doc. By now custom role need to be manualy assigned as explained in following [link](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-setup-rbac#metadata-requests)

