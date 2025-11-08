# =========================================
# ?? Guide de Déploiement SmartOffice API
# Déploiement Azure avec Infrastructure as Code (Bicep)
# Optimisé pour soutenance académique
# =========================================

## ?? Table des matières
1. [Prérequis](#prérequis)
2. [Architecture déployée](#architecture-déployée)
3. [Déploiement rapide](#déploiement-rapide)
4. [Configuration détaillée](#configuration-détaillée)
5. [Déploiement CI/CD avec GitHub Actions](#déploiement-cicd-avec-github-actions)
6. [Commandes utiles](#commandes-utiles)
7. [Dépannage](#dépannage)
8. [Points importants pour la soutenance](#points-importants-pour-la-soutenance)

---

## ?? Prérequis

### Outils requis
```powershell
# 1. Azure CLI (version 2.50+)
az --version
# Si non installé : https://aka.ms/installazurecli

# 2. Azure Developer CLI (azd)
azd version
# Si non installé : https://aka.ms/install-azd

# 3. .NET 9 SDK
dotnet --version
# Si non installé : https://dotnet.microsoft.com/download

# 4. Entity Framework Core Tools
dotnet tool install --global dotnet-ef --version 9.*
```

### Compte Azure
- Abonnement Azure actif
- Droits Contributor sur l'abonnement

---

## ??? Architecture déployée

### Services Azure provisionnés

| Service | SKU | Coût mensuel estimé | Usage |
|---------|-----|---------------------|-------|
| **App Service Plan** | Standard S1 | ~70 USD | Hébergement API |
| **PostgreSQL Flexible** | Burstable B2s | ~30 USD | Base de données |
| **Redis Cache** | Basic C0 | ~17 USD | Cache distribué |
| **Key Vault** | Standard | ~1 USD | Secrets management |
| **Application Insights** | Pay-as-you-go | ~5 USD | Monitoring |
| **IoT Hub** | Existant | - | Gestion IoT |
| **Storage Account** | Existant | - | Checkpoints |

**Coût total estimé : ~123 USD/mois** (production)  
?? *Arrêtez les services après la soutenance pour économiser*

### Diagramme d'architecture

```
???????????????????????????????????????????????????????????????
?                     Azure Cloud (East US)                    ?
?                                                               ?
?  ????????????????      ???????????????????                  ?
?  ?  Azure App   ????????  Key Vault      ?                  ?
?  ?  Service     ?      ?  (Secrets)      ?                  ?
?  ?  (.NET 9)    ?      ???????????????????                  ?
?  ????????????????                                            ?
?         ?                                                     ?
?         ???????????? ????????????????????                   ?
?         ?            ?  PostgreSQL      ?                   ?
?         ?            ?  Flexible Server ?                   ?
?         ?            ????????????????????                   ?
?         ?                                                     ?
?         ???????????? ????????????????????                   ?
?         ?            ?  Redis Cache     ?                   ?
?         ?            ?  (Distributed)   ?                   ?
?         ?            ????????????????????                   ?
?         ?                                                     ?
?         ???????????? ????????????????????                   ?
?         ?            ?  IoT Hub         ?                   ?
?         ?            ?  (brainoffice)   ?                   ?
?         ?            ????????????????????                   ?
?         ?                                                     ?
?         ???????????? ????????????????????                   ?
?                      ?  Application     ?                   ?
?                      ?  Insights        ?                   ?
?                      ????????????????????                   ?
???????????????????????????????????????????????????????????????
```

---

## ?? Déploiement rapide (5 minutes)

### Étape 1 : Authentification Azure

```powershell
# Se connecter à Azure
az login

# Vérifier l'abonnement actif
az account show

# Si vous avez plusieurs abonnements, sélectionner le bon
az account set --subscription "VOTRE_SUBSCRIPTION_ID"
```

### Étape 2 : Préparer les secrets

```powershell
# Créer le fichier .env depuis le template
Copy-Item .env.example .env

# Éditer .env avec vos valeurs :
# - AZURE_SUBSCRIPTION_ID (obtenu via az account show --query id -o tsv)
# - POSTGRESQL_ADMIN_PASSWORD (ex: SmartOffice2025!)
# - JWT_SECRET (votre secret actuel depuis appsettings.json)
# - IOTHUB_CONNECTION_STRING (votre IoT Hub brainoffice)
# - STORAGE_CONNECTION_STRING (votre Storage Account)

notepad .env
```

### Étape 3 : Déployer avec azd

```powershell
# Initialiser l'environnement azd
azd init

# Provisionner l'infrastructure + déployer l'application
azd up

# ?? Durée estimée : 5-7 minutes
```

### ?? C'est terminé !

Votre API est maintenant déployée. Vous recevrez :
- ?? L'URL de l'API : `https://smartoffice-prod-api.azurewebsites.net`
- ?? Swagger UI : `https://smartoffice-prod-api.azurewebsites.net/swagger`
- ?? Health Check : `https://smartoffice-prod-api.azurewebsites.net/health/ready`

---

## ?? Configuration détaillée

### Option 1 : Utilisation de variables d'environnement

```powershell
# Définir les secrets en variables d'environnement
$env:AZURE_SUBSCRIPTION_ID = "votre-subscription-id"
$env:POSTGRESQL_ADMIN_PASSWORD = "SmartOffice2025!"
$env:JWT_SECRET = "votre-jwt-secret-256-bits"
$env:IOTHUB_CONNECTION_STRING = "HostName=brainoffice.azure-devices.net;..."
$env:STORAGE_CONNECTION_STRING = "DefaultEndpointsProtocol=https;..."

# Déployer
azd up
```

### Option 2 : Déploiement interactif

```powershell
# azd vous demandera interactivement chaque secret
azd up

# Suivre les prompts :
# ? Enter a value for 'postgresqlAdminPassword' parameter: ********
# ? Enter a value for 'jwtSecret' parameter: ********
# ...
```

### Option 3 : Déploiement en 2 étapes (provision + deploy)

```powershell
# 1. Provisionner uniquement l'infrastructure
azd provision

# 2. Déployer uniquement le code (si infra déjà créée)
azd deploy
```

---

## ?? Déploiement CI/CD avec GitHub Actions

### Configuration GitHub Actions

Pour automatiser le déploiement à chaque push sur la branche `main` :

#### 1. Créer le workflow GitHub Actions

```bash
mkdir -p .github/workflows
```

Créer le fichier `.github/workflows/azure-deploy.yml` :

```yaml
name: ?? Deploy SmartOffice API to Azure

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: smartoffice-prod-api
  DOTNET_VERSION: '9.0.x'

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - name: ?? Checkout code
      uses: actions/checkout@v4
    
    - name: ?? Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: ?? Restore dependencies
      run: dotnet restore SmartOfficeAPP/SmartOfficeAPP.csproj
    
    - name: ??? Build
      run: dotnet build SmartOfficeAPP/SmartOfficeAPP.csproj --configuration Release --no-restore
    
    - name: ?? Test
      run: dotnet test SmartOfficeAPP/SmartOfficeAPP.csproj --no-restore --verbosity normal
    
    - name: ?? Publish
      run: dotnet publish SmartOfficeAPP/SmartOfficeAPP.csproj --configuration Release --output ./publish
    
    - name: ?? Upload artifact
      uses: actions/upload-artifact@v4
      with:
        name: smartoffice-api
        path: ./publish

  deploy:
    runs-on: ubuntu-latest
    needs: build
    environment:
      name: 'Production'
      url: ${{ steps.deploy-to-webapp.outputs.webapp-url }}
    
    steps:
    - name: ?? Download artifact
      uses: actions/download-artifact@v4
      with:
        name: smartoffice-api
    
    - name: ?? Login to Azure
      uses: azure/login@v2
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: ?? Deploy to Azure App Service
      id: deploy-to-webapp
      uses: azure/webapps-deploy@v3
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        package:.
    
    - name: ?? Run EF migrations
      run: |
        dotnet tool install --global dotnet-ef --version 9.*
        cd SmartOfficeAPP
        dotnet ef database update --connection "${{ secrets.POSTGRESQL_CONNECTION_STRING }}"
    
    - name: ? Logout from Azure
      run: az logout
```

#### 2. Configurer les secrets GitHub

Créer les secrets GitHub suivants dans votre repository :

##### a. Obtenir les credentials Azure (Service Principal)

```powershell
# Créer un Service Principal avec droits Contributor
az ad sp create-for-rbac `
  --name "github-actions-smartoffice" `
  --role contributor `
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/smartoffice-prod-rg `
  --sdk-auth

# Copier la sortie JSON complète
```

##### b. Ajouter les secrets dans GitHub

1. Aller sur GitHub : **Settings** ? **Secrets and variables** ? **Actions**
2. Créer les secrets suivants :

| Secret Name | Description | Exemple |
|-------------|-------------|---------|
| `AZURE_CREDENTIALS` | Output JSON du Service Principal | `{"clientId":"...","clientSecret":"...","subscriptionId":"...","tenantId":"..."}` |
| `POSTGRESQL_CONNECTION_STRING` | Connection string PostgreSQL | `Host=smartoffice-prod-postgres.postgres.database.azure.com;Database=smartofficedb;Username=smartofficeadmin;Password=...;SSL Mode=Require` |

#### 3. Tester le workflow

```bash
# Commit et push
git add .github/workflows/azure-deploy.yml
git commit -m "Add GitHub Actions CI/CD workflow"
git push origin main

# Le workflow se déclenchera automatiquement
```

Voir l'exécution du workflow dans GitHub : **Actions** tab

### Avantages du CI/CD

? **Déploiement automatique** : chaque push sur `main` déclenche un déploiement  
? **Tests automatisés** : validation avant déploiement  
? **Rollback facile** : retour à une version précédente  
? **Historique des déploiements** : traçabilité complète  
? **Zéro downtime** : déploiement sans interruption

### Alternative : Azure DevOps Pipelines

Si vous préférez Azure DevOps :

```yaml
# azure-pipelines.yml
trigger:
  branches:
    include:
    - main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  azureSubscription: 'SmartOffice-ServiceConnection'
  webAppName: 'smartoffice-prod-api'

stages:
- stage: Build
  jobs:
  - job: Build
    steps:
    - task: UseDotNet@2
      inputs:
        version: '9.0.x'
    
    - task: DotNetCoreCLI@2
      displayName: 'Restore'
      inputs:
        command: 'restore'
        projects: 'SmartOfficeAPP/SmartOfficeAPP.csproj'
    
    - task: DotNetCoreCLI@2
      displayName: 'Build'
      inputs:
        command: 'build'
        projects: 'SmartOfficeAPP/SmartOfficeAPP.csproj'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      displayName: 'Publish'
      inputs:
        command: 'publish'
        publishWebProjects: false
        projects: 'SmartOfficeAPP/SmartOfficeAPP.csproj'
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
    
    - task: PublishBuildArtifacts@1
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)'
        ArtifactName: 'drop'

- stage: Deploy
  dependsOn: Build
  jobs:
  - deployment: Deploy
    environment: 'Production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: '$(azureSubscription)'
              appName: '$(webAppName)'
              package: '$(Pipeline.Workspace)/drop/*.zip'
```

---

## ??? Commandes utiles

### Gestion du déploiement

```powershell
# Voir l'état de l'environnement
azd env list

# Afficher les outputs Bicep (URLs, noms de ressources)
azd env get-values

# Ouvrir Application Insights (monitoring)
azd monitor

# Voir les logs en temps réel
azd monitor --logs

# Redéployer uniquement le code (sans toucher infra)
azd deploy

# Supprimer toute l'infrastructure (APRÈS LA SOUTENANCE)
azd down
```

### Gestion de la base de données

```powershell
# Se connecter à PostgreSQL depuis votre machine
$postgresHost = azd env get-value AZURE_POSTGRESQL_FQDN
psql "host=$postgresHost dbname=smartofficedb user=smartofficeadmin sslmode=require"

# Exécuter les migrations manuellement
cd SmartOfficeAPP
dotnet ef database update --connection "Host=$postgresHost;Database=smartofficedb;Username=smartofficeadmin;Password=VotrePassword;SSL Mode=Require"

# Créer une nouvelle migration
dotnet ef migrations add MigrationName
```

### Gestion des secrets (Key Vault)

```powershell
# Lister les secrets
$kvName = azd env get-value AZURE_KEY_VAULT_NAME
az keyvault secret list --vault-name $kvName

# Récupérer un secret
az keyvault secret show --vault-name $kvName --name "JWT-Secret"

# Ajouter/modifier un secret
az keyvault secret set --vault-name $kvName --name "NewSecret" --value "SecretValue"
```

### Monitoring et diagnostic

```powershell
# Voir les logs de l'App Service
$webAppName = azd env get-value AZURE_WEBAPP_NAME
$resourceGroup = azd env get-value AZURE_RESOURCE_GROUP
az webapp log tail --name $webAppName --resource-group $resourceGroup

# Télécharger les logs
az webapp log download --name $webAppName --resource-group $resourceGroup

# Ouvrir le portail Azure sur votre Resource Group
azd monitor --portal
```

---

## ?? Dépannage

### Problème : Échec de déploiement "PostgreSQL password requirements"

**Solution** : Le mot de passe doit contenir :
- Minimum 8 caractères
- Au moins 1 majuscule
- Au moins 1 minuscule
- Au moins 1 chiffre
- Caractères spéciaux autorisés : `!@#$%^&*`

```powershell
# Exemple valide
$env:POSTGRESQL_ADMIN_PASSWORD = "SmartOffice2025!"
```

### Problème : "Key Vault access denied"

**Solution** : La Managed Identity met ~30 secondes à se propager. Attendez puis :

```powershell
# Redéployer
azd deploy
```

### Problème : "Failed to connect to PostgreSQL"

**Solution** : Vérifier les règles firewall :

```powershell
$resourceGroup = azd env get-value AZURE_RESOURCE_GROUP
$serverName = azd env get-value AZURE_POSTGRESQL_SERVER

# Ajouter votre IP
$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
az postgres flexible-server firewall-rule create `
  --resource-group $resourceGroup `
  --name $serverName `
  --rule-name "MyLocalMachine" `
  --start-ip-address $myIp `
  --end-ip-address $myIp
```

### Problème : "Health check failing"

**Diagnostic** :

```powershell
# Tester le health check
$webAppUrl = azd env get-value AZURE_WEBAPP_URL
Invoke-WebRequest -Uri "$webAppUrl/health/ready"

# Voir les logs détaillés
az webapp log tail --name $webAppName --resource-group $resourceGroup
```

### Problème : IoT Hub connection issues

**Solution** : Vérifier la connection string dans Key Vault :

```powershell
$kvName = azd env get-value AZURE_KEY_VAULT_NAME
az keyvault secret show --vault-name $kvName --name "IotHub-ConnectionString"

# Si vide ou incorrecte, mettre à jour
az keyvault secret set `
  --vault-name $kvName `
  --name "IotHub-ConnectionString" `
  --value "HostName=brainoffice.azure-devices.net;SharedAccessKeyName=...;SharedAccessKey=..."
```

---

## ?? Points importants pour la soutenance

### Architecture et choix techniques

**Pourquoi Infrastructure as Code (Bicep) ?**
- ? Reproductibilité : même infrastructure à chaque déploiement
- ? Versioning : infrastructure dans Git
- ? Documentation : le code décrit l'architecture
- ? Best practices : sécurité intégrée (Key Vault, HTTPS, RBAC)

**Pourquoi Azure App Service ?**
- ? PaaS managé : pas de gestion de VM
- ? Auto-scaling : adaptation à la charge
- ? Déploiement continu : intégration CI/CD
- ? Monitoring intégré : Application Insights

**Pourquoi PostgreSQL Flexible Server ?**
- ? Compatibilité avec votre développement local
- ? Flexibilité : auto-scaling du storage
- ? Backups automatiques
- ? Haute disponibilité possible (désactivée pour coût)

**Pourquoi Redis Cache ?**
- ? Performance : réduction latence API
- ? Scalabilité : décharge la base de données
- ? Cache distribué : support multi-instances

### Démonstration live

**Test de santé** :
```powershell
# Health check (doit retourner 200 OK)
curl https://smartoffice-prod-api.azurewebsites.net/health/ready
```

**Swagger UI** :
```
https://smartoffice-prod-api.azurewebsites.net/swagger
```

**Monitoring en temps réel** :
```powershell
azd monitor
# Montrer Application Insights : requêtes, performances, exceptions
```

### Métriques clés à présenter

```powershell
# Obtenir les métriques App Service
$webAppName = azd env get-value AZURE_WEBAPP_NAME
$resourceGroup = azd env get-value AZURE_RESOURCE_GROUP

az monitor metrics list `
  --resource "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$resourceGroup/providers/Microsoft.Web/sites/$webAppName" `
  --metric "CpuPercentage" "MemoryPercentage" "HttpResponseTime" `
  --interval PT1M
```

### Sécurité mise en œuvre

1. **Secrets management** : Azure Key Vault avec RBAC
2. **Identité managée** : pas de credentials en dur
3. **HTTPS obligatoire** : certificat SSL automatique
4. **Firewall PostgreSQL** : règles d'accès restreintes
5. **Redis SSL** : connexions chiffrées
6. **Minimum TLS 1.2** : sur tous les services

### Coûts et optimisation

**Coût actuel** : ~123 USD/mois  
**Optimisation possible** :
- Arrêter les services hors démo : **0 USD**
- Passer en Dev/Test SKU : ~50 USD/mois
- Activer auto-shutdown : économies automatiques

```powershell
# Arrêter l'App Service après la soutenance
az webapp stop --name $webAppName --resource-group $resourceGroup

# Redémarrer avant une démo
az webapp start --name $webAppName --resource-group $resourceGroup
```

---

## ?? Support

### Ressources utiles

- [Documentation Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [PostgreSQL Flexible Server](https://learn.microsoft.com/azure/postgresql/flexible-server/)

### Commandes de diagnostic rapide

```powershell
# Statut global
azd env list

# Logs en temps réel
azd monitor --logs

# Ouvrir le portail Azure
azd monitor --portal

# Nettoyer tout (APRÈS LA SOUTENANCE)
azd down --force --purge
```

---

## ?? Félicitations !

Votre API SmartOffice est maintenant déployée sur Azure avec une architecture production-ready. 

**Prochaines étapes** :
1. ? Tester l'API via Swagger
2. ? Vérifier les health checks
3. ? Configurer votre frontend pour pointer vers l'URL Azure
4. ? Préparer des captures d'écran du portail Azure pour votre soutenance
5. ? Tester les fonctionnalités IoT avec vos devices

**Bon courage pour votre soutenance ! ????**
