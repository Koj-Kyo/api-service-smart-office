# Script de déploiement rapide après modification CORS
# Ce script déploie les changements sur Azure App Service

Write-Host "?? Déploiement des changements CORS sur Azure..." -ForegroundColor Cyan

# Variables
$ResourceGroup = "smartoffice-free-rg"
$AppName = "smartoffice-api-free"
$ProjectPath = "D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP"

# 1. Build du projet
Write-Host "`n?? Build du projet en Release..." -ForegroundColor Yellow
cd $ProjectPath
dotnet clean
dotnet build --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Erreur lors du build" -ForegroundColor Red
    exit 1
}

Write-Host "? Build réussi" -ForegroundColor Green

# 2. Publication
Write-Host "`n?? Publication du projet..." -ForegroundColor Yellow
dotnet publish -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Erreur lors de la publication" -ForegroundColor Red
    exit 1
}

Write-Host "? Publication réussie" -ForegroundColor Green

# 3. Création du package zip
Write-Host "`n?? Création du package de déploiement..." -ForegroundColor Yellow

if (Test-Path "./deploy.zip") {
    Remove-Item "./deploy.zip" -Force
}

Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

if (-not (Test-Path "./deploy.zip")) {
    Write-Host "? Erreur lors de la création du zip" -ForegroundColor Red
    exit 1
}

Write-Host "? Package créé" -ForegroundColor Green

# 4. Vérification de la connexion Azure
Write-Host "`n?? Vérification de la connexion Azure..." -ForegroundColor Yellow

$azAccount = az account show --query "name" -o tsv 2>$null

if ($LASTEXITCODE -ne 0) {
    Write-Host "?? Vous n'êtes pas connecté à Azure" -ForegroundColor Yellow
    Write-Host "Connexion en cours..." -ForegroundColor Cyan
    az login
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Échec de la connexion Azure" -ForegroundColor Red
        exit 1
    }
}

Write-Host "? Connecté à Azure : $azAccount" -ForegroundColor Green

# 5. Déploiement sur Azure
Write-Host "`n?? Déploiement sur Azure App Service..." -ForegroundColor Yellow
Write-Host "   Resource Group: $ResourceGroup" -ForegroundColor Cyan
Write-Host "   App Name: $AppName" -ForegroundColor Cyan

az webapp deployment source config-zip `
  --resource-group $ResourceGroup `
  --name $AppName `
  --src ./deploy.zip

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Échec du déploiement" -ForegroundColor Red
    exit 1
}

Write-Host "? Déploiement réussi" -ForegroundColor Green

# 6. Redémarrage de l'application
Write-Host "`n?? Redémarrage de l'application..." -ForegroundColor Yellow

az webapp restart `
  --resource-group $ResourceGroup `
  --name $AppName

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Échec du redémarrage" -ForegroundColor Red
    exit 1
}

Write-Host "? Application redémarrée" -ForegroundColor Green

# 7. Nettoyage
Write-Host "`n?? Nettoyage des fichiers temporaires..." -ForegroundColor Yellow

if (Test-Path "./publish") {
    Remove-Item "./publish" -Recurse -Force
}

if (Test-Path "./deploy.zip") {
    Remove-Item "./deploy.zip" -Force
}

Write-Host "? Nettoyage terminé" -ForegroundColor Green

# 8. Résumé
Write-Host "`n" -NoNewline
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "? DÉPLOIEMENT TERMINÉ AVEC SUCCÈS !" -ForegroundColor Green
Write-Host "???????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host ""
Write-Host "?? URL du backend : " -NoNewline
Write-Host "https://$AppName.azurewebsites.net" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? URL du frontend : " -NoNewline
Write-Host "https://mango-bush-03e4a6703.3.azurestaticapps.net" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Attendez 1-2 minutes que l'application redémarre" -ForegroundColor Yellow
Write-Host ""
Write-Host "?? Tests recommandés :" -ForegroundColor Yellow
Write-Host "   1. Health check : https://$AppName.azurewebsites.net/health/ready" -ForegroundColor White
Write-Host "   2. Swagger : https://$AppName.azurewebsites.net/swagger" -ForegroundColor White
Write-Host "   3. Test CORS depuis le frontend (Console DevTools)" -ForegroundColor White
Write-Host ""

# Option pour ouvrir le navigateur
$openBrowser = Read-Host "Voulez-vous ouvrir Swagger dans le navigateur ? (O/N)"

if ($openBrowser -eq "O" -or $openBrowser -eq "o") {
    Start-Process "https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/swagger"
    Write-Host "? Swagger ouvert dans le navigateur" -ForegroundColor Green
}

Write-Host "`n?? Déploiement terminé ! Votre API est maintenant accessible." -ForegroundColor Green
Write-Host "?? Les requêtes depuis https://mango-bush-03e4a6703.3.azurestaticapps.net sont autorisées." -ForegroundColor Green
