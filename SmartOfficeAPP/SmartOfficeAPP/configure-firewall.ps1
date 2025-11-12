# ============================================
# Script de Configuration Pare-feu
# SmartOffice API - Appareil Mobile USB
# ============================================
# 
# Ce script doit être exécuté en tant qu'ADMINISTRATEUR
# Clic droit sur PowerShell ? "Exécuter en tant qu'administrateur"
#

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configuration Pare-feu SmartOffice API" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier les privilèges administrateur
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "? ERREUR: Ce script doit être exécuté en tant qu'administrateur" -ForegroundColor Red
    Write-Host "Clic droit sur PowerShell ? 'Exécuter en tant qu'administrateur'" -ForegroundColor Yellow
    pause
    exit
}

Write-Host "? Privilèges administrateur détectés" -ForegroundColor Green
Write-Host ""

# Configuration des ports
$httpPort = 5290
$httpsPort = 7144

# Supprimer les règles existantes si elles existent
Write-Host "?? Suppression des anciennes règles (si existantes)..." -ForegroundColor Yellow
Remove-NetFirewallRule -DisplayName "SmartOffice API HTTP" -ErrorAction SilentlyContinue
Remove-NetFirewallRule -DisplayName "SmartOffice API HTTPS" -ErrorAction SilentlyContinue
Write-Host "? Nettoyage terminé" -ForegroundColor Green
Write-Host ""

# Créer la règle HTTP
Write-Host "?? Création de la règle HTTP (port $httpPort)..." -ForegroundColor Yellow
try {
    New-NetFirewallRule `
        -DisplayName "SmartOffice API HTTP" `
        -Direction Inbound `
        -LocalPort $httpPort `
        -Protocol TCP `
        -Action Allow `
        -Profile Domain,Private,Public `
        -Description "Permet l'accès HTTP à l'API SmartOffice depuis des appareils mobiles" `
        -ErrorAction Stop | Out-Null
    Write-Host "? Règle HTTP créée avec succès" -ForegroundColor Green
} catch {
    Write-Host "? Erreur lors de la création de la règle HTTP: $_" -ForegroundColor Red
}

# Créer la règle HTTPS
Write-Host "?? Création de la règle HTTPS (port $httpsPort)..." -ForegroundColor Yellow
try {
    New-NetFirewallRule `
        -DisplayName "SmartOffice API HTTPS" `
        -Direction Inbound `
        -LocalPort $httpsPort `
        -Protocol TCP `
        -Action Allow `
        -Profile Domain,Private,Public `
        -Description "Permet l'accès HTTPS à l'API SmartOffice depuis des appareils mobiles" `
        -ErrorAction Stop | Out-Null
    Write-Host "? Règle HTTPS créée avec succès" -ForegroundColor Green
} catch {
    Write-Host "? Erreur lors de la création de la règle HTTPS: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configuration terminée!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Afficher l'adresse IP
Write-Host "?? Votre adresse IP locale:" -ForegroundColor Cyan
$ipAddresses = Get-NetIPAddress -AddressFamily IPv4 | 
    Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.PrefixOrigin -eq "Dhcp" -or $_.PrefixOrigin -eq "Manual" } |
    Select-Object IPAddress, InterfaceAlias

foreach ($ip in $ipAddresses) {
    if ($ip.IPAddress -match "^(192\.168\.|10\.|172\.(1[6-9]|2[0-9]|3[0-1])\.)") {
        Write-Host "   ? $($ip.IPAddress) ($($ip.InterfaceAlias))" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "?? Instructions suivantes:" -ForegroundColor Cyan
Write-Host "   1. Notez l'adresse IP ci-dessus" -ForegroundColor White
Write-Host "   2. Démarrez le backend avec: dotnet run --launch-profile http" -ForegroundColor White
Write-Host "   3. Configurez votre app mobile avec: http://[VOTRE_IP]:5290" -ForegroundColor White
Write-Host "   4. Testez avec: http://[VOTRE_IP]:5290/health/live" -ForegroundColor White
Write-Host ""

# Vérifier les règles créées
Write-Host "?? Vérification des règles de pare-feu:" -ForegroundColor Cyan
Get-NetFirewallRule -DisplayName "SmartOffice API*" | 
    Select-Object DisplayName, Enabled, Direction, Action |
    Format-Table -AutoSize

Write-Host ""
Write-Host "? Configuration terminée avec succès!" -ForegroundColor Green
Write-Host "?? Consultez MOBILE-USB-DEVICE-SETUP.md pour plus de détails" -ForegroundColor Yellow
Write-Host ""

pause
