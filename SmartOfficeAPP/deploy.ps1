# =========================================
# ?? SmartOffice API - Script de déploiement automatisé
# Facilite le déploiement sur Azure avec azd
# =========================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('init', 'deploy', 'provision', 'up', 'down', 'monitor', 'logs', 'status')]
    [string]$Action = 'up',
    
    [Parameter(Mandatory=$false)]
    [switch]$Force,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipMigrations
)

# Couleurs pour les messages
function Write-Success { param($Message) Write-Host "? $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "??  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "??  $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "? $Message" -ForegroundColor Red }
function Write-Step { param($Message) Write-Host "`n?? $Message" -ForegroundColor Magenta }

# Banner
Write-Host @"
?????????????????????????????????????????????????????????????
?                                                           ?
?   ?? SmartOffice API - Déploiement Azure                 ?
?   Infrastructure as Code avec Bicep + azd                ?
?                                                           ?
?????????????????????????????????????????????????????????????
"@ -ForegroundColor Cyan

# Vérifier les prérequis
Write-Step "Vérification des prérequis..."

# Azure CLI
try {
    $azVersion = az --version 2>$null | Select-Object -First 1
    Write-Success "Azure CLI installé : $azVersion"
} catch {
    Write-Error "Azure CLI non trouvé. Installer depuis : https://aka.ms/installazurecli"
    exit 1
}

# Azure Developer CLI
try {
    $azdVersion = azd version 2>$null
    Write-Success "Azure Developer CLI installé : $azdVersion"
} catch {
    Write-Error "Azure Developer CLI (azd) non trouvé. Installer depuis : https://aka.ms/install-azd"
    exit 1
}

# .NET SDK
try {
    $dotnetVersion = dotnet --version 2>$null
    Write-Success ".NET SDK installé : $dotnetVersion"
} catch {
    Write-Error ".NET SDK non trouvé. Installer depuis : https://dotnet.microsoft.com/download"
    exit 1
}

# Vérifier l'authentification Azure
Write-Step "Vérification de l'authentification Azure..."
try {
    $account = az account show 2>$null | ConvertFrom-Json
    Write-Success "Connecté à Azure : $($account.user.name)"
    Write-Info "Abonnement actif : $($account.name) ($($account.id))"
} catch {
    Write-Warning "Non connecté à Azure. Connexion en cours..."
    az login
    $account = az account show | ConvertFrom-Json
    Write-Success "Connecté à Azure : $($account.user.name)"
}

# Charger les variables d'environnement depuis .env si disponible
if (Test-Path ".env") {
    Write-Step "Chargement des variables d'environnement depuis .env..."
    Get-Content .env | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$' -and -not $_.StartsWith('#')) {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            if ($value) {
                [Environment]::SetEnvironmentVariable($name, $value, 'Process')
                Write-Info "Variable chargée : $name"
            }
        }
    }
    Write-Success "Variables d'environnement chargées depuis .env"
} else {
    Write-Warning "Fichier .env non trouvé. Copiez .env.example vers .env et remplissez les valeurs."
    
    if ($Action -eq 'up' -or $Action -eq 'provision') {
        $response = Read-Host "Voulez-vous créer .env maintenant ? (o/n)"
        if ($response -eq 'o' -or $response -eq 'O') {
            Copy-Item .env.example .env
            Write-Success "Fichier .env créé. Éditez-le avec vos valeurs puis relancez le script."
            notepad .env
            exit 0
        }
    }
}

# Exécuter l'action demandée
Write-Step "Exécution de l'action : $Action"

