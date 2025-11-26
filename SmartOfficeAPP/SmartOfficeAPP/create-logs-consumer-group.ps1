# create-logs-consumer-group.ps1
# ?? Script automatisé pour créer le consumer group "logs" dans Azure IoT Hub

param(
    [Parameter(Mandatory=$false)]
    [string]$IotHubName = "brainoffice",
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup = "smartoffice-prod-rg",
    
    [Parameter(Mandatory=$false)]
    [string]$ConsumerGroupName = "logs"
)

Write-Host ""
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "  Azure IoT Hub - Création du Consumer Group '$ConsumerGroupName'" -ForegroundColor Cyan
Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Vérifier si Azure CLI est installé
Write-Host "?? Vérification de Azure CLI..." -ForegroundColor Gray
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Host "? Azure CLI n'est pas installé." -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Installation requise :" -ForegroundColor Yellow
    Write-Host "   https://aka.ms/installazurecliwindows" -ForegroundColor White
    Write-Host ""
    Write-Host "Ou utilisez PowerShell :" -ForegroundColor Yellow
    Write-Host "   winget install -e --id Microsoft.AzureCLI" -ForegroundColor White
    Write-Host ""
    exit 1
}
Write-Host "? Azure CLI installé" -ForegroundColor Green

# Vérifier la version
$azVersion = az --version | Select-String "azure-cli" | Out-String
Write-Host "   Version: $($azVersion.Trim())" -ForegroundColor Gray
Write-Host ""

# Vérifier si l'utilisateur est connecté
Write-Host "?? Vérification de la connexion Azure..." -ForegroundColor Gray
$account = az account show 2>$null | ConvertFrom-Json

if (-not $account) {
    Write-Host "?? Vous n'êtes pas connecté à Azure" -ForegroundColor Yellow
    Write-Host "?? Lancement de la connexion..." -ForegroundColor Cyan
    Write-Host ""
    
    az login
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Échec de la connexion Azure" -ForegroundColor Red
        exit 1
    }
    
    $account = az account show | ConvertFrom-Json
}

Write-Host "? Connecté à Azure" -ForegroundColor Green
Write-Host "   Subscription: $($account.name)" -ForegroundColor Gray
Write-Host "   Tenant: $($account.tenantId)" -ForegroundColor Gray
Write-Host ""

# Vérifier si le Resource Group existe
Write-Host "?? Vérification du Resource Group '$ResourceGroup'..." -ForegroundColor Gray
$rgExists = az group exists --name $ResourceGroup 2>$null

if ($rgExists -ne "true") {
    Write-Host "? Resource Group '$ResourceGroup' introuvable" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Suggestion: Créez-le d'abord avec :" -ForegroundColor Yellow
    Write-Host "   az group create --name $ResourceGroup --location eastus" -ForegroundColor White
    Write-Host ""
    exit 1
}
Write-Host "? Resource Group trouvé" -ForegroundColor Green
Write-Host ""

# Vérifier si l'IoT Hub existe
Write-Host "?? Vérification de l'IoT Hub '$IotHubName'..." -ForegroundColor Gray
$hubData = az iot hub show --name $IotHubName --resource-group $ResourceGroup 2>$null

if (-not $hubData) {
    Write-Host "? IoT Hub '$IotHubName' introuvable dans '$ResourceGroup'" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Vérifiez le nom de votre IoT Hub sur le portail Azure" -ForegroundColor Yellow
    Write-Host "   https://portal.azure.com/#blade/HubsExtension/BrowseResource/resourceType/Microsoft.Devices%2FIotHubs" -ForegroundColor White
    Write-Host ""
    exit 1
}

$hub = $hubData | ConvertFrom-Json
Write-Host "? IoT Hub trouvé" -ForegroundColor Green
Write-Host "   Emplacement: $($hub.location)" -ForegroundColor Gray
Write-Host "   SKU: $($hub.sku.name) ($($hub.sku.tier))" -ForegroundColor Gray
Write-Host ""

# Lister les consumer groups existants
Write-Host "?? Consumer groups actuels :" -ForegroundColor Cyan
$existingGroups = az iot hub consumer-group list `
    --hub-name $IotHubName `
    --resource-group $ResourceGroup `
    --query "[].name" `
    -o tsv

if ($existingGroups) {
    foreach ($group in $existingGroups) {
        Write-Host "   • $group" -ForegroundColor Gray
    }
} else {
    Write-Host "   (Aucun consumer group personnalisé)" -ForegroundColor Gray
}
Write-Host ""

# Vérifier si le consumer group existe déjà
if ($existingGroups -contains $ConsumerGroupName) {
    Write-Host "? Le consumer group '$ConsumerGroupName' existe déjà" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Aucune action nécessaire. Vous pouvez redémarrer le backend." -ForegroundColor Yellow
    Write-Host ""
    exit 0
}

# Créer le consumer group
Write-Host "?? Création du consumer group '$ConsumerGroupName'..." -ForegroundColor Yellow
Write-Host ""

az iot hub consumer-group create `
    --hub-name $IotHubName `
    --resource-group $ResourceGroup `
    --name $ConsumerGroupName `
    --output none

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Consumer group '$ConsumerGroupName' créé avec succès !" -ForegroundColor Green
    Write-Host ""
    
    # Afficher la liste mise à jour
    Write-Host "?? Consumer groups mis à jour :" -ForegroundColor Cyan
    $updatedGroups = az iot hub consumer-group list `
        --hub-name $IotHubName `
        --resource-group $ResourceGroup `
        --output table
    
    Write-Host $updatedGroups
    Write-Host ""
    
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host "  ?? Configuration terminée avec succès !" -ForegroundColor Green
    Write-Host "???????????????????????????????????????????????????????????" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "?? Prochaines étapes :" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1?? Vérifiez que 'LogsEnabled: true' dans appsettings.json" -ForegroundColor White
    Write-Host "   Chemin: SmartOfficeAPP/appsettings.json" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2?? Redémarrez le backend :" -ForegroundColor White
    Write-Host "   cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP" -ForegroundColor Gray
    Write-Host "   dotnet run" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3?? Vérifiez les logs de démarrage :" -ForegroundColor White
    Write-Host "   Vous devriez voir : 'LogIngestionWorker démarré avec succès'" -ForegroundColor Gray
    Write-Host ""
    Write-Host "4?? (Optionnel) Modifiez le firmware ESP32 pour envoyer des logs" -ForegroundColor White
    Write-Host "   Consultez: CREATE-LOGS-CONSUMER-GROUP-GUIDE.md" -ForegroundColor Gray
    Write-Host ""
    
} else {
    Write-Host "? Échec de la création du consumer group" -ForegroundColor Red
    Write-Host ""
    Write-Host "?? Causes possibles :" -ForegroundColor Yellow
    Write-Host "   • Permissions insuffisantes (Contributor ou Owner requis)" -ForegroundColor White
    Write-Host "   • IoT Hub en lecture seule" -ForegroundColor White
    Write-Host "   • Problème de connectivité réseau" -ForegroundColor White
    Write-Host ""
    Write-Host "?? Solution alternative : Créez le consumer group manuellement sur le portail Azure" -ForegroundColor Yellow
    Write-Host "   1. Allez sur https://portal.azure.com" -ForegroundColor White
    Write-Host "   2. IoT Hub ? Built-in endpoints ? Add consumer group" -ForegroundColor White
    Write-Host "   3. Nom: logs" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host ""
