# ?? Guide Complet CI/CD avec GitHub Actions

## ?? Table des Matières

1. [Architecture CI/CD](#architecture-cicd)
2. [Configuration des Secrets GitHub](#configuration-des-secrets-github)
3. [Première Exécution](#première-exécution)
4. [Workflow Expliqué](#workflow-expliqué)
5. [Dépannage](#dépannage)
6. [Commandes Utiles](#commandes-utiles)

---

## ??? Architecture CI/CD

### Pipeline en 3 étapes :

```
???????????????????????????????????????????????????????????????
?                    GitHub Repository                          ?
?                         (main)                                ?
???????????????????????????????????????????????????????????????
                        ? git push
                        ?
???????????????????????????????????????????????????????????????
?              GitHub Actions Workflow                          ?
?                                                               ?
?  ????????????????????????????????????????????????            ?
?  ?  Job 1: Build & Test                         ?            ?
?  ?  ? Checkout code                            ?            ?
?  ?  ? Setup .NET 9                             ?            ?
?  ?  ? Restore dependencies                     ?            ?
?  ?  ? Build application                        ?            ?
?  ?  ? Run tests                                ?            ?
?  ?  ? Publish artifact                         ?            ?
?  ????????????????????????????????????????????????            ?
?                        ?                                      ?
?                        ?                                      ?
?  ????????????????????????????????????????????????            ?
?  ?  Job 2: Deploy to Azure                     ?            ?
?  ?  ? Download artifact                        ?            ?
?  ?  ? Azure Login (OIDC)                       ?            ?
?  ?  ? Deploy to App Service                    ?            ?
?  ?  ? Run EF migrations                        ?            ?
?  ?  ? Health check                             ?            ?
?  ????????????????????????????????????????????????            ?
?                        ?                                      ?
?                        ?                                      ?
?  ????????????????????????????????????????????????            ?
?  ?  Job 3: Notification                         ?            ?
?  ?  ? Deployment summary                       ?            ?
?  ????????????????????????????????????????????????            ?
???????????????????????????????????????????????????????????????
                        ?
                        ?
???????????????????????????????????????????????????????????????
?                    Azure App Service                          ?
?              smartoffice-prod-api (East US)                   ?
?                                                               ?
?  ?? URL: https://smartoffice-prod-api.azurewebsites.net     ?
?  ?? Swagger: /swagger                                        ?
?  ?? Health: /health/ready                                   ?
???????????????????????????????????????????????????????????????
```

### Déclencheurs du Pipeline :

- ? **Push sur `main`** : Déploiement automatique
- ? **Pull Request** : Build + Tests uniquement (pas de déploiement)
- ? **Manuel** : Via GitHub Actions UI (`workflow_dispatch`)

---

## ?? Configuration des Secrets GitHub

### Étape 1 : Créer un Service Principal Azure

Un **Service Principal** permet à GitHub Actions de s'authentifier sur Azure de manière sécurisée avec **OpenID Connect (OIDC)** (pas de secrets stockés !).

```powershell
# Se connecter à Azure
az login

# Vérifier l'abonnement actif
az account show

# Obtenir votre Subscription ID
$subscriptionId = az account show --query id -o tsv
Write-Host "Subscription ID: $subscriptionId" -ForegroundColor Green

# Obtenir votre Tenant ID
$tenantId = az account show --query tenantId -o tsv
Write-Host "Tenant ID: $tenantId" -ForegroundColor Green
```

### Étape 2 : Créer le Service Principal avec OIDC

```powershell
# Nom de votre application GitHub (format: username/repo)
$githubRepo = "Koj-Kyo/api-service-smart-office"

# Créer le Service Principal avec Federated Credentials (OIDC)
$appName = "github-actions-smartoffice"
$resourceGroup = "smartoffice-prod-rg"

# Créer l'App Registration
$appId = az ad app create --display-name $appName --query appId -o tsv
Write-Host "App ID (Client ID): $appId" -ForegroundColor Green

# Créer le Service Principal
az ad sp create --id $appId

# Obtenir l'Object ID du Service Principal
$spObjectId = az ad sp show --id $appId --query id -o tsv

# Assigner le rôle Contributor sur le Resource Group
az role assignment create `
  --assignee $appId `
  --role Contributor `
  --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup"

Write-Host "? Service Principal créé avec succès !" -ForegroundColor Green
```

### Étape 3 : Configurer Federated Credentials (OIDC)

```powershell
# Créer Federated Credential pour la branche main
az ad app federated-credential create `
  --id $appId `
  --parameters '{
    "name": "github-main-branch",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"$githubRepo"':ref:refs/heads/main",
    "description": "GitHub Actions for main branch",
    "audiences": ["api://AzureADTokenExchange"]
  }'

# Créer Federated Credential pour les Pull Requests
az ad app federated-credential create `
  --id $appId `
  --parameters '{
    "name": "github-pull-requests",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'"$githubRepo"':pull_request",
    "description": "GitHub Actions for pull requests",
    "audiences": ["api://AzureADTokenExchange"]
  }'

Write-Host "? Federated Credentials configurées !" -ForegroundColor Green
```

### Étape 4 : Obtenir la Connection String PostgreSQL

```powershell
# Récupérer les informations PostgreSQL depuis azd (si vous avez déjà déployé)
$postgresHost = azd env get-value AZURE_POSTGRESQL_FQDN
$postgresDb = "smartofficedb"
$postgresUser = "smartofficeadmin"
$postgresPassword = Read-Host -Prompt "Entrez le mot de passe PostgreSQL" -AsSecureString
$postgresPasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($postgresPassword)
)

# Construire la connection string
$connectionString = "Host=$postgresHost;Database=$postgresDb;Username=$postgresUser;Password=$postgresPasswordPlain;SSL Mode=Require"

Write-Host "? Connection string générée" -ForegroundColor Green
```

### Étape 5 : Ajouter les Secrets dans GitHub

#### Option A : Via l'interface GitHub (Recommandé)

1. Aller sur votre repository GitHub : `https://github.com/Koj-Kyo/api-service-smart-office`

2. Cliquer sur **Settings** ? **Secrets and variables** ? **Actions**

3. Cliquer sur **New repository secret**

4. Créer les secrets suivants :

| Secret Name | Valeur | Description |
|-------------|--------|-------------|
| `AZURE_CLIENT_ID` | `$appId` | Client ID du Service Principal |
| `AZURE_TENANT_ID` | `$tenantId` | Tenant ID Azure |
| `AZURE_SUBSCRIPTION_ID` | `$subscriptionId` | Subscription ID Azure |
| `POSTGRESQL_CONNECTION_STRING` | `$connectionString` | Connection string PostgreSQL |

#### Option B : Via GitHub CLI

```powershell
# Installer GitHub CLI si pas déjà fait
# https://cli.github.com/

# Se connecter à GitHub
gh auth login

# Ajouter les secrets
gh secret set AZURE_CLIENT_ID -b"$appId" --repo Koj-Kyo/api-service-smart-office
gh secret set AZURE_TENANT_ID -b"$tenantId" --repo Koj-Kyo/api-service-smart-office
gh secret set AZURE_SUBSCRIPTION_ID -b"$subscriptionId" --repo Koj-Kyo/api-service-smart-office
gh secret set POSTGRESQL_CONNECTION_STRING -b"$connectionString" --repo Koj-Kyo/api-service-smart-office

Write-Host "? Secrets GitHub configurés !" -ForegroundColor Green
```

### Étape 6 : Vérifier les Secrets

```powershell
# Lister les secrets (sans afficher les valeurs)
gh secret list --repo Koj-Kyo/api-service-smart-office
```

Vous devriez voir :
```
AZURE_CLIENT_ID              Updated 2025-11-08
AZURE_SUBSCRIPTION_ID        Updated 2025-11-08
AZURE_TENANT_ID              Updated 2025-11-08
POSTGRESQL_CONNECTION_STRING Updated 2025-11-08
```

---

## ?? Première Exécution

### Déclencher le Pipeline

#### Option 1 : Push sur main (Automatique)

```bash
# Commit et push vos changements
git add .
git commit -m "feat: Add CI/CD pipeline with GitHub Actions"
git push origin main
```

#### Option 2 : Déclenchement Manuel

1. Aller sur GitHub : **Actions** tab
2. Sélectionner le workflow **"?? Deploy SmartOffice API to Azure"**
3. Cliquer sur **Run workflow** ? **Run workflow**

### Suivre l'Exécution

1. Aller sur GitHub : **Actions** tab
2. Cliquer sur le workflow en cours d'exécution
3. Voir les logs en temps réel pour chaque job

### Résultat Attendu

Après ~5-7 minutes :
- ? Job "Build & Test" : SUCCESS
- ? Job "Deploy to Azure" : SUCCESS
- ? Job "Notify" : SUCCESS

Vous verrez dans les logs :
```
???????????????????????????????????????????????????
?? Déploiement réussi !
???????????????????????????????????????????????????
?? URL API       : https://smartoffice-prod-api.azurewebsites.net
?? Swagger UI    : https://smartoffice-prod-api.azurewebsites.net/swagger
?? Health Check  : https://smartoffice-prod-api.azurewebsites.net/health/ready
?? Azure Portal  : https://portal.azure.com/#@/resource/...
???????????????????????????????????????????????????
```

---

## ?? Workflow Expliqué

### Job 1 : Build & Test

```yaml
- Checkout code       # Récupère le code depuis GitHub
- Setup .NET 9        # Installe .NET 9 SDK
- Restore deps        # Télécharge les packages NuGet
- Build               # Compile l'application
- Run Tests           # Exécute les tests unitaires
- Publish             # Crée le package de déploiement
- Upload artifact     # Sauvegarde le package pour le job suivant
```

### Job 2 : Deploy to Azure

```yaml
- Download artifact   # Récupère le package du job 1
- Azure Login (OIDC)  # Authentification sécurisée sans secrets
- Deploy to App Svc   # Déploie sur Azure App Service
- EF Migrations       # Applique les migrations de base de données
- Health Check        # Vérifie que l'API répond
```

### Job 3 : Notification

```yaml
- Deployment Status   # Affiche un résumé du déploiement
```

---

## ?? Dépannage

### Problème : "Failed to authenticate with Azure"

**Cause** : Federated Credentials mal configurées ou Service Principal sans permissions.

**Solution** :

```powershell
# Vérifier les Federated Credentials
az ad app federated-credential list --id $appId

# Vérifier les permissions
az role assignment list --assignee $appId --output table

# Recréer si nécessaire
az ad app federated-credential create --id $appId --parameters '...'
```

### Problème : "Health check failed (HTTP 000)"

**Cause** : L'application n'a pas démarré ou met du temps à démarrer.

**Solution** :

1. Augmenter le délai d'attente dans le workflow (ligne `sleep 30` ? `sleep 60`)
2. Vérifier les logs dans Azure Portal :
   ```powershell
   az webapp log tail --name smartoffice-prod-api --resource-group smartoffice-prod-rg
   ```

### Problème : "EF Migrations failed"

**Cause** : Connection string incorrecte ou base de données inaccessible.

**Solution** :

1. Vérifier le secret `POSTGRESQL_CONNECTION_STRING` dans GitHub
2. Tester la connection string localement :
   ```powershell
   psql "$connectionString"
   ```
3. Vérifier les règles firewall PostgreSQL :
   ```powershell
   az postgres flexible-server firewall-rule list `
     --name smartoffice-prod-postgres `
     --resource-group smartoffice-prod-rg
   ```

### Problème : "Build failed: Package not found"

**Cause** : Dépendances NuGet manquantes ou version .NET incorrecte.

**Solution** :

1. Vérifier `SmartOfficeAPP.csproj` : `<TargetFramework>net9.0</TargetFramework>`
2. Nettoyer et rebuild :
   ```powershell
   dotnet clean
   dotnet restore
   dotnet build
   ```

### Problème : "Artifact not found in deploy job"

**Cause** : Upload artifact a échoué ou nom incorrect.

**Solution** :

1. Vérifier les logs du job "Build & Test"
2. S'assurer que le chemin `${{github.workspace}}/publish` contient bien les fichiers

---

## ?? Commandes Utiles

### GitHub Actions

```bash
# Voir les workflows
gh workflow list --repo Koj-Kyo/api-service-smart-office

# Voir les runs d'un workflow
gh run list --workflow=azure-deploy.yml --repo Koj-Kyo/api-service-smart-office

# Voir les logs d'un run
gh run view <run-id> --log --repo Koj-Kyo/api-service-smart-office

# Relancer un workflow
gh workflow run azure-deploy.yml --repo Koj-Kyo/api-service-smart-office
```

### Azure CLI

```powershell
# Voir les déploiements App Service
az webapp deployment list --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# Logs en temps réel
az webapp log tail --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# Redémarrer l'app
az webapp restart --name smartoffice-prod-api --resource-group smartoffice-prod-rg
```

### Tests

```bash
# Tester l'API déployée
curl https://smartoffice-prod-api.azurewebsites.net/health/ready

# Tester Swagger
curl https://smartoffice-prod-api.azurewebsites.net/swagger/index.html
```

---

## ?? Pour la Soutenance

### Badge GitHub Actions

Ajoutez ce badge dans votre README pour montrer le statut du pipeline :

```markdown
[![Deploy to Azure](https://github.com/Koj-Kyo/api-service-smart-office/actions/workflows/azure-deploy.yml/badge.svg)](https://github.com/Koj-Kyo/api-service-smart-office/actions/workflows/azure-deploy.yml)
```

### Démonstration Live

1. **Montrer le workflow dans GitHub Actions**
   - Aller sur : https://github.com/Koj-Kyo/api-service-smart-office/actions

2. **Déclencher un déploiement en direct**
   ```bash
   # Faire un petit changement
   echo "// CI/CD Demo" >> SmartOfficeAPP/Program.cs
   git add .
   git commit -m "demo: CI/CD pipeline live"
   git push origin main
   ```

3. **Montrer les logs en temps réel**
   - Ouvrir le workflow en cours
   - Montrer chaque job qui s'exécute

4. **Vérifier le déploiement**
   ```bash
   curl https://smartoffice-prod-api.azurewebsites.net/health/ready
   ```

### Points Clés à Expliquer

? **CI/CD Automatisé** : Push ? Build ? Test ? Deploy  
? **Sécurité OIDC** : Pas de secrets stockés, authentification fédérée  
? **Tests Automatiques** : Validation avant déploiement  
? **Migrations Auto** : Base de données mise à jour automatiquement  
? **Health Checks** : Validation post-déploiement  
? **Rollback** : Via GitHub Actions re-run d'un commit précédent  

---

## ?? Ressources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure OIDC with GitHub](https://learn.microsoft.com/azure/developer/github/connect-from-azure)
- [Azure App Service Deploy Action](https://github.com/Azure/webapps-deploy)
- [Entity Framework Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)

---

## ? Checklist Finale

Avant de committer le workflow :

- [ ] Service Principal créé
- [ ] Federated Credentials configurées
- [ ] Secrets GitHub ajoutés (4 secrets)
- [ ] Workflow YAML créé dans `.github/workflows/`
- [ ] Infrastructure Azure déployée (azd up)
- [ ] Test local réussi (dotnet build)

Après le premier push :

- [ ] Workflow exécuté avec succès
- [ ] Health check OK
- [ ] Swagger accessible
- [ ] Migrations appliquées
- [ ] Logs vérifiés dans Azure

---

**?? Votre pipeline CI/CD est prêt !**

À chaque push sur `main`, votre API sera automatiquement déployée sur Azure. ??