switch ($Action) {
    'init' {
        Write-Info "Initialisation de l'environnement azd..."
        azd init
        Write-Success "Environnement azd initialisé"
    }
    
    'provision' {
        Write-Info "Provisionnement de l'infrastructure Azure uniquement..."
        Write-Warning "Cela créera toutes les ressources Azure sans déployer le code"
        
        if (-not $Force) {
            $confirm = Read-Host "Continuer ? (o/n)"
            if ($confirm -ne 'o' -and $confirm -ne 'O') {
                Write-Warning "Opération annulée"
                exit 0
            }
        }
        
        # Preview des changements
        Write-Info "Aperçu des ressources qui seront créées..."
        azd provision --preview
        
        $confirm = Read-Host "`nConfirmer le provisionnement ? (o/n)"
        if ($confirm -eq 'o' -or $confirm -eq 'O') {
            azd provision
            Write-Success "Infrastructure provisionnée avec succès"
        }
    }
    
    'deploy' {
        Write-Info "Déploiement du code uniquement (sans modifier l'infrastructure)..."
        azd deploy
        Write-Success "Code déployé avec succès"
    }
    
    'up' {
        Write-Info "Provisionnement de l'infrastructure + déploiement du code..."
        Write-Warning "Cela créera toutes les ressources Azure ET déploiera le code"
        
        if (-not $Force) {
            $confirm = Read-Host "Continuer ? (o/n)"
            if ($confirm -ne 'o' -and $confirm -ne 'O') {
                Write-Warning "Opération annulée"
                exit 0
            }
        }
        
        # Déploiement complet
        azd up
        
        # Récupérer les informations de déploiement
        Write-Step "Récupération des informations de déploiement..."
        
        $webAppUrl = azd env get-value AZURE_WEBAPP_URL 2>$null
        $resourceGroup = azd env get-value AZURE_RESOURCE_GROUP 2>$null
        $subscriptionId = $account.id
        
        Write-Host "`n?????????????????????????????????????????????????????????????" -ForegroundColor Green
        Write-Host "?          ?? DÉPLOIEMENT TERMINÉ AVEC SUCCÈS               ?" -ForegroundColor Green
        Write-Host "?????????????????????????????????????????????????????????????" -ForegroundColor Green
        
        if ($webAppUrl) {
            Write-Host "`n?? Informations de déploiement :" -ForegroundColor Cyan
            Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray
            Write-Host "?? URL de l'API       : " -NoNewline -ForegroundColor White
            Write-Host $webAppUrl -ForegroundColor Green
            Write-Host "?? Swagger UI         : " -NoNewline -ForegroundColor White
            Write-Host "$webAppUrl/swagger" -ForegroundColor Green
            Write-Host "??  Health Check      : " -NoNewline -ForegroundColor White
            Write-Host "$webAppUrl/health/ready" -ForegroundColor Green
        }
        
        if ($resourceGroup) {
            Write-Host "`n?? Resource Group     : " -NoNewline -ForegroundColor White
            Write-Host $resourceGroup -ForegroundColor Yellow
            $portalUrl = "https://portal.azure.com/#@/resource/subscriptions/$subscriptionId/resourceGroups/$resourceGroup"
            Write-Host "?? Portail Azure      : " -NoNewline -ForegroundColor White
            Write-Host $portalUrl -ForegroundColor Cyan
        }
        
        Write-Host "`n???????????????????????????????????????????????????" -ForegroundColor Gray
        Write-Host "`n?? Commandes utiles :" -ForegroundColor Cyan
        Write-Host "   .\deploy.ps1 monitor         # Ouvrir Application Insights" -ForegroundColor White
        Write-Host "   .\deploy.ps1 logs            # Voir les logs en temps réel" -ForegroundColor White
        Write-Host "   .\deploy.ps1 status          # Voir l'état du déploiement" -ForegroundColor White
        Write-Host "   .\deploy.ps1 deploy          # Redéployer uniquement le code" -ForegroundColor White
        Write-Host "   .\deploy.ps1 down            # Supprimer l'infrastructure" -ForegroundColor White
        Write-Host ""
        
        # Tester le health check
        if ($webAppUrl) {
            Write-Step "Test du health check..."
            try {
                $healthResponse = Invoke-WebRequest -Uri "$webAppUrl/health/ready" -UseBasicParsing -TimeoutSec 10
                if ($healthResponse.StatusCode -eq 200) {
                    Write-Success "Health check OK : API opérationnelle ?"
                }
            } catch {
                Write-Warning "Health check échoué. L'API démarre peut-être encore..."
                Write-Info "Attendez 1-2 minutes puis testez : $webAppUrl/health/ready"
            }
        }
        
        Write-Success "Déploiement terminé avec succès"
    }
    
    'down' {
        Write-Warning "ATTENTION : Cela supprimera TOUTE l'infrastructure Azure !"
        Write-Warning "Toutes les ressources (App Service, PostgreSQL, Redis, Key Vault) seront supprimées."
        
        if (-not $Force) {
            $confirm = Read-Host "`nÊtes-vous ABSOLUMENT sûr ? Tapez 'SUPPRIMER' pour confirmer"
            if ($confirm -ne 'SUPPRIMER') {
                Write-Warning "Opération annulée (heureusement !)"
                exit 0
            }
        }
        
        Write-Info "Suppression de l'infrastructure..."
        azd down --force --purge
        Write-Success "Infrastructure supprimée"
    }
    
    'monitor' {
        Write-Info "Ouverture d'Application Insights..."
        azd monitor
    }
    
    'logs' {
        Write-Info "Affichage des logs en temps réel..."
        azd monitor --logs
    }
    
    'status' {
        Write-Step "État de l'environnement azd..."
        azd env list
        
        Write-Host "`n?? Valeurs d'environnement :" -ForegroundColor Cyan
        azd env get-values
        
        $webAppUrl = azd env get-value AZURE_WEBAPP_URL 2>$null
        if ($webAppUrl) {
            Write-Step "Test de connectivité..."
            try {
                $response = Invoke-WebRequest -Uri "$webAppUrl/health/ready" -UseBasicParsing -TimeoutSec 5
                Write-Success "API accessible : $webAppUrl"
                Write-Success "Status : $($response.StatusCode) $($response.StatusDescription)"
            } catch {
                Write-Warning "API non accessible : $($_.Exception.Message)"
            }
        }
    }
}

Write-Host "`n? Opération terminée" -ForegroundColor Green
