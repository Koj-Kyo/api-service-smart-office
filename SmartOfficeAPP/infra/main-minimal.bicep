// =========================================
// ?? SmartOffice API - Infrastructure MINIMALE
// Déploiement ÉCONOMIQUE pour CI/CD GitHub Actions
// Coût estimé : ~10-15 USD/mois
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
  ManagedBy: 'GitHub-Actions'
  Purpose: 'Academic-Demo'
  CostOptimized: 'true'
}

// =========================================
// Variables calculées
// =========================================
var resourcePrefix = '${appName}-${environment}'
var webAppName = '${resourcePrefix}-api'
var appServicePlanName = '${resourcePrefix}-plan'

// =========================================
// Paramètres sensibles (depuis GitHub Secrets)
// =========================================
@secure()
@description('Secret JWT pour l\'authentification')
param jwtSecret string

@description('Connection string IoT Hub existant')
param iotHubConnectionString string = ''

@description('Connection string Event Hub existant')
param iotHubEventHubConnectionString string = ''

@description('Connection string Storage Account existant')
param storageConnectionString string = ''

@description('Connection string PostgreSQL externe (déjà créé)')
param postgresqlConnectionString string = ''

// =========================================
// ?? App Service Plan - FREE ou B1 (MINIMAL)
// =========================================
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: 'B1' // ?? Basic B1 : 1 Core, 1.75 GB RAM - ~13 USD/mois
    tier: 'Basic'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true // Linux obligatoire
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
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: false // ?? Désactivé pour économiser (pas de warm-up permanent)
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
          '*' // Pour la démo mobile
        ]
        supportCredentials: true
      }
      appSettings: [
        // ?? Connexion PostgreSQL (externe)
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: postgresqlConnectionString
        }
        // ?? JWT Settings
        {
          name: 'JwtSettings__Secret'
          value: jwtSecret
        }
        {
          name: 'JwtSettings__Issuer'
          value: 'SmartOfficeAPI'
        }
        {
          name: 'JwtSettings__Audience'
          value: 'SmartOfficeClient'
        }
        // ?? IoT Hub
        {
          name: 'IotHubConnectionString'
          value: iotHubConnectionString
        }
        {
          name: 'IotHubEventHub__ConnectionString'
          value: iotHubEventHubConnectionString
        }
        {
          name: 'IotHubEventHub__Enabled'
          value: 'true'
        }
        {
          name: 'IotHubEventHub__Name'
          value: 'brainoffice'
        }
        // ?? Storage Account
        {
          name: 'IotHubEventHub__BlobConnectionString'
          value: storageConnectionString
        }
        {
          name: 'IotHubEventHub__BlobContainer'
          value: 'iothub-checkpoints'
        }
        {
          name: 'IotHubEventHub__BlobContainerLogs'
          value: 'iothub-logs-checkpoints'
        }
        // ?? Device Sync
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
        // ?? Configuration ASP.NET Core
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'ASPNETCORE_URLS'
          value: 'http://+:8080'
        }
        // ?? Désactiver Redis (cache en mémoire)
        {
          name: 'ConnectionStrings__Redis'
          value: ''
        }
      ]
    }
  }
}

// =========================================
// ?? Outputs pour GitHub Actions
// =========================================
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
output resourceGroupName string = resourceGroup().name
output appServicePlanName string = appServicePlan.name
