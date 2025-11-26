# ?? Solution : Erreur Event Hub "logs" - SmartOfficeAPP

## ?? Problème rencontré

```
Azure.Messaging.EventHubs.EventHubsException(ResourceNotFound): 
The messaging entity 'iothub-ns-brainoffic-68950721-964970e2de:eventhub:brainoffice-prod-iothub~16383|logs' 
could not be found.
```

### Cause

Le `LogIngestionWorker` tente de se connecter à un **consumer group "logs"** qui n'existe pas dans votre IoT Hub. Par défaut, IoT Hub n'a que le consumer group **`$Default`**.

---

## ? Solution appliquée (RECOMMANDÉE)

### Option 1 : Désactiver le LogIngestionWorker (appliquée)

J'ai mis à jour `appsettings.json` :

```json
"IotHubEventHub": {
  "Enabled": true,
  "LogsEnabled": false, // ?? Désactivé
  "ConnectionString": "",
  "Name": "brainoffice",
  "BlobConnectionString": "",
  "BlobContainer": "iothub-checkpoints",
  "BlobContainerLogs": "iothub-logs-checkpoints"
}
```

**Résultat :**
- ? Le `LogIngestionWorker` ne démarre plus
- ? Pas d'erreur au démarrage
- ? Le `TelemetrieIngestionWorker` (télémétrie) continue de fonctionner normalement
- ? Les logs des devices ESP32 ne sont **PAS nécessaires** pour le moment

---

## ?? Alternatives (si vous voulez les logs plus tard)

### Option 2 : Créer le consumer group "logs" dans IoT Hub

Si vous voulez activer les logs structurés depuis vos devices ESP32 :

#### Étape 1 : Créer le consumer group dans Azure Portal

1. Allez dans votre **IoT Hub** (brainoffice-prod-iothub)
2. Menu : **"Hub settings"** > **"Built-in endpoints"**
3. Section **"Consumer groups"**
4. Cliquez sur **"+ Add consumer group"**
5. Nom : `logs`
6. Cliquez sur **"Save"**

#### Étape 2 : Réactiver LogsEnabled

```json
"IotHubEventHub": {
  "LogsEnabled": true
}
```

#### Étape 3 : Modifier le firmware ESP32

Le firmware doit envoyer des messages avec le property `type = "log"` :

```cpp
// Dans le firmware ESP32
void sendLog(const char* level, const char* source, const char* message) {
  DynamicJsonDocument doc(256);
  doc["level"] = level;
  doc["source"] = source;
  doc["message"] = message;
  doc["timestamp"] = getISOTimestamp();
  
  String payload;
  serializeJson(doc, payload);
  
  // Ajouter la property "type" pour différencier logs et télémétrie
  Message msg(payload.c_str());
  msg.addProperty("type", "log");
  
  deviceClient.sendEventAsync(msg);
}
```

---

## ?? Différence entre Télémétrie et Logs

| Feature | TelemetrieIngestionWorker | LogIngestionWorker |
|---------|--------------------------|-------------------|
| **Consumer Group** | `$Default` | `logs` |
| **Type de données** | Métriques, états devices | Logs applicatifs |
| **Obligatoire** | ? OUI | ?? NON |
| **Property type** | `type = "telemetry"` (optionnel) | `type = "log"` (obligatoire) |
| **Utilité** | Monitoring temps réel | Debugging, audit |

---

## ?? Recommandation pour votre soutenance

### Configuration ACTUELLE (recommandée)

```json
"IotHubEventHub": {
  "Enabled": true,          // ? Télémétrie activée
  "LogsEnabled": false      // ?? Logs désactivés (pas nécessaires)
}
```

**Avantages :**
- ? Pas d'erreur au démarrage
- ? Télémétrie fonctionne (température, humidité, consommation)
- ? Commandes IoT Hub fonctionnent
- ? Device Twin synchronisé
- ? Suffisant pour la démo

**Ce qui fonctionne déjà :**
- Tableau de bord avec statistiques temps réel
- Contrôle des LEDs, servo, buzzer
- Alertes fumée
- Historique des commandes
- Consommation énergétique

---

## ?? Déploiement sur Azure

