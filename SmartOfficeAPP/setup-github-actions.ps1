# =========================================
# ?? Script de Configuration CI/CD GitHub Actions
# Configure automatiquement le Service Principal et les secrets
# =========================================

param(
    [Parameter(Mandatory=$false)]
    [string]$GitHubRepo = "Koj-Kyo/api-service-smart-office",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "smartoffice-prod-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$AppName = "github-actions-smartoffice"
)

# Couleurs
function Write-Success { param($Message) Write-Host "? $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "??  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "??  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "? $Message" -ForegroundColor Red }
function Write-Step { param($Message) Write-Host "`n?? $Message" -ForegroundColor Magenta }

# Banner
Write-Host @"
?????????????????????????????????????????????????????????????
?                                                           ?
?   ?? Configuration CI/CD GitHub Actions                  ?
?   SmartOffice API - Azure Deployment                     ?
?                                                           ?
?????????????????????????????????????????????????????????????
"@ -ForegroundColor Cyan

# Vérifier Azure CLI
Write-Step "Vérification des prérequis..."
try {
    $azVersion = az --version | Select-Object -First 1
    Write-Success "Azure CLI installé"
} catch {
    Write-Error "Azure CLI non trouvé. Installer depuis : https://aka.ms/installazurecli"
    exit 1
}

# Vérifier GitHub CLI
try {
    $ghVersion = gh --version | Select-Object -First 1
    Write-Success "GitHub CLI installé"
    $useGhCli = $true
} catch {
    Write-Warning "GitHub CLI non trouvé. Les secrets devront être ajoutés manuellement."
    Write-Info "Télécharger depuis : https://cli.github.com/"
    $useGhCli = $false
}

# Authentification Azure
Write-Step "Authentification Azure..."
try {
    $account = az account show 2>$null | ConvertFrom-Json
    Write-Success "Connecté à Azure : $($account.user.name)"
} catch {
    Write-Info "Connexion à Azure..."
    az login
    $account = az account show | ConvertFrom-Json
    Write-Success "Connecté à Azure : $($account.user.name)"
}

# Récupérer les informations Azure
$subscriptionId = $account.id
$tenantId = $account.tenantId
$subscriptionName = $account.name

Write-Host "`n?? Informations Azure :" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray
Write-Host "Abonnement     : $subscriptionName" -ForegroundColor White
Write-Host "Subscription ID: $subscriptionId" -ForegroundColor Yellow
Write-Host "Tenant ID      : $tenantId" -ForegroundColor Yellow
Write-Host "Resource Group : $ResourceGroup" -ForegroundColor Yellow
Write-Host "GitHub Repo    : $GitHubRepo" -ForegroundColor Yellow
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray

# Confirmation
$confirm = Read-Host "`nContinuer avec ces paramètres ? (o/n)"
if ($confirm -ne 'o' -and $confirm -ne 'O') {
    Write-Warning "Opération annulée"
    exit 0
}

# Créer le Service Principal
Write-Step "Création du Service Principal..."

# Vérifier si l'app existe déjà
$existingApp = az ad app list --filter "displayName eq '$AppName'" | ConvertFrom-Json
if ($existingApp.Count -gt 0) {
    Write-Warning "Une application '$AppName' existe déjà"
    $overwrite = Read-Host "Voulez-vous la réutiliser ? (o/n)"
    
    if ($overwrite -eq 'o' -or $overwrite -eq 'O') {
        $appId = $existingApp[0].appId
        Write-Info "Réutilisation de l'application existante : $appId"
    } else {
        Write-Error "Opération annulée. Supprimez l'application existante ou utilisez un autre nom."
        exit 1
    }
} else {
    # Créer l'App Registration
    $appId = az ad app create --display-name $AppName --query appId -o tsv
    Write-Success "App Registration créée : $appId"
}

# Créer le Service Principal (si pas déjà existant)
$existingSp = az ad sp list --filter "appId eq '$appId'" | ConvertFrom-Json
if ($existingSp.Count -eq 0) {
    az ad sp create --id $appId | Out-Null
    Write-Success "Service Principal créé"
} else {
    Write-Info "Service Principal existe déjà"
}

# Assigner le rôle Contributor
Write-Step "Attribution des permissions..."
$roleScope = "/subscriptions/$subscriptionId/resourceGroups/$ResourceGroup"

# Vérifier si le rôle est déjà assigné
$existingRole = az role assignment list --assignee $appId --scope $roleScope | ConvertFrom-Json
if ($existingRole.Count -eq 0) {
    az role assignment create `
        --assignee $appId `
        --role Contributor `
        --scope $roleScope | Out-Null
    Write-Success "Rôle Contributor assigné sur $ResourceGroup"
} else {
    Write-Info "Rôle Contributor déjà assigné"
}

# Configurer Federated Credentials (OIDC)
Write-Step "Configuration des Federated Credentials (OIDC)..."

# Vérifier les credentials existantes
$existingCreds = az ad app federated-credential list --id $appId | ConvertFrom-Json

# Credential pour main branch
$mainBranchCredName = "github-main-branch"
if (-not ($existingCreds | Where-Object { $_.name -eq $mainBranchCredName })) {
    $mainBranchParams = @{
        name = $mainBranchCredName
        issuer = "https://token.actions.githubusercontent.com"
        subject = "repo:$GitHubRepo:ref:refs/heads/main"
        description = "GitHub Actions for main branch"
        audiences = @("api://AzureADTokenExchange")
    } | ConvertTo-Json -Compress
    
    az ad app federated-credential create --id $appId --parameters $mainBranchParams | Out-Null
    Write-Success "Federated Credential créée pour main branch"
} else {
    Write-Info "Federated Credential pour main branch existe déjà"
}

# Credential pour pull requests
$prCredName = "github-pull-requests"
if (-not ($existingCreds | Where-Object { $_.name -eq $prCredName })) {
    $prParams = @{
        name = $prCredName
        issuer = "https://token.actions.githubusercontent.com"
        subject = "repo:$GitHubRepo:pull_request"
        description = "GitHub Actions for pull requests"
        audiences = @("api://AzureADTokenExchange")
    } | ConvertTo-Json -Compress
    
    az ad app federated-credential create --id $appId --parameters $prParams | Out-Null
    Write-Success "Federated Credential créée pour pull requests"
} else {
    Write-Info "Federated Credential pour pull requests existe déjà"
}

# Obtenir la connection string PostgreSQL
Write-Step "Configuration de la connection string PostgreSQL..."

# Essayer de récupérer depuis azd
$postgresHost = $null
try {
    $postgresHost = azd env get-value AZURE_POSTGRESQL_FQDN 2>$null
} catch {}

if (-not $postgresHost) {
    Write-Info "Impossible de récupérer le host PostgreSQL depuis azd"
    $postgresHost = Read-Host "Entrez le FQDN PostgreSQL (ex: smartoffice-prod-postgres.postgres.database.azure.com)"
}

$postgresDb = Read-Host "Nom de la base de données [smartofficedb]"
if ([string]::IsNullOrWhiteSpace($postgresDb)) { $postgresDb = "smartofficedb" }

$postgresUser = Read-Host "Nom d'utilisateur PostgreSQL [smartofficeadmin]"
if ([string]::IsNullOrWhiteSpace($postgresUser)) { $postgresUser = "smartofficeadmin" }

$postgresPassword = Read-Host "Mot de passe PostgreSQL" -AsSecureString
$postgresPasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($postgresPassword)
)

$connectionString = "Host=$postgresHost;Database=$postgresDb;Username=$postgresUser;Password=$postgresPasswordPlain;SSL Mode=Require"
Write-Success "Connection string générée"

# Configurer les secrets GitHub
Write-Step "Configuration des secrets GitHub..."

$secrets = @{
    "AZURE_CLIENT_ID" = $appId
    "AZURE_TENANT_ID" = $tenantId
    "AZURE_SUBSCRIPTION_ID" = $subscriptionId
    "POSTGRESQL_CONNECTION_STRING" = $connectionString
}

if ($useGhCli) {
    # Vérifier l'authentification GitHub
    try {
        gh auth status 2>$null | Out-Null
        Write-Info "GitHub CLI authentifié"
    } catch {
        Write-Info "Connexion à GitHub..."
        gh auth login
    }
    
    # Ajouter les secrets
    foreach ($secretName in $secrets.Keys) {
        $secretValue = $secrets[$secretName]
        try {
            gh secret set $secretName -b"$secretValue" --repo $GitHubRepo 2>$null
            Write-Success "Secret '$secretName' configuré"
        } catch {
            Write-Warning "Échec de configuration du secret '$secretName'"
        }
    }
    
    # Vérifier les secrets
    Write-Step "Vérification des secrets GitHub..."
    gh secret list --repo $GitHubRepo
    
} else {
    Write-Warning "GitHub CLI non disponible. Secrets à ajouter manuellement :"
    Write-Host "`n?? Secrets à ajouter dans GitHub :" -ForegroundColor Cyan
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray
    Write-Host "URL: https://github.com/$GitHubRepo/settings/secrets/actions" -ForegroundColor Yellow
    Write-Host ""
    foreach ($secretName in $secrets.Keys) {
        Write-Host "Secret Name: $secretName" -ForegroundColor White
        Write-Host "Valeur     : $($secrets[$secretName])" -ForegroundColor Yellow
        Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray
    }
}

# Résumé final
Write-Host "`n?????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?          ? CONFIGURATION TERMINÉE AVEC SUCCÈS            ?" -ForegroundColor Green
Write-Host "?????????????????????????????????????????????????????????????" -ForegroundColor Green

Write-Host "`n?? Résumé de la configuration :" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray
Write-Host "Service Principal  : $appId" -ForegroundColor Yellow
Write-Host "GitHub Repository  : $GitHubRepo" -ForegroundColor Yellow
Write-Host "Resource Group     : $ResourceGroup" -ForegroundColor Yellow
Write-Host "OIDC Configuré     : ? Main branch + Pull Requests" -ForegroundColor Green
Write-Host "Secrets GitHub     : $(if($useGhCli){'? Configurés'}else{'?? À configurer manuellement'})" -ForegroundColor $(if($useGhCli){'Green'}else{'Yellow'})
Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray

Write-Host "`n?? Prochaines étapes :" -ForegroundColor Cyan
Write-Host "1. Commit et push le workflow GitHub Actions" -ForegroundColor White
Write-Host "   git add .github/workflows/azure-deploy.yml" -ForegroundColor Gray
Write-Host "   git commit -m 'feat: Add CI/CD pipeline'" -ForegroundColor Gray
Write-Host "   git push origin main" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Suivre l'exécution du workflow" -ForegroundColor White
Write-Host "   https://github.com/$GitHubRepo/actions" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Vérifier le déploiement" -ForegroundColor White
Write-Host "   https://smartoffice-prod-api.azurewebsites.net/health/ready" -ForegroundColor Gray
Write-Host ""

Write-Success "Configuration CI/CD terminée !"
