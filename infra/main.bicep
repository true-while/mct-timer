targetScope = 'resourceGroup'

@description('Short environment name used in Azure resource names.')
@minLength(2)
@maxLength(20)
param environmentName string

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('App Service Plan SKU.')
param appServiceSkuName string = 'S1'

@description('Blob container used for uploaded and generated background files. $web enables static website delivery.')
param storageContainerName string = '$web'

@description('Azure OpenAI endpoint. Leave empty to disable AI background generation until configured.')
param openAiEndpoint string = ''

@secure()
@description('Azure OpenAI key. Stored as an App Service setting.')
param openAiKey string = ''

@description('Azure OpenAI image model deployment name.')
param openAiModel string = 'dall-e-3'

@description('Maximum AI image generations per user per day.')
param maxAiInTheDay string = '5'

@secure()
@description('JWT/ALTCHA signing secret for the app.')
param jwtSecret string

@secure()
@description('Optional password reset callback URL.')
param pwdResetRequestUrl string = ''

@description('Key Vault key name used by the app for password encryption.')
param cryptographyKeyName string = 'password-encryption'

var normalizedEnvironment = take(toLower(replace(replace(environmentName, '-', ''), '_', '')), 12)
var uniqueSuffix = uniqueString(resourceGroup().id, normalizedEnvironment)
var namePrefix = 'mct-${normalizedEnvironment}-${uniqueSuffix}'
var storageAccountName = take('mct${normalizedEnvironment}${uniqueSuffix}', 24)
var tags = {
  application: 'mct-timer'
  environment: environmentName
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${namePrefix}'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${namePrefix}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
  properties: {
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    // Bicep type metadata omits staticWebsite for this API version, but ARM accepts it
    // and the app relies on the storage website endpoint for background images.
    #disable-next-line BCP037
    staticWebsite: {
      enabled: true
      indexDocument: 'index.html'
      error404Document: '404.html'
    }
  }
}

resource backgroundContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: storageContainerName
  properties: {
    publicAccess: 'None'
  }
}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: 'cosmos-${namePrefix}'
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    disableLocalAuth: true
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  parent: cosmosAccount
  name: 'webapp'
  properties: {
    resource: {
      id: 'webapp'
    }
  }
}

resource usersContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = {
  parent: cosmosDatabase
  name: 'Users'
  properties: {
    resource: {
      id: 'Users'
      partitionKey: {
        paths: [
          '/Email'
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
      }
    }
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'kv-${namePrefix}'
  location: location
  tags: tags
  properties: {
    enableRbacAuthorization: true
    enabledForTemplateDeployment: true
    publicNetworkAccess: 'Enabled'
    sku: {
      family: 'A'
      name: 'standard'
    }
    softDeleteRetentionInDays: 7
    tenantId: tenant().tenantId
  }
}

resource passwordKey 'Microsoft.KeyVault/vaults/keys@2023-07-01' = {
  parent: keyVault
  name: cryptographyKeyName
  properties: {
    kty: 'RSA'
    keySize: 2048
    keyOps: [
      'encrypt'
      'decrypt'
      'wrapKey'
      'unwrapKey'
    ]
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'plan-${namePrefix}'
  location: location
  tags: tags
  sku: {
    name: appServiceSkuName
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-${namePrefix}'
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    enabled: true
    httpsOnly: true
    serverFarmId: appServicePlan.id
    siteConfig: {
      alwaysOn: appServiceSkuName != 'F1'
      ftpsState: 'Disabled'
      healthCheckPath: '/health'
      http20Enabled: true
      linuxFxVersion: 'DOTNETCORE|10.0'
      minTlsVersion: '1.2'
    }
  }
}

resource webAppSettings 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: webApp
  name: 'appsettings'
  properties: {
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    ApplicationInsights__ConnectionString: appInsights.properties.ConnectionString
    ConfigMng__ContainerName: storageContainerName
    ConfigMng__CosmosDBEndpoint: cosmosAccount.properties.documentEndpoint
    ConfigMng__FileSizeLimit: '10485760'
    ConfigMng__JWT: jwtSecret
    ConfigMng__KeyVault: keyVault.properties.vaultUri
    ConfigMng__MaxAIinTheDay: maxAiInTheDay
    ConfigMng__OpenAIEndpoint: openAiEndpoint
    ConfigMng__OpenAIKey: openAiKey
    ConfigMng__OpenAIModel: openAiModel
    ConfigMng__PssKey: passwordKey.name
    ConfigMng__PwdResetRequestUrl: pwdResetRequestUrl
    ConfigMng__StorageAccountName: storage.name
    ConfigMng__SubscriptionID: subscription().subscriptionId
    ConfigMng__TenantID: tenant().tenantId
    ConfigMng__WebCDN: storage.properties.primaryEndpoints.web
    SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
  }
}

var storageBlobDataContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var keyVaultCryptoUserRoleId = '12338af0-0e69-4776-bea7-57ae8d297424'
var cosmosDataContributorRoleDefinitionId = '00000000-0000-0000-0000-000000000002'

resource storageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, webApp.name, storageBlobDataContributorRoleId)
  scope: storage
  properties: {
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRoleId)
  }
}

resource keyVaultCryptoUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, webApp.name, keyVaultCryptoUserRoleId)
  scope: keyVault
  properties: {
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultCryptoUserRoleId)
  }
}

resource cosmosDataContributor 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, webApp.name, cosmosDataContributorRoleDefinitionId)
  properties: {
    principalId: webApp.identity.principalId
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${cosmosDataContributorRoleDefinitionId}'
    scope: cosmosAccount.id
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output resourceGroupName string = resourceGroup().name
output storageStaticWebsiteUrl string = storage.properties.primaryEndpoints.web
output cosmosEndpoint string = cosmosAccount.properties.documentEndpoint
output keyVaultUri string = keyVault.properties.vaultUri
