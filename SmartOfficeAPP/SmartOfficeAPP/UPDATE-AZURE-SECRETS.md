# ?? Mise à jour des Secrets Azure - SmartOfficeAPP

## ?? État actuel de vos secrets

Voici les secrets **actuellement configurés** dans votre projet :

```powershell
? IotHubConnectionString              = HostName=brainoffice.azure-devices.net;...
? IotHubEventHub:ConnectionString     = Endpoint=sb://iothub-ns-brainoffic-67368601-...
? IotHubEventHub:Name                 = brainoffice
? IotHubEventHub:BlobConnectionString = DefaultEndpointsProtocol=https;AccountName=smartofficecheckpoints;...
? IotHubEventHub:BlobContainer        = iothub-checkpoints
? ConnectionStrings:DefaultConnection = Host=localhost;Port=5432;Database=BureauIntelligentDB;...
? AutoMapper:LicenseKey               = eyJhbGc...
? Kestrel:Certificates:Development:Password = 65375556-...
```

---

## ?? Secrets à mettre à jour pour votre nouveau compte Azure

### ?? IMPORTANT : Voici les 4 secrets Azure à modifier

| # | Secret Key | Ce qu'il faut récupérer | Obligatoire |
|---|-----------|------------------------|-------------|
| 1?? | `IotHubConnectionString` | Connection string principale IoT Hub | ? OUI |
| 2?? | `IotHubEventHub:ConnectionString` | Event Hub endpoint intégré | ? OUI |
| 3?? | `IotHubEventHub:BlobConnectionString` | Storage Account connection string | ? OUI |
| 4?? | `IotHubEventHub:Name` | Nom de l'Event Hub (généralement "brainoffice") | ? OUI |

### ?? Secrets à GARDER (ne pas modifier)

Ces secrets sont locaux et ne changent PAS avec votre compte Azure :

- ? `ConnectionStrings:DefaultConnection` (PostgreSQL local)
- ? `AutoMapper:LicenseKey` (licence AutoMapper)
- ? `Kestrel:Certificates:Development:Password` (certificat SSL local)

---

## ?? Guide étape par étape

### Étape 1 : Récupérer les nouvelles connection strings depuis Azure Portal

#### 1?? **IoT Hub Connection String**

```sh
# Dans le portail Azure:
1. Ouvrez votre IoT Hub (par exemple: "smartoffice-prod-iothub" ou "brainoffice")
2. Menu de gauche : "Security settings" > "Shared access policies"
3. Cliquez sur "iothubowner"
4. Copiez "Primary connection string"

# Format attendu:
HostName=VOTRE_IOT_HUB.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=VOTRE_CLE
```

**Exemple de ce que vous devriez copier :**
```
HostName=smartoffice-prod-iothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=AbCdEfGhIjKlMnOpQrStUvWxYz1234567890==
```

---

#### 2?? **Event Hub Connection String**

```sh
# Dans le portail Azure:
1. Toujours dans votre IoT Hub
2. Menu : "Hub settings" > "Built-in endpoints"
3. Cherchez la section "Event Hub-compatible endpoint"
4. Copiez la connection string complète

# Format attendu:
Endpoint=sb://iothub-ns-XXXXX.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=VOTRE_CLE;EntityPath=NOM_EVENT_HUB
```

**Exemple de ce que vous devriez copier :**
```
Endpoint=sb://iothub-ns-smartoffice-12345678.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=AbCdEfGhIjKlMnOpQrStUvWxYz1234567890==;EntityPath=brainoffice
```

?? **IMPORTANT** : Vérifiez que `EntityPath=` est bien présent à la fin !

---

#### 3?? **Storage Account Connection String**

```sh
# Dans le portail Azure:
1. Ouvrez votre Storage Account (par exemple: "smartofficeprodsa" ou "smartofficecheckpoints")
2. Menu : "Security + networking" > "Access keys"
3. Sous "key1", cliquez sur "Show"
4. Copiez la "Connection string"

# Format attendu:
DefaultEndpointsProtocol=https;AccountName=VOTRE_STORAGE;AccountKey=VOTRE_CLE;EndpointSuffix=core.windows.net
```

**Exemple de ce que vous devriez copier :**
```
DefaultEndpointsProtocol=https;AccountName=smartofficeprodsa;AccountKey=AbCdEfGhIjKlMnOpQrStUvWxYz1234567890==;EndpointSuffix=core.windows.net
```

---

#### 4?? **Event Hub Name**

```sh
# Dans le portail Azure:
1. Toujours dans "Built-in endpoints" de votre IoT Hub
2. Cherchez "Event Hub-compatible name"
3. Notez le nom (généralement "brainoffice" ou un nom généré automatiquement)

# Exemple:
brainoffice
```

Si vous voyez un nom différent comme `iothub-ehub-smartoff-12345-abcdef`, utilisez ce nom exact.

