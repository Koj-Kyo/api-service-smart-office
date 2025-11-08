# ?? CI/CD GitHub Actions - Guide Express (5 minutes)

## ? Configuration Rapide

### Étape 1 : Créer le Service Principal (2 min)

```powershell
# 1. Se connecter à Azure
az login

# 2. Obtenir les IDs nécessaires
$subscriptionId = az account show --query id -o tsv
$tenantId = az account show --query tenantId -o tsv

Write-Host "Subscription ID: $subscriptionId" -ForegroundColor Green
Write-Host "Tenant ID: $tenantId" -ForegroundColor Green

# 3. Créer le Service Principal
$appName = "github-actions-smartoffice"
$githubRepo = "Koj-Kyo/api-service-smart-office"
$resourceGroup = "smartoffice-prod-rg"

# Créer l'app
$appId = az ad app create --display-name $appName --query appId -o tsv
Write-Host "Client ID: $appId" -ForegroundColor Green

# Créer le Service Principal
az ad sp create --id $appId

# Assigner le rôle
az role assignment create `
  --assignee $appId `
  --role Contributor `
  --scope "/subscriptions/$subscriptionId/resourceGroups/$resourceGroup"

# 4. Configurer OIDC (Federated Credentials)
az ad app federated-credential create `
  --id $appId `
  --parameters "{
    \`"name\`": \`"github-main-branch\`",
    \`"issuer\`": \`"https://token.actions.githubusercontent.com\`",
    \`"subject\`": \`"repo:$githubRepo:ref:refs/heads/main\`",
    \`"description\`": \`"GitHub Actions for main branch\`",
    \`"audiences\`": [\`"api://AzureADTokenExchange\`"]
  }"

Write-Host "? Service Principal créé avec OIDC !" -ForegroundColor Green
```

### Étape 2 : Obtenir la Connection String PostgreSQL (1 min)

```powershell
# Si vous avez déployé avec azd
$postgresHost = azd env get-value AZURE_POSTGRESQL_FQDN

# Sinon, obtenir manuellement
# $postgresHost = "smartoffice-prod-postgres.postgres.database.azure.com"

$postgresDb = "smartofficedb"
$postgresUser = "smartofficeadmin"
$postgresPassword = Read-Host -Prompt "Mot de passe PostgreSQL" -AsSecureString
$postgresPasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($postgresPassword)
)

$connectionString = "Host=$postgresHost;Database=$postgresDb;Username=$postgresUser;Password=$postgresPasswordPlain;SSL Mode=Require"

Write-Host "? Connection string générée" -ForegroundColor Green
```

### Étape 3 : Ajouter les Secrets GitHub (2 min)

#### Option A : Interface GitHub (Recommandé)

1. Aller sur : https://github.com/Koj-Kyo/api-service-smart-office/settings/secrets/actions
2. Cliquer sur **New repository secret**
3. Ajouter ces 4 secrets :

| Secret Name | Valeur (copier depuis PowerShell) |
|-------------|-----------------------------------|
| `AZURE_CLIENT_ID` | `$appId` |
| `AZURE_TENANT_ID` | `$tenantId` |
| `AZURE_SUBSCRIPTION_ID` | `$subscriptionId` |
| `POSTGRESQL_CONNECTION_STRING` | `$connectionString` |

#### Option B : GitHub CLI

```powershell
# Installer gh CLI si nécessaire : https://cli.github.com/

gh auth login

gh secret set AZURE_CLIENT_ID -b"$appId" --repo Koj-Kyo/api-service-smart-office
gh secret set AZURE_TENANT_ID -b"$tenantId" --repo Koj-Kyo/api-service-smart-office
gh secret set AZURE_SUBSCRIPTION_ID -b"$subscriptionId" --repo Koj-Kyo/api-service-smart-office
gh secret set POSTGRESQL_CONNECTION_STRING -b"$connectionString" --repo Koj-Kyo/api-service-smart-office

Write-Host "? Secrets configurés !" -ForegroundColor Green
```

### Étape 4 : Vérifier et Déployer

```bash
# Vérifier les secrets
gh secret list --repo Koj-Kyo/api-service-smart-office

# Commit et push pour déclencher le pipeline
git add .
git commit -m "feat: Add CI/CD pipeline"
git push origin main

# Suivre l'exécution
gh run watch --repo Koj-Kyo/api-service-smart-office
```

---

## ? Résultat

Après ~5-7 minutes :
- ? Build & Test : SUCCESS
- ? Deploy to Azure : SUCCESS
- ?? API accessible : https://smartoffice-prod-api.azurewebsites.net

---

## ?? Test Rapide

```bash
# Health check
curl https://smartoffice-prod-api.azurewebsites.net/health/ready

# Ouvrir Swagger
start https://smartoffice-prod-api.azurewebsites.net/swagger
```

---

## ?? Dépannage Express

### Erreur "Failed to authenticate"
```powershell
# Vérifier les Federated Credentials
az ad app federated-credential list --id $appId

# Vérifier les permissions
az role assignment list --assignee $appId --output table
```

### Erreur "Health check failed"
```powershell
# Voir les logs
az webapp log tail --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# Redémarrer
az webapp restart --name smartoffice-prod-api --resource-group smartoffice-prod-rg
```

### Erreur "Migrations failed"
```powershell
# Tester la connection string
psql "$connectionString"

# Vérifier le firewall PostgreSQL
az postgres flexible-server firewall-rule list `
  --name smartoffice-prod-postgres `
  --resource-group smartoffice-prod-rg
```

---

## ?? Documentation Complète

Pour plus de détails : [GITHUB-ACTIONS-CICD-COMPLETE-GUIDE.md](GITHUB-ACTIONS-CICD-COMPLETE-GUIDE.md)

---

## ?? C'est Prêt !

À chaque push sur `main`, votre API sera automatiquement :
1. ? Buildée
2. ? Testée
3. ? Déployée sur Azure
4. ? Migrée (base de données)
5. ? Vérifiée (health check)

**Temps total de configuration : 5 minutes** ?
