# ?? SmartOffice API - Commandes Utiles (Cheatsheet)

## ?? Déploiement Azure

### Déploiement complet (Infrastructure + Code)
```powershell
# Windows
.\deploy.ps1 up

# Linux/Mac
./deploy.sh up

# Ou directement avec azd
azd up
```

### Déploiement par étapes
```powershell
# 1. Provisionner infrastructure uniquement
azd provision

# 2. Déployer le code uniquement
azd deploy
```

### Voir l'état du déploiement
```powershell
.\deploy.ps1 status
# ou
azd env list
azd env get-values
```

## ?? Gestion des Secrets

### Charger variables d'environnement (.env)
```powershell
# Windows PowerShell
Get-Content .env | ForEach-Object {
    if ($_ -match '^([^=]+)=(.*)$') {
        [Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process')
    }
}

# Linux/Mac
export $(grep -v '^#' .env | xargs)
```

### Obtenir les connection strings existantes
```powershell
# IoT Hub Connection String
az iot hub connection-string show --hub-name brainoffice --policy-name service

# Storage Account Connection String
az storage account show-connection-string --name <storage-account-name> --resource-group <rg-name>

# Redis Connection String (depuis Azure)
az redis list-keys --name smartoffice-prod-redis --resource-group smartoffice-prod-rg
```

### Gérer les secrets Key Vault
```powershell
# Récupérer le nom du Key Vault
$kvName = azd env get-value AZURE_KEY_VAULT_NAME

# Lister tous les secrets
az keyvault secret list --vault-name $kvName

# Voir un secret
az keyvault secret show --vault-name $kvName --name "JWT-Secret"

# Ajouter/modifier un secret
az keyvault secret set --vault-name $kvName --name "NewSecret" --value "SecretValue"
```

## ?? Monitoring & Logs

### Ouvrir Application Insights
```powershell
azd monitor
# ou
.\deploy.ps1 monitor
```

### Voir les logs en temps réel
```powershell
azd monitor --logs
# ou
.\deploy.ps1 logs
```

### Logs App Service
```powershell
$webAppName = azd env get-value AZURE_WEBAPP_NAME
$resourceGroup = azd env get-value AZURE_RESOURCE_GROUP

# Logs en temps réel
az webapp log tail --name $webAppName --resource-group $resourceGroup

# Télécharger les logs
az webapp log download --name $webAppName --resource-group $resourceGroup --log-file logs.zip
```

### Métriques App Service
```powershell
az monitor metrics list `
  --resource "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$resourceGroup/providers/Microsoft.Web/sites/$webAppName" `
  --metric "CpuPercentage" "MemoryPercentage" "HttpResponseTime" `
  --interval PT1M
```

## ??? Gestion Base de Données

### Connexion PostgreSQL
```powershell
$postgresHost = azd env get-value AZURE_POSTGRESQL_FQDN
$postgresDb = "smartofficedb"
$postgresUser = "smartofficeadmin"

# Se connecter avec psql
psql "host=$postgresHost dbname=$postgresDb user=$postgresUser sslmode=require"

# Ou avec connection string complète
$connectionString = azd env get-value AZURE_POSTGRESQL_CONNECTION_STRING
psql "$connectionString"
```

### Migrations Entity Framework
```powershell
cd SmartOfficeAPP

# Créer une nouvelle migration
dotnet ef migrations add NomDeLaMigration

# Appliquer les migrations
dotnet ef database update

# Avec une connection string spécifique
dotnet ef database update --connection "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"

# Rollback à une migration précédente
dotnet ef database update NomDeLaMigrationPrecedente

# Supprimer la dernière migration (si pas encore appliquée)
dotnet ef migrations remove
```

### Backup PostgreSQL
```powershell
$resourceGroup = azd env get-value AZURE_RESOURCE_GROUP
$serverName = azd env get-value AZURE_POSTGRESQL_SERVER