---

### Étape 2 : Mettre à jour les secrets

Ouvrez **PowerShell** et allez dans le répertoire de votre projet :

```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
```

#### Option A : Mise à jour manuelle (recommandée)

Exécutez ces commandes **une par une** en remplaçant les valeurs :

```powershell
# 1?? IoT Hub Connection String (BACKEND)
dotnet user-secrets set "IotHubConnectionString" "HostName=VOTRE_IOT_HUB.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=VOTRE_CLE"

# 2?? Event Hub Connection String (TÉLÉMÉTRIE)
dotnet user-secrets set "IotHubEventHub:ConnectionString" "Endpoint=sb://iothub-ns-XXXXX.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=VOTRE_CLE;EntityPath=brainoffice"

# 3?? Storage Account Connection String
dotnet user-secrets set "IotHubEventHub:BlobConnectionString" "DefaultEndpointsProtocol=https;AccountName=VOTRE_STORAGE;AccountKey=VOTRE_CLE;EndpointSuffix=core.windows.net"

# 4?? Event Hub Name (si différent de "brainoffice")
dotnet user-secrets set "IotHubEventHub:Name" "brainoffice"
```

---

#### Option B : Script automatisé

Créez un fichier `update-azure-secrets.ps1` avec ce contenu :

```powershell
# update-azure-secrets.ps1
# ?? Script de mise à jour des secrets Azure pour SmartOfficeAPP

param(
    [Parameter(Mandatory=$true, HelpMessage="Connection string principale IoT Hub")]
    [string]$IotHubConnectionString,
    
    [Parameter(Mandatory=$true, HelpMessage="Connection string Event Hub intégré")]
    [string]$EventHubConnectionString,
    
    [Parameter(Mandatory=$true, HelpMessage="Connection string Storage Account")]
    [string]$StorageConnectionString,
    
    [Parameter(Mandatory=$false, HelpMessage="Nom de l'Event Hub")]
    [string]$EventHubName = "brainoffice"
)

Write-Host "?? Mise à jour des secrets Azure pour SmartOfficeAPP..." -ForegroundColor Cyan
Write-Host ""

# Naviguer vers le répertoire du projet
$projectPath = "D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP"
Set-Location $projectPath

Write-Host "?? Répertoire: $projectPath" -ForegroundColor Gray
Write-Host ""

# Mise à jour des secrets
Write-Host "1?? Mise à jour IoT Hub Connection String..." -ForegroundColor Yellow
dotnet user-secrets set "IotHubConnectionString" $IotHubConnectionString

Write-Host "2?? Mise à jour Event Hub Connection String..." -ForegroundColor Yellow
dotnet user-secrets set "IotHubEventHub:ConnectionString" $EventHubConnectionString

Write-Host "3?? Mise à jour Storage Account Connection String..." -ForegroundColor Yellow
dotnet user-secrets set "IotHubEventHub:BlobConnectionString" $StorageConnectionString

Write-Host "4?? Mise à jour Event Hub Name..." -ForegroundColor Yellow
dotnet user-secrets set "IotHubEventHub:Name" $EventHubName

Write-Host ""
Write-Host "? Mise à jour terminée avec succès!" -ForegroundColor Green
Write-Host ""
Write-Host "?? Vérification des secrets actuels:" -ForegroundColor Cyan
Write-Host "?????????????????????????????????????????????????????????????" -ForegroundColor Gray
dotnet user-secrets list | Where-Object { $_ -match "IotHub|Blob" }
Write-Host "?????????????????????????????????????????????????????????????" -ForegroundColor Gray
Write-Host ""
Write-Host "?? Vous pouvez maintenant démarrer l'application:" -ForegroundColor Yellow
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
```

**Utilisation du script :**

```powershell
.\update-azure-secrets.ps1 `
    -IotHubConnectionString "HostName=xxx.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=xxx" `
    -EventHubConnectionString "Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=xxx;EntityPath=brainoffice" `
    -StorageConnectionString "DefaultEndpointsProtocol=https;AccountName=xxx;AccountKey=xxx;EndpointSuffix=core.windows.net" `
    -EventHubName "brainoffice"
```

---

### Étape 3 : Vérifier les secrets

Après la mise à jour, vérifiez que tout est correct :

```powershell
dotnet user-secrets list
```

**Vous devriez voir :**

```
? IotHubConnectionString              = HostName=VOTRE_NOUVEAU_IOT_HUB.azure-devices.net;...
? IotHubEventHub:ConnectionString     = Endpoint=sb://iothub-ns-NOUVEAU-...
? IotHubEventHub:Name                 = brainoffice (ou votre nouveau nom)
? IotHubEventHub:BlobConnectionString = DefaultEndpointsProtocol=https;AccountName=NOUVEAU_STORAGE;...
? IotHubEventHub:BlobContainer        = iothub-checkpoints
? ConnectionStrings:DefaultConnection = Host=localhost;... (inchangé)
? AutoMapper:LicenseKey               = eyJhbGc... (inchangé)
```

