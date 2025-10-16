# ? Corrections des Exceptions Appliquées

Ce document résume les **corrections appliquées** pour résoudre les exceptions détectées dans SmartOfficeAPP.

## ?? Exceptions Corrigées

### ? **1. Erreur JSON Parsing (ligne 209) - CORRIGÉ**

**Problème :**
```
System.InvalidOperationException: The requested operation requires an element of type 'Object', but the target element has type 'Array'.
```

**Cause :** Le code tentait de lire un objet JSON mais recevait un array à la racine.

**Solution appliquée :**
- Ajout de la vérification `root.ValueKind == JsonValueKind.Array` pour traiter les arrays directement
- Gestion robuste des 3 formats JSON possibles :
  1. Array de périphériques à la racine
  2. Objet avec propriété "peripherals" (array)
  3. Objet de données directes

**Fichier modifié :** `TelemetrieIngestionWorker.cs` lignes 205-215

---

### ? **2. Event Hub "logs" Consumer Group - DÉSACTIVÉ TEMPORAIREMENT**

**Problème :**
```
Azure.Messaging.EventHubs.EventHubsException(ResourceNotFound): 
The messaging entity 'iothub-ns-brainoffic-67368601-0b12eb886b:eventhub:brainoffice~16383|logs' could not be found.
```

**Solution appliquée :**
- Désactivation temporaire de l'Event Hub dans `appsettings.json`
- `"Enabled": false` et `"LogsEnabled": false`

**Action requise :** Créer le consumer group "logs" dans Azure :

```bash
# Via Azure CLI
az iot hub consumer-group create \
  --name logs \
  --hub-name brainoffice \
  --resource-group votre-resource-group
```

**Ou via Azure Portal :**
1. IoT Hub ? brainoffice ? Built-in endpoints ? Events
2. Consumer groups ? + Add consumer group
3. Nom: `logs` ? Save

---

### ? **3. Connexions Azure Blob Storage - PRÉVENU**

**Problème :**
```
Azure.RequestFailedException dans Azure.Storage.Blobs.dll
```

**Solution appliquée :**
- Event Hub désactivé temporairement évite ces erreurs
- Gestion d'erreurs déjà robuste dans le code avec try/catch spécifiques

---

## ?? Modifications de Code Appliquées

### 1. TelemetrieIngestionWorker.cs
```csharp
// AVANT (ligne 209)
if (root.TryGetProperty("peripherals", out var peripherals) && peripherals.ValueKind == JsonValueKind.Array)

// APRÈS (gestion robuste)
try
{
    // Cas 1: Le JSON root est déjà un array de périphériques
    if (root.ValueKind == JsonValueKind.Array)
    {
        await ProcessPeripheralsDataAsync(root, deviceId, enqueued, telemetrieRepository, periphRepo, signalRService);
    }
    // Cas 2: Le JSON est un objet avec la propriété "peripherals"
    else if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("peripherals", out var peripherals))
    {
        await ProcessPeripheralsDataAsync(peripherals, deviceId, enqueued, telemetrieRepository, periphRepo, signalRService);
    }
    // Cas 3: Objet direct
    else if (root.ValueKind == JsonValueKind.Object)
    {
        await ProcessDirectDataAsync(root, deviceId, enqueued, telemetrieRepository, periphRepo, signalRService);
    }
}
catch (InvalidOperationException ex) when (ex.Message.Contains("Array") || ex.Message.Contains("Object"))
{
    _logger.LogError(ex, "Erreur de parsing JSON pour device {DeviceId}. JSON body: {Body}", deviceId, body);
}
```

### 2. appsettings.json
```json
// AVANT
"IotHubEventHub": {
  "Enabled": true,
  "LogsEnabled": true,
  
// APRÈS
"IotHubEventHub": {
  "Enabled": false,
  "LogsEnabled": false,
```

## ?? Prochaines Étapes

### Obligatoire pour réactiver l'Event Hub :

1. **Créer le consumer group "logs"** dans Azure IoT Hub
2. **Configurer les chaînes de connexion** dans User Secrets :
   ```bash
   dotnet user-secrets set "IotHubEventHub:ConnectionString" "votre-connection-string"
   dotnet user-secrets set "IotHubEventHub:BlobConnectionString" "votre-blob-connection-string"
   ```
3. **Réactiver l'Event Hub** en remettant `"Enabled": true` dans `appsettings.json`

### Optionnel pour monitoring :

1. **Tests de télémétrie** avec différents formats JSON
2. **Monitoring des performances** SignalR
3. **Validation** des checkpoints Azure Blob

## ?? Résultats Attendus

Après ces corrections :
- ? **Plus d'erreurs JSON parsing** (ligne 209)
- ? **Plus d'erreurs Event Hub consumer group**
- ? **Application démarre sans exceptions critiques**
- ? **SignalR fonctionne normalement**
- ? **API REST disponible pour tests manuels**

Les services de télémétrie en temps réel pourront être réactivés une fois la configuration Azure complétée.