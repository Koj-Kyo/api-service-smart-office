# ?? Guide de Déploiement CI/CD Azure - SmartOffice API

## ?? Table des matières
1. [Vue d'ensemble](#vue-densemble)
2. [Coûts estimés](#coûts-estimés)
3. [Prérequis Azure](#prérequis-azure)
4. [Configuration GitHub](#configuration-github)
5. [Déploiement étape par étape](#déploiement-étape-par-étape)
6. [Vérification](#vérification)
7. [Maintenance et coûts](#maintenance-et-coûts)
8. [Dépannage](#dépannage)

---

## ?? Vue d'ensemble

Ce guide vous permet de déployer SmartOfficeAPP sur Azure avec **GitHub Actions** en **minimisant les coûts** pour une soutenance académique.

### Architecture déployée

```
???????????????????????????????????????????????????????????
?                    AZURE CLOUD                          ?
???????????????????????????????????????????????????????????
?                                                         ?
?  ???????????????????????????????????????????           ?
?  ?  App Service Plan (B1)                  ?           ?
?  ?  - Linux                                ?           ?
?  ?  - .NET 9                               ?           ?
?  ?  - 1 vCore, 1.75 GB RAM                ?           ?
?  ?  - ~13 USD/mois                         ?           ?
?  ???????????????????????????????????????????           ?
?         ?                                               ?
?         ?                                               ?
?  ???????????????????????????????????????????           ?
?  ?  SmartOffice Web API                    ?           ?
?  ?  - HTTPS uniquement                     ?           ?
?  ?  - Health checks                        ?           ?
?  ?  - Swagger UI                           ?           ?
?  ???????????????????????????????????????????           ?
?                                                         ?
???????????????????????????????????????????????????????????
         ?                     ?
         ?                     ?
         ?                     ?
???????????????      ???????????????????
?  IoT Hub    ?      ?  Storage Account?
?  (Existant) ?      ?  (Existant)     ?
???????????????      ???????????????????
         ?
         ?
???????????????????
?  PostgreSQL     ?
?  (Externe)      ?
???????????????????
```

### Optimisations de coûts appliquées

| Ressource | Choix économique | Coût mensuel |
|-----------|------------------|--------------|
| **App Service Plan** | B1 (Basic) au lieu de S1 (Standard) | ~13 USD |
| **PostgreSQL** | Externe (déjà existant) | 0 USD |
| **Redis** | Cache mémoire au lieu de Redis | 0 USD |
| **Key Vault** | Secrets dans GitHub | 0 USD |
| **Application Insights** | Logs console | 0 USD |
| **Always On** | Désactivé (cold start acceptable) | 0 USD |
| **TOTAL** | | **~13 USD/mois** |

---

## ?? Coûts estimés

### Coût mensuel détaillé

```
?????????????????????????????????????????????????????????
?  RESSOURCE              ?  TIER    ?  COÛT MENSUEL   ?
?????????????????????????????????????????????????????????
?  App Service Plan       ?  B1      ?  ~13.00 USD    ?
?  Bandwidth (sortie)     ?  -       ?  ~0.50 USD     ?
?  Storage (logs)         ?  -       ?  ~0.10 USD     ?
????????????????????????????????????????????????????????
?  TOTAL MENSUEL                     ?  ~13.60 USD    ?
?????????????????????????????????????????????????????????
```

### Ressources RÉUTILISÉES (déjà créées)

Ces ressources existent déjà et NE GÉNÈRENT PAS de coûts supplémentaires :

- ? **IoT Hub** : déjà créé (~25 USD/mois)
- ? **Storage Account** : déjà créé (~0.50 USD/mois)
- ? **PostgreSQL** : local ou externe (0 USD ou prix actuel)

**Coût TOTAL du nouveau déploiement : ~13.60 USD/mois** ??

---

## ?? Prérequis Azure

### 1?? Créer un Resource Group

```bash
# Se connecter à Azure
az login

# Créer le resource group
az group create --name smartoffice-prod-rg --location eastus
```

### 2?? Créer un Service Principal pour GitHub Actions

GitHub Actions a besoin d'un Service Principal pour se connecter à Azure.

```bash
# Récupérer votre Subscription ID
az account show --query id --output tsv

# Créer le Service Principal avec Federated Identity
az ad app create --display-name "GitHub-Actions-SmartOffice"

# Récupérer l'Application (Client) ID
APP_ID=$(az ad app list --display-name "GitHub-Actions-SmartOffice" --query "[0].appId" -o tsv)
echo "Application (Client) ID: $APP_ID"

# Créer le Service Principal
az ad sp create --id $APP_ID

# Obtenir l'Object ID du Service Principal
OBJECT_ID=$(az ad sp show --id $APP_ID --query id -o tsv)
echo "Object ID: $OBJECT_ID"

# Assigner le rôle Contributor sur le Resource Group
SUBSCRIPTION_ID=$(az account show --query id --output tsv)
az role assignment create \
  --role "Contributor" \
  --assignee-object-id $OBJECT_ID \
  --assignee-principal-type "ServicePrincipal" \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/smartoffice-prod-rg"
```

### 3?? Configurer Federated Identity pour GitHub

```bash
# Remplacer par vos valeurs
GITHUB_ORG="Koj-Kyo"
GITHUB_REPO="api-service-smart-office"

# Créer le Federated Credential
az ad app federated-credential create \
  --id $APP_ID \
  --parameters '{
    "name": "GitHub-Actions-SmartOffice-Main",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:'$GITHUB_ORG'/'$GITHUB_REPO':ref:refs/heads/main",
    "audiences": ["api://AzureADTokenExchange"]
  }'

echo "? Federated Credential créé pour la branche main"
```

### 4?? Récupérer les informations nécessaires

```bash
# Subscription ID
echo "AZURE_SUBSCRIPTION_ID: $(az account show --query id -o tsv)"

# Tenant ID
echo "AZURE_TENANT_ID: $(az account show --query tenantId -o tsv)"

# Client ID (Application ID)
echo "AZURE_CLIENT_ID: $APP_ID"
```

**?? NOTEZ CES 3 VALEURS** pour GitHub Secrets !

---

## ?? Configuration GitHub

### Étape 1 : Ajouter les secrets GitHub

Allez dans votre repository GitHub :
```
Settings ? Secrets and variables ? Actions ? New repository secret
```

#### Secrets Azure (obligatoires)

| Nom du secret | Description | Exemple de valeur |
|---------------|-------------|-------------------|
| `AZURE_CLIENT_ID` | Application (Client) ID du Service Principal | `12345678-abcd-efgh-ijkl-123456789012` |
| `AZURE_TENANT_ID` | Tenant ID Azure | `87654321-dcba-hgfe-lkji-210987654321` |
| `AZURE_SUBSCRIPTION_ID` | Subscription ID | `abcdef12-3456-7890-abcd-ef1234567890` |

#### Secrets Application (obligatoires)

| Nom du secret | Description | Où le trouver |
|---------------|-------------|---------------|
| `JWT_SECRET` | Secret JWT | Déjà dans vos User Secrets |
| `IOT_HUB_CONNECTION_STRING` | Connection string IoT Hub | Azure Portal ? IoT Hub ? Shared access policies ? iothubowner |
| `IOT_HUB_EVENTHUB_CONNECTION_STRING` | Connection string Event Hub | Azure Portal ? IoT Hub ? Built-in endpoints |
| `STORAGE_CONNECTION_STRING` | Connection string Storage Account | Azure Portal ? Storage Account ? Access keys |
| `POSTGRESQL_CONNECTION_STRING` | Connection string PostgreSQL | Format : `Host=xxx;Database=xxx;Username=xxx;Password=xxx;SSL Mode=Require` |

### Étape 2 : Vérifier les secrets

Créez un script PowerShell pour générer les commandes GitHub CLI :

```powershell
# set-github-secrets.ps1
# ?? REMPLACEZ LES VALEURS PAR LES VÔTRES

$REPO = "Koj-Kyo/api-service-smart-office"

# Secrets Azure (récupérés précédemment)
gh secret set AZURE_CLIENT_ID -b "VOTRE_CLIENT_ID" --repo $REPO
gh secret set AZURE_TENANT_ID -b "VOTRE_TENANT_ID" --repo $REPO
gh secret set AZURE_SUBSCRIPTION_ID -b "VOTRE_SUBSCRIPTION_ID" --repo $REPO

# Secrets Application (récupérés depuis vos User Secrets)
gh secret set JWT_SECRET -b "VOTRE_JWT_SECRET" --repo $REPO
gh secret set IOT_HUB_CONNECTION_STRING -b "VOTRE_IOT_HUB_CONNECTION_STRING" --repo $REPO
gh secret set IOT_HUB_EVENTHUB_CONNECTION_STRING -b "VOTRE_EVENT_HUB_CONNECTION_STRING" --repo $REPO
gh secret set STORAGE_CONNECTION_STRING -b "VOTRE_STORAGE_CONNECTION_STRING" --repo $REPO
gh secret set POSTGRESQL_CONNECTION_STRING -b "VOTRE_POSTGRESQL_CONNECTION_STRING" --repo $REPO

Write-Host "? Tous les secrets GitHub sont configurés!" -ForegroundColor Green
```

**Exécution :**

```powershell
# Installer GitHub CLI si nécessaire
winget install GitHub.cli

# Se connecter
gh auth login

# Récupérer les valeurs depuis vos User Secrets
dotnet user-secrets list --project D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP

# Modifier le script avec les vraies valeurs, puis exécuter
.\set-github-secrets.ps1
```

---

## ?? Déploiement étape par étape

### Étape 1 : Commit et Push

```bash
cd D:\Memoire\ApiSmart\SmartOfficeAPP

# Vérifier les fichiers créés
git status

# Ajouter les nouveaux fichiers
git add .github/workflows/deploy-azure-minimal.yml
git add infra/main-minimal.bicep
git add infra/main-minimal.parameters.bicepparam

# Commit
git commit -m "feat: Add minimal Azure deployment with GitHub Actions"

# Push vers GitHub
git push origin main
```

### Étape 2 : Vérifier le déploiement

1. **Allez sur GitHub** : https://github.com/Koj-Kyo/api-service-smart-office
2. **Cliquez sur "Actions"**
3. Vous verrez le workflow **"?? Deploy SmartOffice API to Azure (Optimized)"** en cours

### Étape 3 : Surveiller les jobs

Le workflow exécute 4 jobs dans l'ordre :

```
1?? Build .NET Application     (~2-3 minutes)
    ?? Compilation et publication de l'app

2?? Deploy Infrastructure      (~3-5 minutes)
    ?? Création App Service avec Bicep

3?? Deploy Application         (~2-3 minutes)
    ?? Déploiement du code sur App Service

4?? Health Check              (~30 secondes)
    ?? Vérification /health/ready et /swagger
```

**Durée totale : ~8-12 minutes** ??

### Étape 4 : Récupérer l'URL de l'API

À la fin du déploiement, vous verrez :

```
========================================
?? DÉPLOIEMENT RÉUSSI!
========================================

?? API URL        : https://smartoffice-prod-api.azurewebsites.net
?? Swagger UI     : https://smartoffice-prod-api.azurewebsites.net/swagger
?? Health Check   : https://smartoffice-prod-api.azurewebsites.net/health/ready
?? Resource Group : smartoffice-prod-rg

========================================
```

---

## ? Vérification

### Test 1 : Health Check

```bash
curl https://smartoffice-prod-api.azurewebsites.net/health/ready
```

**Résultat attendu :**
```
Healthy
```

### Test 2 : Swagger UI

Ouvrez dans le navigateur :
```
https://smartoffice-prod-api.azurewebsites.net/swagger
```

### Test 3 : Authentification API

```bash
curl -X POST https://smartoffice-prod-api.azurewebsites.net/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "adminer@smartoffice.com",
    "password": "Admin1234!"
  }'
```

**Résultat attendu :**
```json
{
  "token": "eyJhbGc...",
  "email": "adminer@smartoffice.com",
  "userName": "admin"
}
```

### Test 4 : Depuis l'application mobile MAUI

Mettez à jour l'URL dans votre app mobile :

```csharp
// Dans ApiService.cs ou MauiProgram.cs
private const string BaseUrl = "https://smartoffice-prod-api.azurewebsites.net";
```

---

## ?? Maintenance et coûts

### Arrêter l'App Service (économiser)

Si vous n'utilisez pas l'application pendant un moment :

```bash
# Arrêter l'App Service (0 coût)
az webapp stop --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# Redémarrer quand nécessaire
az webapp start --name smartoffice-prod-api --resource-group smartoffice-prod-rg
```

### Surveillance des coûts

```bash
# Voir les coûts actuels
az consumption usage list \
  --start-date 2024-01-01 \
  --end-date 2024-01-31 \
  --query "[].{Service:meterName, Cost:pretaxCost}" \
  --output table
```

### Supprimer complètement (après soutenance)

```bash
# ?? ATTENTION : Supprime TOUTE l'infrastructure
az group delete --name smartoffice-prod-rg --yes --no-wait
```

---

## ?? Dépannage

### Erreur : "The subscription is disabled"

**Cause** : Ancien compte Azure désactivé dans les secrets

**Solution** :
1. Vérifiez que les secrets GitHub utilisent le NOUVEAU compte Azure
2. Re-créez le Service Principal sur le nouveau compte
3. Mettez à jour `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`

### Erreur : "Resource group not found"

**Cause** : Resource group pas créé

**Solution** :
```bash
az group create --name smartoffice-prod-rg --location eastus
```

### Erreur : "Health check failed"

**Cause** : PostgreSQL ou IoT Hub connection strings incorrects

**Solution** :
1. Vérifiez les secrets GitHub
2. Testez localement avec les mêmes connection strings
3. Vérifiez les logs App Service :
   ```bash
   az webapp log tail --name smartoffice-prod-api --resource-group smartoffice-prod-rg
   ```

### Erreur : "Deployment failed: 401 Unauthorized"

**Cause** : Federated Identity mal configurée

**Solution** :
```bash
# Vérifier les Federated Credentials
az ad app federated-credential list --id $APP_ID

# Supprimer et recréer si nécessaire
az ad app federated-credential delete --id $APP_ID --federated-credential-id <ID>
```

---

## ?? Comparaison avec l'ancien déploiement

| Aspect | Ancien (S1 + Redis + PG) | Nouveau (B1 minimal) |
|--------|-------------------------|---------------------|
| **App Service** | S1 (~70 USD/mois) | B1 (~13 USD/mois) |
| **Redis** | C0 (~15 USD/mois) | Cache mémoire (0 USD) |
| **PostgreSQL** | Flexible (~20 USD/mois) | Externe (0 USD) |
| **Key Vault** | Standard (~5 USD/mois) | GitHub Secrets (0 USD) |
| **App Insights** | (~5 USD/mois) | Console logs (0 USD) |
| **TOTAL** | **~115 USD/mois** | **~13 USD/mois** |
| **ÉCONOMIE** | | **~102 USD/mois (88%)** ?? |

---

## ?? Checklist finale

Avant de déclarer le déploiement terminé :

- [ ] ? Service Principal créé avec Federated Identity
- [ ] ? Resource Group créé (`smartoffice-prod-rg`)
- [ ] ? 8 secrets GitHub configurés
- [ ] ? Workflow GitHub Actions passé avec succès
- [ ] ? Health check `/health/ready` retourne "Healthy"
- [ ] ? Swagger UI accessible
- [ ] ? API `/api/Auth/login` fonctionne
- [ ] ? Application mobile connectée à l'API Azure
- [ ] ? Coût mensuel vérifié (~13 USD)

---

## ?? Liens utiles

- **GitHub Repository** : https://github.com/Koj-Kyo/api-service-smart-office
- **Azure Portal** : https://portal.azure.com
- **GitHub Actions Docs** : https://docs.github.com/en/actions
- **Azure App Service Docs** : https://learn.microsoft.com/azure/app-service/
- **Bicep Docs** : https://learn.microsoft.com/azure/azure-resource-manager/bicep/

---

## ?? Support

Si vous rencontrez des problèmes :

1. **Vérifiez les logs GitHub Actions** : onglet "Actions" ? cliquez sur le workflow
2. **Vérifiez les logs App Service** :
   ```bash
   az webapp log tail --name smartoffice-prod-api --resource-group smartoffice-prod-rg
   ```
3. **Testez manuellement l'API** avec les fichiers `.http` du projet

---

**Créé pour SmartOfficeAPP - Déploiement CI/CD Azure Optimisé**  
?? Dernière mise à jour : Janvier 2024  
?? **Coût optimisé : ~13 USD/mois (88% d'économie)**  
?? **Bonne chance pour votre soutenance !**
