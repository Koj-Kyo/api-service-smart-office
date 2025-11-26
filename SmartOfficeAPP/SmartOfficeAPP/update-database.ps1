# Script pour mettre à jour la base de données avec Entity Framework
# Utilise la chaîne de connexion depuis les secrets utilisateur

Write-Host "?? Mise à jour de la base de données..." -ForegroundColor Cyan

# Récupérer la chaîne de connexion depuis les secrets utilisateur
$connectionString = dotnet user-secrets get "ConnectionStrings:DefaultConnection"

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    Write-Host "? Erreur: Chaîne de connexion non trouvée dans les secrets utilisateur" -ForegroundColor Red
    Write-Host "Exécutez: dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'VOTRE_CHAINE'" -ForegroundColor Yellow
    exit 1
}

# Définir la variable d'environnement pour EF Tools
$env:ConnectionStrings__DefaultConnection = $connectionString

# Exécuter la mise à jour
dotnet ef database update

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Base de données mise à jour avec succès!" -ForegroundColor Green
} else {
    Write-Host "? Échec de la mise à jour de la base de données" -ForegroundColor Red
    exit $LASTEXITCODE
}
