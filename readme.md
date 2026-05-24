## Project description

[![Provision and deploy MCT Timer](https://github.com/dngoins/mct-timer/actions/workflows/azure-webapps-dotnet-core.yml/badge.svg?branch=main)](https://github.com/dngoins/mct-timer/actions/workflows/azure-webapps-dotnet-core.yml)
[![Dependabot](https://img.shields.io/badge/dependabot-enabled-success?logo=dependabot)](.github/dependabot.yml)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core/mvc/overview)
[![Bicep](https://img.shields.io/badge/IaC-Bicep-0078D4?logo=azurepipelines&logoColor=white)](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
[![Azure Container Apps](https://img.shields.io/badge/Azure-Container%20Apps-0078D4?logo=microsoftazure&logoColor=white)](https://learn.microsoft.com/azure/container-apps/)
[![Cosmos DB](https://img.shields.io/badge/Azure-Cosmos%20DB-0078D4?logo=microsoftazure&logoColor=white)](https://learn.microsoft.com/azure/cosmos-db/)

[![License](https://img.shields.io/github/license/dngoins/mct-timer)](LICENSE)
[![Last commit](https://img.shields.io/github/last-commit/dngoins/mct-timer)](https://github.com/dngoins/mct-timer/commits/main)
[![Open issues](https://img.shields.io/github/issues/dngoins/mct-timer)](https://github.com/dngoins/mct-timer/issues)
[![Open PRs](https://img.shields.io/github/issues-pr/dngoins/mct-timer)](https://github.com/dngoins/mct-timer/pulls)
[![Contributors](https://img.shields.io/github/contributors/dngoins/mct-timer)](https://github.com/dngoins/mct-timer/graphs/contributors)
[![Stars](https://img.shields.io/github/stars/dngoins/mct-timer?style=social)](https://github.com/dngoins/mct-timer/stargazers)
[![Forks](https://img.shields.io/github/forks/dngoins/mct-timer?style=social)](https://github.com/dngoins/mct-timer/network/members)
[![Top language](https://img.shields.io/github/languages/top/dngoins/mct-timer)](https://github.com/dngoins/mct-timer)
[![Code size](https://img.shields.io/github/languages/code-size/dngoins/mct-timer)](https://github.com/dngoins/mct-timer)
[![Latest release](https://img.shields.io/github/v/release/dngoins/mct-timer?include_prereleases&sort=semver)](https://github.com/dngoins/mct-timer/releases)

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fdngoins%2Fmct-timer%2Fmain%2Finfra%2Fmain.bicep)

The "MCT Timer" projectis an innovative tool designed to assist the Microsoft Certified Trainer (MCT) community. It simplifies the process of setting up and managing timers for classroom sessions directly from a web page. This project aims to enhance the teaching and learning experience by providing a seamless way to notify learners when classes resume.

### Project Hosting
The MCT Timer can be hosted on Azure Container Apps using the infrastructure and GitHub Actions workflow included in this repository. This keeps the app deployable without an Azure App Service Plan while still using managed Azure services for storage, identity, data, and monitoring.

One of the core principles of the MCT Timer project is accessibility. The website is publicly accessible, meaning anyone can use it without any restrictions. Moreover, the service is provided free of charge, making it an invaluable resource for the MCT community and beyond.

### Features and Benefits
- Easy Setup: The web interface is user-friendly, allowing trainers to quickly set up timers with minimal effort.
- Real-Time Notifications: Learners are promptly informed when classes will resume, ensuring that everyone stays on schedule.
- Public Access: The site is accessible to all, fostering a collaborative and inclusive learning environment.
- Cost-Free: The service is provided at no cost, removing any financial barriers to its use.
- Background customization: The service will allowed to upload personal background for different timers and generate custom background with Open AI model DALE
- Spotify playlist support: Trainers can attach a public Spotify playlist to a timer session and play it from the countdown page.


![Timer setup page](docs/images/setup-page.png)

The countdown page shows the remaining time and resume-at clock, the trainer's main message, an optional showcase video (top right), and an embedded Spotify playlist player (bottom right) when a playlist has been attached to the session.

![Countdown page with Spotify playlist and showcase video](docs/images/countdown-spotify-video.png)


## Architecture

The MCT Timer runs as a containerized ASP.NET Core MVC app on Azure Container Apps. All Azure resources are provisioned by `infra/main.bicep` and the app authenticates to data and secret services with a user-assigned managed identity (no connection strings or keys in app settings).

Azure resources:

- **Azure Container Apps** hosts the ASP.NET Core MVC site. A user-assigned managed identity is attached for all outbound Azure calls.
- **Azure Container Apps Environment** provides the runtime and integrates with Log Analytics.
- **Azure Container Registry** stores the application container image; the app pulls with `AcrPull` via managed identity.
- **Azure Cosmos DB (SQL API)** persists timer sessions, user profiles, instructor profiles, and customization metadata. Access is via Cosmos SQL Data Contributor on the managed identity (RBAC, no keys).
- **Azure Storage (Blob)** stores user-uploaded backgrounds, showcase media, and DALL-E generated images. Access is via Storage Blob Data Contributor on the managed identity.
- **Azure Key Vault** stores the cryptographic key used to encrypt sensitive user data. The app uses Key Vault Crypto User via managed identity.
- **Azure OpenAI (DALL-E 3)** generates optional AI background images from user prompts.
- **Application Insights + Log Analytics** capture telemetry, logs, and request traces.
- **External (no Azure resource):** the countdown page embeds Spotify's public playlist player and an optional YouTube showcase video directly in the browser; the app never calls these services server-side and stores no credentials.

```mermaid
flowchart LR
    User([Trainer / Learners<br/>Browser])

    subgraph Azure["Azure subscription"]
        direction LR
        ACR[Azure Container Registry<br/>app container image]

        subgraph CAE["Container Apps Environment"]
            CA["Container App<br/>ASP.NET Core MVC<br/>(MCT Timer)"]
        end

        MI[User-assigned<br/>Managed Identity]
        Cosmos[(Azure Cosmos DB<br/>SQL API)]
        Blob[(Azure Storage<br/>Blob container)]
        KV[Azure Key Vault<br/>encryption key]
        OpenAI[Azure OpenAI<br/>DALL-E 3]
        AppI[Application Insights]
        Logs[Log Analytics]
    end

    subgraph External["External services (browser-only)"]
        Spotify[Spotify embed player]
        YouTube[YouTube embed player]
    end

    User -- HTTPS --> CA
    User -. iframe .-> Spotify
    User -. iframe .-> YouTube

    CA -- pulls image --> ACR
    CA -- uses --> MI
    MI -- Cosmos Data Contributor --> Cosmos
    MI -- Blob Data Contributor --> Blob
    MI -- Crypto User --> KV
    CA -- generate image --> OpenAI
    CA -- telemetry --> AppI
    CAE -- logs --> Logs
```

CI/CD: `.github/workflows/azure-webapps-dotnet-core.yml` builds the container image, pushes it to ACR, and deploys to the Container App. The workflow uses federated OIDC credentials so no client secrets are stored in GitHub.

## Spotify Playlist Support

Timer setup includes an optional **Spotify Playlist** field. Paste a public Spotify playlist URL, Spotify playlist URI, Spotify embed URL, or raw playlist ID before starting a timer. The timer stores only the normalized playlist embed URL in the timer session URL; it does not use Spotify credentials, OAuth, private APIs, or paid API features.

Supported input formats:

- `https://open.spotify.com/playlist/{playlistId}`
- `https://open.spotify.com/playlist/{playlistId}?si=abc123`
- `spotify:playlist:{playlistId}`
- `https://open.spotify.com/embed/playlist/{playlistId}`
- `{playlistId}`

The countdown page shows a Spotify embedded playlist player when a playlist is provided. Music does not autoplay on page load; use **Play playlist** and then the Spotify play button inside the embedded player. Use the countdown page controls to show or hide the player, stop playback, or clear the playlist from the timer session. When the timer reaches zero, the embedded player is reset so music does not continue unnoticed.

## Session Details and Showcase Media

Timer setup can include optional session details that travel with the timer URL: session title, customer name, region/location, class hours, instructor name, and instructor LinkedIn or QR-code image links. The countdown page displays those details in a slim header above the timer and can show instructor profile links at the bottom of the screen. A bundled LinkedIn QR image is available at `/profile/linkedin-qr.png` and is publicly served by the app when deployed.

The setup page also supports:

- AI-generated background: creates a one-off background from the session title and context when Azure OpenAI image generation is configured. If configuration is missing or generation fails, the timer shows a friendly message and keeps the standard background.
- Feeling lucky Bing background: loads the latest Bing image of the day on each countdown page refresh.
- Custom background upload: pick a PNG, JPEG, or WebP image (up to 4 MB) from the setup page. When provided, the uploaded image is used as the countdown background and overrides both the Bing daily image and AI-generated backgrounds. Uploads are saved under `wwwroot/uploads/session/` with a randomized name and are not associated with any user account.
- Showcase media: displays an image, animated GIF, video file, YouTube video, or Vimeo video above the message editor for advertisements, upcoming topics, references, or instructor promotional content. Media must be a public URL; local file paths must be uploaded or hosted first. Videos autoplay muted when browser policy allows it.
- Quick countdown controls: **+1 min** and **+5 min** buttons on the countdown page extend the active timer without restarting the session.


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
    "OpenAIModel": "<name of the DALL-E image deployment>",
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

- Azure Container Apps for the ASP.NET Core web app
- Azure Container Registry for the application image
- Application Insights and Log Analytics
- Azure Cosmos DB for NoSQL database `webapp` and container `Users`
- Azure Storage static website container for uploaded/generated backgrounds
- Azure Key Vault with an RSA key for password encryption
- Managed identity and RBAC assignments for the container app

The storage account enables static website hosting for the `$web` background image container during the GitHub Actions workflow after ARM provisioning creates the account. That requires blob public access to be enabled at the storage account level, and the workflow passes the resulting static website endpoint into the Container App configuration for public background delivery.

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

The workflow creates a resource group, provisions Bicep resources, builds the container image in Azure Container Registry, and creates RBAC assignments for the container app managed identity. For a fully automated first deployment, assign these roles at subscription scope:

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
| `AZURE_OPENAI_ENDPOINT` | Optional Azure OpenAI endpoint. Required for AI background generation. |
| `AZURE_OPENAI_KEY` | Optional Azure OpenAI key. Required for AI background generation. |
| `PWD_RESET_REQUEST_URL` | Optional password reset email endpoint |

Create these **Actions variables**:

| Variable | Value |
|----------|-------|
| `AZURE_ENV_NAME` | `dev` or another short environment name |
| `AZURE_LOCATION` | Azure region, for example `eastus2` |
| `AZURE_OPENAI_MODEL` | Optional image model deployment name, for example `dall-e-3`. Required for AI background generation. |

AI background generation uses an existing Azure OpenAI image deployment; the deployment workflow does not create Azure OpenAI capacity for you. Leave these settings empty to disable AI backgrounds, or set the endpoint, key, and deployment name to enable the checkbox on the timer setup page.

### 5. Run the deployment workflow

Push to `main`, or run **Provision and deploy MCT Timer** from the GitHub Actions tab. The manual workflow lets you override `environmentName` and `location`.

The workflow runs tests, provisions Bicep infrastructure, builds the .NET 10 container image with ACR Tasks, pushes it to Azure Container Registry, and updates the Azure Container App with the new image tag.

### Container Apps and registry quota notes

This deployment no longer creates an App Service Plan, so App Service worker quota is not required. Azure Container Apps and Azure Container Registry can still be subject to regional availability, subscription policy, or resource provider registration requirements.

Fix it by doing one of the following:

- Run the workflow in another Azure region by changing `AZURE_LOCATION`.
- Register the required resource providers: `Microsoft.App`, `Microsoft.ContainerRegistry`, `Microsoft.OperationalInsights`, `Microsoft.DocumentDB`, `Microsoft.Storage`, and `Microsoft.KeyVault`.
- If Container Apps capacity is unavailable in a region, choose a nearby region with Container Apps support.

## .NET Aspire local orchestration

The solution includes an Aspire AppHost project in `mct-timer.AppHost/`. Use it for local orchestration and Aspire dashboard support:

```bash
dotnet run --project ./mct-timer.AppHost/mct-timer.AppHost.csproj
```

Production deployment uses Azure Container Apps so the app does not depend on Azure App Service.

## Security Config

The deployed container app uses a user-assigned managed identity with `DefaultAzureCredentialOptions`.
Local runs should be implemented behalf of the VS login user. Alternatively you can use App registration and configure local environment variables described in the following [link](https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/local-development-service-principal?tabs=azure-cli%2Cwindows%2Ccommand-line#4---set-application-environment-variables)

For Keyvault connection you have to configure role 'Key Vault Crypto User' for the container app managed identity and test account.
For cosmos DB we recommend use RBAC assignment as explained in the following doc. By now custom role need to be manualy assigned as explained in following [link](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-setup-rbac#metadata-requests)