# Créer un backup manuel
az postgres flexible-server backup create `
  --resource-group $resourceGroup `
  --name $serverName `
  --backup-name "backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Lister les backups
az postgres flexible-server backup list `
  --resource-group $resourceGroup `
  --name $serverName
```

## ?? Gestion App Service

### Contrôle de l'App Service
```powershell
$webAppName = azd env get-value AZURE_WEBAPP_NAME
$resourceGroup = azd env get-value AZURE_RESOURCE_GROUP

# Démarrer
az webapp start --name $webAppName --resource-group $resourceGroup

# Arrêter
az webapp stop --name $webAppName --resource-group $resourceGroup

# Redémarrer
az webapp restart --name $webAppName --resource-group $resourceGroup

# Voir l'état
az webapp show --name $webAppName --resource-group $resourceGroup --query state
```

### Configuration App Service
```powershell
# Lister les App Settings
az webapp config appsettings list --name $webAppName --resource-group $resourceGroup

# Ajouter/modifier un App Setting
az webapp config appsettings set `
  --name $webAppName `
  --resource-group $resourceGroup `
  --settings "MyNewSetting=MyValue"

# Supprimer un App Setting
az webapp config appsettings delete `
  --name $webAppName `
  --resource-group $resourceGroup `
  --setting-names "MyNewSetting"
```

### Scaling App Service
```powershell
$planName = azd env get-value AZURE_APP_SERVICE_PLAN

# Scale up (changer le SKU)
az appservice plan update `
  --name $planName `
  --resource-group $resourceGroup `
  --sku S2

# Scale out (augmenter instances)
az appservice plan update `
  --name $planName `
  --resource-group $resourceGroup `
  --number-of-workers 2
```

## ?? Tests & Validation

### Health Checks
```powershell
$webAppUrl = azd env get-value AZURE_WEBAPP_URL

# Test health ready
Invoke-WebRequest -Uri "$webAppUrl/health/ready" -UseBasicParsing

# Test health live
Invoke-WebRequest -Uri "$webAppUrl/health/live" -UseBasicParsing

# Test Swagger
Start-Process "$webAppUrl/swagger"
```

### Tests API avec curl
```bash
# Health check
curl https://smartoffice-prod-api.azurewebsites.net/health/ready

# Login (obtenir JWT token)
curl -X POST https://smartoffice-prod-api.azurewebsites.net/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@smartoffice.com","password":"Admin123!"}'

# Appel authentifié
curl https://smartoffice-prod-api.azurewebsites.net/api/Dashboard/stats \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Tests locaux
```powershell
cd SmartOfficeAPP

# Lancer l'API localement
dotnet run

# Lancer avec hot reload
dotnet watch run

# Tests unitaires
dotnet test

# Tests avec coverage
dotnet test /p:CollectCoverage=true
```

## ?? Dépannage

### Problème : Key Vault access denied
```powershell
# Attendre 30 secondes puis redéployer
Start-Sleep -Seconds 30
azd deploy
```

### Problème : PostgreSQL connection failed
```powershell
# Ajouter votre IP au firewall
$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
az postgres flexible-server firewall-rule create `
  --resource-group $resourceGroup `
  --name $serverName `
  --rule-name "MyLocalMachine" `
  --start-ip-address $myIp `
  --end-ip-address $myIp
```

### Problème : App Service ne démarre pas
```powershell
# Voir les logs de démarrage
az webapp log tail --name $webAppName --resource-group $resourceGroup

# Voir les erreurs stderr
az webapp log download --name $webAppName --resource-group $resourceGroup
```

### Problème : Health check failing
```powershell
# Vérifier la configuration
az webapp config show --name $webAppName --resource-group $resourceGroup

# Vérifier les variables d'environnement
az webapp config appsettings list --name $webAppName --resource-group $resourceGroup

# Redémarrer l'app
az webapp restart --name $webAppName --resource-group $resourceGroup
```

## ?? Nettoyage

