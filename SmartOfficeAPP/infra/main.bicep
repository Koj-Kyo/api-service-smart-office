// =========================================
// ?? SmartOffice API - Infrastructure principale
// Déploiement optimisé pour soutenance académique
// Région : East US | Environnement : Production
// =========================================

targetScope = 'resourceGroup'

@description('Nom de l\'application (préfixe pour toutes les ressources)')
param appName string = 'smartoffice'

@description('Environnement de déploiement')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'prod'

@description('Région Azure pour le déploiement')
param location string = resourceGroup().location

@description('Tags communs pour toutes les ressources')
param tags object = {
  Application: 'SmartOffice'
  Environment: environment
  ManagedBy: 'Bicep'
  Purpose: 'Academic-Presentation'
}

// =========================================
// Variables calculées
// =========================================
var resourcePrefix = '${appName}-${environment}'
var webAppName = '${resourcePrefix}-api'
var appServicePlanName = '${resourcePrefix}-plan'
var postgresqlServerName = '${resourcePrefix}-postgres'
var redisCacheName = '${resourcePrefix}-redis'
var keyVaultName = '${take(replace(resourcePrefix, '-', ''), 20)}kv' // KeyVault max 24 chars, no hyphens
var logAnalyticsWorkspaceName = '${resourcePrefix}-logs'

// =========================================
// Paramètres sensibles (seront dans Key Vault)
// =========================================
@secure()
@description('Mot de passe administrateur PostgreSQL')
param postgresqlAdminPassword string

@description('Nom d\'utilisateur administrateur PostgreSQL')
param postgresqlAdminUsername string = 'smartofficeadmin'

@secure()
@description('Secret JWT pour l\'authentification')
param jwtSecret string

@description('Connection string IoT Hub existant')
param iotHubConnectionString string = ''

@description('Connection string Storage Account existant')
param storageConnectionString string = ''

// =========================================
// ?? Log Analytics Workspace
// =========================================
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// =========================================
// ?? Azure Key Vault
// =========================================
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true // ? RBAC au lieu des access policies
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enablePurgeProtection: true // ? Protection contre suppression définitive
    networkAcls: {
      defaultAction: 'Allow' // Pour démo - en prod: 'Deny' + firewall rules
      bypass: 'AzureServices'
    }
  }
}

// Secrets stockés dans Key Vault
resource secretPostgresqlPassword 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'PostgreSQL-AdminPassword'
  properties: {
    value: postgresqlAdminPassword
  }
}

resource secretJwtSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'JWT-Secret'
  properties: {
    value: jwtSecret
  }
}

resource secretIotHubConnection 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(iotHubConnectionString)) {
  parent: keyVault
  name: 'IotHub-ConnectionString'
  properties: {
    value: iotHubConnectionString
  }
}

resource secretStorageConnection 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(storageConnectionString)) {
  parent: keyVault
  name: 'Storage-ConnectionString'
  properties: {
    value: storageConnectionString
  }
}

// =========================================
// ?? PostgreSQL Flexible Server
// =========================================
resource postgresqlServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: postgresqlServerName
  location: location
  tags: tags
  sku: {
    name: 'Standard_B2s' // 2 vCores, 4 GB RAM - Économique mais performant
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: postgresqlAdminUsername
    administratorLoginPassword: postgresqlAdminPassword
    version: '16' // ? PostgreSQL 16 (dernière version)
    storage: {
      storageSizeGB: 32
      autoGrow: 'Enabled'
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled' // Pour économie - en prod: 'Enabled'
    }
    highAvailability: {
      mode: 'Disabled' // Pour démo - en prod: 'ZoneRedundant'
    }
    network: {
      publicNetworkAccess: 'Enabled' // ? Accès public pour démo
    }
  }
}

// Règle firewall : autoriser services Azure
resource postgresqlFirewallRuleAzure 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresqlServer
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Règle firewall : autoriser toutes IP (SEULEMENT POUR DÉMO/SOUTENANCE)
resource postgresqlFirewallRuleAll 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresqlServer
  name: 'AllowAllIPs-Demo'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Base de données SmartOffice
resource postgresqlDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresqlServer
  name: 'smartofficedb'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.UTF8'
  }
}

