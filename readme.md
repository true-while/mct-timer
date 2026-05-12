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

For local exaction the configuration file should be provided with following template: 

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
    "StorageAccountName": "https://<your storage acc name>.blob.core.windows.net/",
    "ContainerName": "<cosmos db container name for images>",
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
  "ApplicationInsights": "<cs copy from AI page>",  
  "AllowedHosts": "*"
}
````
## Security Config

The project configured to use *System assigned Managed Identity* with *DefaultAzureCredentialOptions*. 
Local runs should be implemented behalf of the VS login user. Alternatively you can use App registration and configure local environment variables described in the following [link](https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/local-development-service-principal?tabs=azure-cli%2Cwindows%2Ccommand-line#4---set-application-environment-variables)

For Keyvault connection you have to configure role 'Key Vault Crypto User' for the web site account and test account. 
For cosmos DB we recommend use RBAC assignment as explained in the following doc. By now custom role need to be manualy assigned as explained in following [link](https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-setup-rbac#metadata-requests)