---

### Étape 4 : Tester la connexion

Démarrez l'application pour vérifier la connexion :

```powershell
dotnet run
```

**Logs attendus si TOUT est OK :**

```
info: SmartOfficeAPP.Services.ServicesImpl.IotHubService[0]
      ? Service IoT Hub initialisé avec succès avec Azure Cloud

info: SmartOfficeAPP.Services.Background.LogIngestionWorker[0]
      ? LogIngestionWorker démarré avec succès

info: SmartOfficeAPP.Services.Background.TelemetrieIngestionWorker[0]
      ? TelemetrieIngestionWorker démarré avec succès
```

---

## ?? Dépannage

### Erreur : "IotHubConnectionString n'est pas configuré"

**Cause** : La connection string est vide ou mal formatée

**Solution** :
```powershell
# Vérifiez le format
dotnet user-secrets list | Select-String "IotHubConnectionString"

# Re-définissez avec la bonne valeur
dotnet user-secrets set "IotHubConnectionString" "VOTRE_CONNECTION_STRING_COMPLETE"
```

---

### Erreur : "Unauthorized" (401)

**Cause** : Clé d'accès incorrecte ou expirée

**Solution** :
1. Retournez dans Azure Portal
2. Vérifiez que la clé `iothubowner` est bien activée
3. Re-copiez la **Primary connection string**
4. Mettez à jour le secret

---

### Erreur : "Entity path is missing"

**Cause** : Le `EntityPath=` manque dans la connection string Event Hub

**Solution** :
```powershell
# Ajoutez EntityPath à la fin
dotnet user-secrets set "IotHubEventHub:ConnectionString" "Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=xxx;EntityPath=brainoffice"
```

---

### Erreur : "Storage account not found"

**Cause** : Connection string Storage Account incorrecte

**Solution** :
1. Vérifiez que le Storage Account existe dans Azure Portal
2. Vérifiez que les containers `iothub-checkpoints` et `iothub-logs-checkpoints` existent
3. Re-copiez la connection string depuis "Access keys"

---

## ?? Checklist de validation

Avant de passer à la suite, vérifiez :

- [ ] ? Connection string IoT Hub récupérée depuis Azure Portal
- [ ] ? Connection string Event Hub récupérée (avec `EntityPath=`)
- [ ] ? Connection string Storage Account récupérée
- [ ] ? Nom de l'Event Hub noté
- [ ] ? Secrets mis à jour avec `dotnet user-secrets set`
- [ ] ? Vérification avec `dotnet user-secrets list`
- [ ] ? Application démarre sans erreur
- [ ] ? Logs confirment la connexion IoT Hub
- [ ] ? Workers de télémétrie démarrent correctement

---

## ?? Récapitulatif

### Ce qui a changé avec votre nouveau compte Azure

| Ancien (compte précédent) | Nouveau (compte actuel) |
|---------------------------|-------------------------|
| IoT Hub : `brainoffice.azure-devices.net` | IoT Hub : `VOTRE_NOUVEAU_HUB.azure-devices.net` |
| Storage : `smartofficecheckpoints` | Storage : `VOTRE_NOUVEAU_STORAGE` |
| Event Hub : `iothub-ns-brainoffic-67368601-...` | Event Hub : `iothub-ns-NOUVEAU-...` |

### Ce qui reste IDENTIQUE (local)

- ? PostgreSQL local : `localhost:5432`
- ? AutoMapper License Key
- ? Certificats SSL Kestrel
- ? Configuration des containers Blob : `iothub-checkpoints`, `iothub-logs-checkpoints`

---

## ?? Liens utiles

- **Guide de création IoT Hub** : `AZURE-IOT-HUB-CREATION-GUIDE.md`
- **Tests API IoT Hub** : `Tests/09-IoT-Hub.http`
- **Déploiement Azure** : `AZURE-DEPLOYMENT-GUIDE.md`

---

## ?? Besoin d'aide ?

Si vous rencontrez des difficultés :

1. **Vérifiez les logs** :
   ```powershell
   dotnet run --verbosity detailed
   ```

2. **Testez la connexion manuellement** :
   ```http
   GET https://localhost:7152/api/iothub/devices
   Authorization: Bearer {{authToken}}
   ```

3. **Validez les ressources Azure** :
   - IoT Hub doit être en tier **Standard** (pas Basic)
   - Storage Account doit avoir les 2 containers créés
   - Les clés d'accès doivent être activées

---

**Dernière mise à jour** : Janvier 2024  
**Pour** : SmartOfficeAPP - Migration nouveau compte Azure  
?? **Bonne chance !**
