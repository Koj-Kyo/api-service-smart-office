# ?? Correction des Exceptions - SmartOfficeAPP

## ?? Problèmes Identifiés dans les Logs

D'après l'analyse des logs de débogage, voici les **3 problèmes majeurs** à résoudre :

### ? Problème 1: Consumer Group "logs" introuvable
```
Azure.Messaging.EventHubs.EventHubsException(ResourceNotFound): 
The messaging entity 'iothub-ns-brainoffic-67368601-0b12eb886b:eventhub:brainoffice~16383|logs' could not be found.
```

### ? Problème 2: Erreur JSON Parsing Télémétrie (Array vs Object)
```
System.InvalidOperationException: The requested operation requires an element of type 'Object', 
but the target element has type 'Array'.
at SmartOfficeAPP.Services.Background.TelemetrieIngestionWorker.ProcessEventHandler line 209
```

### ? Problème 3: Connexion Azure Blob Storage
```
Azure.RequestFailedException dans Azure.Storage.Blobs.dll
```

---

## ? Corrections à Appliquer

### ?? **CORRECTION 1: Créer le Consumer Group "logs"**

#### Via Azure CLI:
```bash
# Remplacer par vos valeurs réelles
RESOURCE_GROUP="your-resource-group-name"
IOT_HUB_NAME="brainoffice"  # ou votre nom exact

# Créer le consumer group logs
az iot hub consumer-group create \
  --name logs \
  --hub-name $IOT_HUB_NAME \
  --resource-group $RESOURCE_GROUP
```

#### Via Azure Portal:
1. Aller dans Azure Portal ? IoT Hub ? **brainoffice**
2. **Built-in endpoints** ? **Events**
3. **Consumer groups** ? **+ Add consumer group**
4. Nom: `logs`
5. **Save**

---

### ?? **CORRECTION 2: Fixer le Parser JSON (TelemetrieIngestionWorker)**

Le problème est à la ligne 209 du `TelemetrieIngestionWorker.cs`. Le code tente de lire un objet mais reçoit un array.