// =========================================
// ?? Azure Cache for Redis
// =========================================
resource redisCache 'Microsoft.Cache/redis@2023-08-01' = {
  name: redisCacheName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0 // C0 - 250 MB (économique pour démo)
    }
    enableNonSslPort: false // ? Sécurité : SSL uniquement
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    redisConfiguration: {
      'maxmemory-policy': 'allkeys-lru' // ? Éviction LRU
    }
  }
}

// Secret Redis dans Key Vault
resource secretRedisConnection 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Redis-ConnectionString'
  properties: {
    value: '${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False'
  }
}

// =========================================
// ?? App Service Plan
// =========================================
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: 'S1' // Standard S1 : 1 Core, 1.75 GB RAM, auto-scaling
    tier: 'Standard'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true // ? Linux obligatoire
  }
}

// =========================================
// ?? App Service (Web API)
// =========================================
resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned' // ? Managed Identity pour Key Vault
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true // ? HTTPS uniquement
    clientAffinityEnabled: false // ? Stateless pour scaling
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0' // ? .NET 9
      alwaysOn: true // ? Toujours actif (pas de cold start)
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      healthCheckPath: '/health/ready'
      cors: {
        allowedOrigins: [
          'https://localhost:7152'
          'http://localhost:5249'
          'http://localhost:3000'
          'https://localhost:3000'
        ]
        supportCredentials: true
      }
      appSettings: [
        // ? Connexion PostgreSQL depuis Key Vault
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Host=${postgresqlServer.properties.fullyQualifiedDomainName};Database=smartofficedb;Username=${postgresqlAdminUsername};Password=${postgresqlAdminPassword};SSL Mode=Require'
        }
        // ? Redis depuis Key Vault
        {
          name: 'ConnectionStrings__Redis'
          value: '@Microsoft.KeyVault(SecretUri=${keyVault.properties.vaultUri}secrets/Redis-ConnectionString/)'
        }
        // ? JWT Settings
        {
          name: 'JwtSettings__Secret'
          value: '@Microsoft.KeyVault(SecretUri=${keyVault.properties.vaultUri}secrets/JWT-Secret/)'
        }
        {
          name: 'JwtSettings__Issuer'
          value: 'SmartOfficeAPI'
        }
        {
          name: 'JwtSettings__Audience'
          value: 'SmartOfficeClient'
        }
        // ? IoT Hub (si fourni)
        {
          name: 'IotHubConnectionString'
          value: !empty(iotHubConnectionString) ? '@Microsoft.KeyVault(SecretUri=${keyVault.properties.vaultUri}secrets/IotHub-ConnectionString/)' : ''
        }
        {
          name: 'IotHubEventHub__Enabled'
          value: 'true'
        }
        {
          name: 'IotHubEventHub__Name'
          value: 'brainoffice'
        }
        // ? Storage Account (si fourni)
        {
          name: 'IotHubEventHub__BlobConnectionString'
          value: !empty(storageConnectionString) ? '@Microsoft.KeyVault(SecretUri=${keyVault.properties.vaultUri}secrets/Storage-ConnectionString/)' : ''
        }
        {
          name: 'IotHubEventHub__BlobContainer'
          value: 'iothub-checkpoints'
        }
        // ? Device Sync Configuration
        {
          name: 'DeviceSync__IntervalMinutes'
          value: '5'
        }
        {
          name: 'DeviceSync__Enabled'
          value: 'true'
        }
        {
          name: 'DeviceSync__MaxParallelDevices'
          value: '5'
        }
        // ? Application Insights (monitoring)
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        // ? Configuration ASP.NET Core
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'ASPNETCORE_URLS'
          value: 'http://+:8080'
        }
      ]
    }
  }
}

// ? Assigner rôle Key Vault Secrets User à la Managed Identity
resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, webApp.id, 'Key Vault Secrets User')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // Key Vault Secrets User
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// =========================================
// ?? Application Insights
// =========================================
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${resourcePrefix}-insights'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// =========================================
// ?? Outputs pour référence
// =========================================
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
output postgresqlServerFqdn string = postgresqlServer.properties.fullyQualifiedDomainName
output postgresqlDatabaseName string = postgresqlDatabase.name
output redisHostName string = redisCache.properties.hostName
output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
output appInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey
output appInsightsConnectionString string = applicationInsights.properties.ConnectionString
output resourceGroupName string = resourceGroup().name
