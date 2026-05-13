targetScope = 'resourceGroup'

@description('Short environment name used in Azure resource names.')
@minLength(2)
@maxLength(20)
param environmentName string

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Container image to run. The workflow deploys once with the placeholder image, builds to ACR, then redeploys with the real app image.')
param containerImage string = 'mcr.microsoft.com/k8se/quickstart:latest'

@description('HTTP port exposed by the container.')
param containerPort int = 8080

@description('Container CPU allocation.')
param containerCpu string = '0.25'

@description('Container memory allocation.')
param containerMemory string = '0.5Gi'

@description('Minimum Container Apps replicas. Set to 0 for consumption scale-to-zero.')
param minReplicas int = 0

@description('Maximum Container Apps replicas.')
param maxReplicas int = 1

@description('Blob container used for uploaded and generated background files. $web enables static website delivery.')
param storageContainerName string = '$web'

@description('Azure OpenAI endpoint. Leave empty to disable AI background generation until configured.')
param openAiEndpoint string = ''

@secure()
@description('Azure OpenAI key. Stored as a Container Apps secret.')
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

var normalizedEnvironment = take(toLower(replace(replace(environmentName, '-', ''), '_', '')), 7)
var uniqueSuffix = uniqueString(resourceGroup().id, normalizedEnvironment)
var namePrefix = 'mct-${normalizedEnvironment}-${uniqueSuffix}'
var storageAccountName = take('mct${normalizedEnvironment}${uniqueSuffix}', 24)
var registryName = take('mct${normalizedEnvironment}${uniqueSuffix}acr', 50)
var keyVaultName = take('kv${normalizedEnvironment}${uniqueSuffix}', 24)
var appName = 'ca-${namePrefix}'
var usesPrivateRegistry = startsWith(containerImage, '${registry.properties.loginServer}/')
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

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: registryName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
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
  name: keyVaultName
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

resource containerIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'id-${namePrefix}'
  location: location
  tags: tags
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'cae-${namePrefix}'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  tags: tags
  dependsOn: [
    acrPull
  ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: containerPort
        transport: 'auto'
        allowInsecure: false
      }
      registries: usesPrivateRegistry ? [
        {
          server: registry.properties.loginServer
          identity: containerIdentity.id
        }
      ] : []
      secrets: [
        {
          name: 'jwt-secret'
          value: jwtSecret
        }
        {
          name: 'openai-key'
          value: empty(openAiKey) ? 'not-configured' : openAiKey
        }
        {
          name: 'pwd-reset-request-url'
          value: empty(pwdResetRequestUrl) ? 'not-configured' : pwdResetRequestUrl
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'mct-timer'
          image: containerImage
          env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights.properties.ConnectionString
            }
            {
              name: 'ApplicationInsights__ConnectionString'
              value: appInsights.properties.ConnectionString
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: containerIdentity.properties.clientId
            }
            {
              name: 'ConfigMng__ContainerName'
              value: storageContainerName
            }
            {
              name: 'ConfigMng__CosmosDBEndpoint'
              value: cosmosAccount.properties.documentEndpoint
            }
            {
              name: 'ConfigMng__FileSizeLimit'
              value: '10485760'
            }
            {
              name: 'ConfigMng__JWT'
              secretRef: 'jwt-secret'
            }
            {
              name: 'ConfigMng__KeyVault'
              value: keyVault.properties.vaultUri
            }
            {
              name: 'ConfigMng__MaxAIinTheDay'
              value: maxAiInTheDay
            }
            {
              name: 'ConfigMng__OpenAIEndpoint'
              value: openAiEndpoint
            }
            {
              name: 'ConfigMng__OpenAIKey'
              secretRef: 'openai-key'
            }
            {
              name: 'ConfigMng__OpenAIModel'
              value: openAiModel
            }
            {
              name: 'ConfigMng__PssKey'
              value: passwordKey.name
            }
            {
              name: 'ConfigMng__PwdResetRequestUrl'
              secretRef: 'pwd-reset-request-url'
            }
            {
              name: 'ConfigMng__StorageAccountName'
              value: storage.name
            }
            {
              name: 'ConfigMng__SubscriptionID'
              value: subscription().subscriptionId
            }
            {
              name: 'ConfigMng__TenantID'
              value: tenant().tenantId
            }
            {
              name: 'ConfigMng__WebCDN'
              value: storage.properties.primaryEndpoints.web
            }
          ]
          resources: {
            cpu: json(containerCpu)
            memory: containerMemory
          }
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
    }
  }
}

var storageBlobDataContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
var keyVaultCryptoUserRoleId = '12338af0-0e69-4776-bea7-57ae8d297424'
var cosmosDataContributorRoleDefinitionId = '00000000-0000-0000-0000-000000000002'
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'

resource storageBlobDataContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storage.id, containerIdentity.name, storageBlobDataContributorRoleId)
  scope: storage
  properties: {
    principalId: containerIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataContributorRoleId)
  }
}

resource keyVaultCryptoUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, containerIdentity.name, keyVaultCryptoUserRoleId)
  scope: keyVault
  properties: {
    principalId: containerIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultCryptoUserRoleId)
  }
}

resource acrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, containerIdentity.name, acrPullRoleId)
  scope: registry
  properties: {
    principalId: containerIdentity.properties.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
  }
}

resource cosmosDataContributor 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-05-15' = {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, containerIdentity.name, cosmosDataContributorRoleDefinitionId)
  properties: {
    principalId: containerIdentity.properties.principalId
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${cosmosDataContributorRoleDefinitionId}'
    scope: cosmosAccount.id
  }
}

output acrName string = registry.name
output acrLoginServer string = registry.properties.loginServer
output containerAppName string = containerApp.name
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output resourceGroupName string = resourceGroup().name
output storageStaticWebsiteUrl string = storage.properties.primaryEndpoints.web
output cosmosEndpoint string = cosmosAccount.properties.documentEndpoint
output keyVaultUri string = keyVault.properties.vaultUri
