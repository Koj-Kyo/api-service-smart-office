# ?? Déploiement SmartOffice API - Guide Express
# ?? Temps : 5 minutes

## Étape 1 : Préparer l'environnement (1 min)

```powershell
# Se connecter à Azure
az login

# Vérifier l'abonnement
az account show

# Obtenir votre Subscription ID
$subscriptionId = az account show --query id -o tsv
Write-Host "Votre Subscription ID : $subscriptionId"
```

## Étape 2 : Configurer les secrets (2 min)

```powershell
# Copier le fichier template
Copy-Item .env.example .env

# REMPLIR CES VALEURS DANS .env :
# AZURE_SUBSCRIPTION_ID=<votre-subscription-id>
# POSTGRESQL_ADMIN_PASSWORD=SmartOffice2025!
# JWT_SECRET=<votre-jwt-secret-actuel>
# IOTHUB_CONNECTION_STRING=<votre-connection-string-iothub>
# STORAGE_CONNECTION_STRING=<votre-connection-string-storage>

notepad .env
```

### Obtenir vos connection strings existantes :

```powershell
# IoT Hub Connection String
az iot hub connection-string show --hub-name brainoffice --policy-name service

# Storage Account Connection String (si vous l'avez)
az storage account show-connection-string --name <votre-storage-account> --resource-group <votre-rg>
```

## Étape 3 : Déployer (2 min)

```powershell
# Charger les variables d'environnement
Get-Content .env | ForEach-Object {
    if ($_ -match '^([^=]+)=(.*)$') {
        [Environment]::SetEnvironmentVariable($matches[1], $matches[2], 'Process')
    }
}

# DÉPLOYER !
azd up

# Suivre les prompts et attendre ~5 minutes
```

## ? Résultat

Vous obtiendrez :
- ?? URL API : `https://smartoffice-prod-api.azurewebsites.net`
- ?? Swagger : `https://smartoffice-prod-api.azurewebsites.net/swagger`
- ?? Health : `https://smartoffice-prod-api.azurewebsites.net/health/ready`

## ?? Test rapide

```powershell
# Tester le health check
$url = azd env get-value AZURE_WEBAPP_URL
Invoke-WebRequest -Uri "$url/health/ready"

# Ouvrir Swagger dans le navigateur
Start-Process "$url/swagger"

# Voir les logs
azd monitor --logs
```

## ?? Pour la soutenance

### Montrer l'infrastructure dans le portail Azure
```powershell
azd monitor --portal
```

### Métriques en temps réel
```powershell
azd monitor
```

### Arrêter après la soutenance (économiser)
```powershell
$webAppName = azd env get-value AZURE_WEBAPP_NAME
$resourceGroup = azd env get-value AZURE_RESOURCE_GROUP
az webapp stop --name $webAppName --resource-group $resourceGroup
```

### Redémarrer avant une démo
```powershell
az webapp start --name $webAppName --resource-group $resourceGroup
```

### Nettoyer complètement (FIN DU PROJET)
```powershell
azd down --force --purge
```

## ?? Problèmes courants

### "PostgreSQL password invalid"
Le mot de passe doit avoir : majuscule, minuscule, chiffre, 8+ caractères
Exemple : `SmartOffice2025!`

### "azd not found"
Installer azd : https://aka.ms/install-azd

### "Key Vault access denied"
Attendre 30 secondes puis :
```powershell
azd deploy
```

## ?? Aide complète
Voir `AZURE-DEPLOYMENT-COMPLETE-GUIDE.md` pour la documentation détaillée.

---

**?? C'est tout ! Votre API sera déployée en 5 minutes.**