### Pour le déploiement manuel sur Azure Portal

Vous avez choisi de déployer **manuellement** sur Azure Portal avec PostgreSQL sur Railway. Voici les configurations à vérifier :

### 1?? Configuration App Service (App Settings)

Ajoutez ces variables d'environnement dans Azure Portal :

```
IotHubConnectionString              = HostName=xxx.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=xxx
IotHubEventHub__ConnectionString    = Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=xxx;EntityPath=brainoffice
IotHubEventHub__BlobConnectionString = DefaultEndpointsProtocol=https;AccountName=xxx;AccountKey=xxx;EndpointSuffix=core.windows.net
IotHubEventHub__Name                = brainoffice
IotHubEventHub__Enabled             = true
IotHubEventHub__LogsEnabled         = false
IotHubEventHub__BlobContainer       = iothub-checkpoints
IotHubEventHub__BlobContainerLogs   = iothub-logs-checkpoints

ConnectionStrings__DefaultConnection = Host=xxx.railway.app;Port=5432;Database=railway;Username=postgres;Password=xxx;SSL Mode=Require

JwtSettings__Secret                 = VOTRE_JWT_SECRET
JwtSettings__Issuer                 = SmartOfficeAPI
JwtSettings__Audience               = SmartOfficeClient

DeviceSync__IntervalMinutes         = 5
DeviceSync__Enabled                 = true
DeviceSync__MaxParallelDevices      = 5

ASPNETCORE_ENVIRONMENT              = Production
```

### 2?? Configuration Railway PostgreSQL

Dans Railway, récupérez la connection string :

```
postgresql://postgres:PASSWORD@monorail.proxy.rlwy.net:PORT/railway
```

Convertissez-la au format .NET :

```
Host=monorail.proxy.rlwy.net;Port=PORT;Database=railway;Username=postgres;Password=PASSWORD;SSL Mode=Require
```

---

## ? Vérification après démarrage

Après avoir mis à jour `appsettings.json`, redémarrez le backend :

```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
dotnet run
```

**Logs attendus (sans erreur) :**

```
info: SmartOfficeAPP.Services.ServicesImpl.IotHubService[0]
      ? Service IoT Hub initialisé avec succès avec Azure Cloud

info: SmartOfficeAPP.Services.Background.TelemetrieIngestionWorker[0]
      ? Service d'ingestion télémétrie Event Hub initialisé avec succès.
      ? Démarrage de l'ingestion télémétrie Event Hub...

info: SmartOfficeAPP.Services.Background.LogIngestionWorker[0]
      ? Service d'ingestion logs Event Hub désactivé par configuration.
```

?? **Vous ne devriez PLUS voir l'erreur** `The messaging entity '...|logs' could not be found.`

---

## ?? Fichiers modifiés

| Fichier | Modification | Raison |
|---------|-------------|--------|
| `appsettings.json` | `"LogsEnabled": false` | Désactiver LogIngestionWorker |

---

## ?? Pour votre soutenance

### Points à mentionner

1. **Télémétrie temps réel activée**
   - Consumer group `$Default`
   - Réception de température, humidité, consommation

2. **Logs désactivés temporairement**
   - Pas nécessaires pour la démo
   - Peuvent être activés plus tard avec un consumer group dédié

3. **Architecture modulaire**
   - `TelemetrieIngestionWorker` : données métriques
   - `LogIngestionWorker` : logs structurés (désactivé)
   - Chacun avec son consumer group et ses checkpoints

4. **Évolutivité**
   - Facile d'activer les logs en créant le consumer group
   - Firmware prêt à envoyer des logs structurés

---

## ?? Ressources

- **Azure IoT Hub Consumer Groups** : https://learn.microsoft.com/azure/iot-hub/iot-hub-event-hubs-consumer-group
- **Event Hubs Best Practices** : https://learn.microsoft.com/azure/event-hubs/event-hubs-programming-guide
- **Railway PostgreSQL** : https://docs.railway.app/databases/postgresql

---

**Problème résolu ! Le backend devrait maintenant démarrer sans erreur.** ??

Voulez-vous que je crée aussi un guide pour le déploiement manuel sur Azure Portal avec PostgreSQL Railway ?