### Arrêter les services (économiser)
```powershell
# Arrêter App Service uniquement
az webapp stop --name $webAppName --resource-group $resourceGroup

# Arrêter PostgreSQL (pas de facturation en pause)
az postgres flexible-server stop --name $serverName --resource-group $resourceGroup
```

### Supprimer complètement
```powershell
# ATTENTION : Cela supprime TOUT
azd down --force --purge

# Ou manuellement
az group delete --name smartoffice-prod-rg --yes --no-wait
```

## ?? CI/CD

### GitHub Actions - Re-run workflow
```bash
# Depuis GitHub UI : Actions > Re-run all jobs

# Ou avec GitHub CLI
gh workflow run azure-deploy.yml
gh run list --workflow=azure-deploy.yml
```

### Obtenir Service Principal credentials
```powershell
# Créer un Service Principal
az ad sp create-for-rbac `
  --name "github-actions-smartoffice" `
  --role contributor `
  --scopes /subscriptions/$(az account show --query id -o tsv)/resourceGroups/smartoffice-prod-rg `
  --sdk-auth
```

## ?? Package Management

### Mettre à jour packages NuGet
```powershell
cd SmartOfficeAPP

# Lister les packages obsolètes
dotnet list package --outdated

# Mettre à jour un package spécifique
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.9

# Mettre à jour tous les packages (attention!)
dotnet outdated --upgrade
```

## ?? Azure CLI Essentials

### Authentification
```powershell
# Login
az login

# Login avec un Service Principal
az login --service-principal -u <app-id> -p <password-or-cert> --tenant <tenant-id>

# Voir le compte actif
az account show

# Changer d'abonnement
az account set --subscription "SUBSCRIPTION_ID"
```

### Resource Groups
```powershell
# Lister tous les Resource Groups
az group list --output table

# Créer un Resource Group
az group create --name smartoffice-dev-rg --location eastus

# Supprimer un Resource Group
az group delete --name smartoffice-dev-rg --yes --no-wait
```

### Portail Azure
```powershell
# Ouvrir le portail sur le Resource Group
$resourceGroup = azd env get-value AZURE_RESOURCE_GROUP
$subscriptionId = az account show --query id -o tsv
Start-Process "https://portal.azure.com/#@/resource/subscriptions/$subscriptionId/resourceGroups/$resourceGroup"
```

## ?? Documentation & Aide

### azd help
```powershell
azd --help
azd up --help
azd provision --help
azd deploy --help
```

### Azure CLI help
```powershell
az --help
az webapp --help
az postgres flexible-server --help
```

### Ouvrir la documentation
```powershell
# Documentation azd
Start-Process "https://learn.microsoft.com/azure/developer/azure-developer-cli/"

# Documentation Azure CLI
Start-Process "https://learn.microsoft.com/cli/azure/"

# Documentation Bicep
Start-Process "https://learn.microsoft.com/azure/azure-resource-manager/bicep/"
```

---

## ?? Commandes les plus utilisées

### Pour le développement quotidien
```powershell
# 1. Déployer des changements de code
azd deploy

# 2. Voir les logs
azd monitor --logs

# 3. Tester l'API
$url = azd env get-value AZURE_WEBAPP_URL
Start-Process "$url/swagger"
```

### Pour la soutenance
```powershell
# 1. Démarrer tout
az webapp start --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# 2. Ouvrir monitoring
azd monitor

# 3. Tester health
Invoke-WebRequest -Uri "$(azd env get-value AZURE_WEBAPP_URL)/health/ready"

# 4. Arrêter après (économiser)
az webapp stop --name smartoffice-prod-api --resource-group smartoffice-prod-rg
```

### Pour le dépannage
```powershell
# 1. Logs en temps réel
az webapp log tail --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# 2. Redémarrer
az webapp restart --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# 3. Vérifier la config
az webapp config appsettings list --name smartoffice-prod-api --resource-group smartoffice-prod-rg
```

---

**?? Astuce** : Ajoutez ces commandes à votre historique PowerShell avec `Set-PSReadLineOption -HistorySaveStyle SaveIncrementally`
