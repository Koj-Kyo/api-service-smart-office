# ?? Guide Complet : Création d'Azure IoT Hub pour SmartOfficeAPP

## ?? Table des matières
1. [Prérequis](#prérequis)
2. [Création pas à pas](#création-pas-à-pas)
3. [Configuration IoT Hub](#configuration-iot-hub)
4. [Création du Storage Account](#création-du-storage-account)
5. [Récupération des chaînes de connexion](#récupération-des-chaînes-de-connexion)
6. [Configuration du projet](#configuration-du-projet)
7. [Vérification et tests](#vérification-et-tests)
8. [Dépannage](#dépannage)

---

## ?? Prérequis

### Compte Azure
- ? Compte Azure actif (gratuit ou payant)
- ? Accès au portail Azure : https://portal.azure.com
- ? Droits de contributeur sur votre abonnement

### Ressources nécessaires pour SmartOfficeAPP
Votre projet nécessite :
- **Azure IoT Hub** : pour la gestion des appareils IoT
- **Storage Account** : pour les checkpoints Event Hub
- **Resource Group** : pour regrouper les ressources

---

## ?? Création pas à pas

### Étape 1 : Connexion au portail Azure

1. **Ouvrez votre navigateur** et allez sur : https://portal.azure.com
2. **Connectez-vous** avec vos identifiants Azure
3. Une fois connecté, vous verrez le **tableau de bord Azure**

---

### Étape 2 : Créer un Resource Group (si vous n'en avez pas)

> ?? **Pourquoi ?** Pour organiser toutes vos ressources SmartOffice au même endroit

1. Dans la barre de recherche en haut, tapez **"Resource groups"**
2. Cliquez sur **"Resource groups"** dans les résultats
3. Cliquez sur **"+ Create"** (Créer)
4. Remplissez les informations :
   ```
   Subscription     : [Votre abonnement Azure]
   Resource group   : smartoffice-prod-rg
   Region          : East US (ou votre région préférée)
   ```
5. Cliquez sur **"Review + create"**
6. Cliquez sur **"Create"**
7. ? Attendez quelques secondes, votre groupe est créé !

---

### Étape 3 : Créer l'IoT Hub

#### 3.1 - Démarrer la création

1. Dans la barre de recherche en haut, tapez **"IoT Hub"**
2. Cliquez sur **"IoT Hub"** dans les résultats
3. Cliquez sur **"+ Create"** (Créer)

#### 3.2 - Onglet "Basics" (Configuration de base)

Remplissez les informations suivantes :

```
???????????????????????????????????????????????????????????
? BASICS                                                  ?
???????????????????????????????????????????????????????????
? Subscription        : [Votre abonnement]               ?
? Resource group      : smartoffice-prod-rg              ?
? Region             : East US                           ?
? IoT hub name       : smartoffice-prod-iothub           ?
?                      (doit être unique globalement)     ?
???????????????????????????????????????????????????????????
```

> ?? **Important** : Le nom de l'IoT Hub doit être **unique dans tout Azure**. Si "smartoffice-prod-iothub" est pris, essayez :
> - `smartoffice-prod-iothub-[vos-initiales]`
> - `smartoffice-prod-iothub-2024`
> - `brainoffice-prod-iothub` (comme dans votre config)

**Cliquez sur "Next: Networking >"**

#### 3.3 - Onglet "Networking" (Réseau)

```
???????????????????????????????????????????????????????????
? NETWORKING                                              ?
???????????????????????????????????????????????????????????
? Connectivity configuration :                            ?
?   ? Private endpoint                                   ?
?   ?? Public endpoint (all networks)  ? Sélectionnez    ?
?   ? Public endpoint (selected IP ranges)              ?
???????????????????????????????????????????????????????????
```

> ?? **Conseil** : Pour le développement et la soutenance, choisissez **"Public endpoint (all networks)"**. En production réelle, utilisez un endpoint privé.

**Cliquez sur "Next: Management >"**

#### 3.4 - Onglet "Management" (Gestion)

C'est **l'étape la plus importante** pour le coût !

```
???????????????????????????????????????????????????????????
? MANAGEMENT                                              ?
???????????????????????????????????????????????????????????
? Pricing and scale tier :                                ?
?                                                         ?
?   Tier : Standard                      ? IMPORTANT     ?
?   (Ne pas choisir Basic - pas d'Event Hub endpoint)    ?
?                                                         ?
?   Units : 1                                            ?
?   Device-to-cloud partitions : 4                       ?
?                                                         ?
? Role-based access control :                             ?
?   [?] Enable                           ? Cochez       ?
?                                                         ?
? Device-to-cloud messages :                              ?
?   Retention time : 1 day                               ?
?   Partition count : 4                                  ?
?                                                         ?
? Defender for IoT :                                      ?
?   [?] Enable Defender for IoT          ? Optionnel    ?
???????????????????????????????????????????????????????????
```

> ?? **CRUCIAL** : Sélectionnez le tier **"Standard"** (pas Basic) car votre application utilise les Event Hubs intégrés pour la télémétrie !

**Coût estimé** : ~25 USD/mois pour 1 unité Standard

**Cliquez sur "Next: Add-ons >"**

#### 3.5 - Onglet "Add-ons" (Extensions)

Vous pouvez laisser les valeurs par défaut ou configurer :

```
???????????????????????????????????????????????????????????
? ADD-ONS                                                 ?
???????????????????????????????????????????????????????????
? Device Update for IoT Hub :                             ?
?   ? Enable      ? Disable              ? Optionnel   ?
?                                                         ?
? (Pour cette démo/soutenance, vous pouvez désactiver)   ?
???????????????????????????????????????????????????????????
```

**Cliquez sur "Next: Tags >"**

#### 3.6 - Onglet "Tags" (Étiquettes)

Ajoutez des tags pour organiser vos ressources :

```
???????????????????????????????????????????????????????????
? TAGS                                                    ?
???????????????????????????????????????????????????????????
? Name               Value                                ?
? ?????????????????  ??????????????????                  ?
? Application        SmartOffice                          ?
? Environment        Production                           ?
? Project           SmartOfficeAPP                        ?
? Purpose           Academic-IoT-Demo                     ?
???????????????????????????????????????????????????????????
```

**Cliquez sur "Next: Review + create >"**

#### 3.7 - Onglet "Review + create" (Révision)

1. **Vérifiez** toutes vos configurations
2. Assurez-vous que :
   - ? Tier = **Standard** (pas Basic)
   - ? Region = East US
   - ? Public endpoint est activé
3. **Cliquez sur "Create"**

#### 3.8 - Déploiement en cours

```
???????????????????????????????????????????????????????????
?  ? Deployment in progress...                           ?
?                                                         ?
?  Creating IoT Hub...                                    ?
?  This may take 2-5 minutes                              ?
?                                                         ?
?  [????????????????????] 80%                            ?
???????????????????????????????????????????????????????????
```

?? **Durée** : 2 à 5 minutes

4. Une fois terminé, vous verrez : **"Your deployment is complete"**
5. **Cliquez sur "Go to resource"**

? **Félicitations !** Votre IoT Hub est créé !

---

## ?? Configuration IoT Hub

### Étape 4 : Vérifier l'Event Hub intégré

Votre application SmartOfficeAPP utilise l'Event Hub intégré pour recevoir la télémétrie.

1. Dans votre IoT Hub, allez dans le menu de gauche :
   - **"Hub settings"** > **"Built-in endpoints"**

2. Vous verrez les informations de l'Event Hub :
   ```
   Event Hub-compatible endpoint : sb://iothub-ns-smartoffi-xxxxx-xxxxxxx.servicebus.windows.net/
   Event Hub-compatible name     : brainoffice (ou autre nom)
   Partitions                    : 4
   Retention time                : 1 day
   Consumer groups               : $Default
   ```

3. **Notez le nom Event Hub-compatible** (vous en aurez besoin plus tard)

---

## ?? Création du Storage Account

> ?? **Pourquoi ?** Pour stocker les checkpoints des Event Hubs (important pour ne pas perdre les messages)

### Étape 5 : Créer un Storage Account

#### 5.1 - Démarrer la création

1. Dans la barre de recherche, tapez **"Storage accounts"**
2. Cliquez sur **"Storage accounts"**
3. Cliquez sur **"+ Create"**

#### 5.2 - Onglet "Basics"

```
???????????????????????????????????????????????????????????
? BASICS                                                  ?
???????????????????????????????????????????????????????????
? Subscription        : [Votre abonnement]               ?
? Resource group      : smartoffice-prod-rg              ?
? Storage account name: smartofficeprodsa                ?
?                       (lettres minuscules, pas de -)    ?
? Region             : East US                           ?
? Performance        : Standard                          ?
? Redundancy         : LRS (Locally-redundant)           ?
???????????????????????????????????????????????????????????
```

> ?? Le nom doit être **unique**, **en minuscules**, **sans tirets**

#### 5.3 - Autres onglets

- **Advanced** : Laissez les valeurs par défaut
- **Networking** : Sélectionnez **"Public endpoint (all networks)"**
- **Data protection** : Laissez les valeurs par défaut
- **Encryption** : Laissez les valeurs par défaut
- **Tags** : Ajoutez les mêmes tags que l'IoT Hub

#### 5.4 - Créer

1. Cliquez sur **"Review + create"**
2. Cliquez sur **"Create"**
3. Attendez 1-2 minutes
4. Cliquez sur **"Go to resource"**

### Étape 6 : Créer les containers Blob

Votre application a besoin de 2 containers :
- `iothub-checkpoints` : pour les checkpoints de télémétrie
- `iothub-logs-checkpoints` : pour les checkpoints de logs

#### 6.1 - Créer le premier container

1. Dans votre Storage Account, menu de gauche :
   - **"Data storage"** > **"Containers"**
2. Cliquez sur **"+ Container"**
3. Remplissez :
   ```
   Name                : iothub-checkpoints
   Public access level : Private (no anonymous access)
   ```
4. Cliquez sur **"Create"**

#### 6.2 - Créer le second container

1. Cliquez à nouveau sur **"+ Container"**
2. Remplissez :
   ```
   Name                : iothub-logs-checkpoints
   Public access level : Private (no anonymous access)
   ```
3. Cliquez sur **"Create"**

? Vos 2 containers sont créés !

---

## ?? Récupération des chaînes de connexion

### Étape 7 : Récupérer la connection string de l'IoT Hub

#### 7.1 - Connection string principale (pour le backend)

1. Retournez dans votre **IoT Hub** (smartoffice-prod-iothub)
2. Menu de gauche : **"Security settings"** > **"Shared access policies"**
3. Cliquez sur **"iothubowner"** (politique avec tous les droits)
4. Vous verrez plusieurs chaînes de connexion :
   ```
   Primary connection string
   Secondary connection string
   ```
5. **Copiez la "Primary connection string"**
   - Format : `HostName=xxx.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=xxxxx`
6. ? **Sauvegardez-la** dans un fichier texte temporaire

#### 7.2 - Connection string Event Hub (pour la télémétrie)

1. Toujours dans votre IoT Hub
2. Menu : **"Hub settings"** > **"Built-in endpoints"**
3. Copiez la chaîne : **"Event Hub-compatible endpoint"**
   - Format : `Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=xxxxx;EntityPath=brainoffice`
4. ? **Sauvegardez-la** également

### Étape 8 : Récupérer la connection string du Storage Account

1. Allez dans votre **Storage Account** (smartofficeprodsa)
2. Menu de gauche : **"Security + networking"** > **"Access keys"**
3. Sous **"key1"**, cliquez sur **"Show"**
4. **Copiez la "Connection string"**
   - Format : `DefaultEndpointsProtocol=https;AccountName=xxx;AccountKey=xxxxx;EndpointSuffix=core.windows.net`
5. ? **Sauvegardez-la**

---

## ?? Configuration du projet

### Étape 9 : Configurer les User Secrets (SÉCURISÉ)

> ?? **Sécurité** : Ne JAMAIS mettre les connection strings dans `appsettings.json` !

#### 9.1 - Ouvrir un terminal dans votre projet

```bash
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
```

#### 9.2 - Configurer les secrets

Exécutez ces commandes **une par une** :

```bash
# 1. Connection string IoT Hub (BACKEND)
dotnet user-secrets set "IotHubConnectionString" "HostName=smartoffice-prod-iothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=VOTRE_CLE_ICI"

# 2. Connection string Event Hub (TELEMETRIE)
dotnet user-secrets set "IotHubEventHub:ConnectionString" "Endpoint=sb://iothub-ns-smartoffi-xxxxx.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=VOTRE_CLE_ICI;EntityPath=brainoffice"

# 3. Nom de l'Event Hub
dotnet user-secrets set "IotHubEventHub:Name" "brainoffice"

# 4. Connection string Storage Account
dotnet user-secrets set "IotHubEventHub:BlobConnectionString" "DefaultEndpointsProtocol=https;AccountName=smartofficeprodsa;AccountKey=VOTRE_CLE_ICI;EndpointSuffix=core.windows.net"

# 5. Noms des containers Blob
dotnet user-secrets set "IotHubEventHub:BlobContainer" "iothub-checkpoints"
dotnet user-secrets set "IotHubEventHub:BlobContainerLogs" "iothub-logs-checkpoints"
```

> ?? **Remplacez** `VOTRE_CLE_ICI` par les vraies clés copiées à l'étape 7-8 !

#### 9.3 - Vérifier les secrets

```bash
dotnet user-secrets list
```

Vous devriez voir :
```
IotHubConnectionString = HostName=smartoffice-prod-iothub.azure-devices.net;...
IotHubEventHub:ConnectionString = Endpoint=sb://iothub-ns-...
IotHubEventHub:Name = brainoffice
IotHubEventHub:BlobConnectionString = DefaultEndpointsProtocol=https;...
IotHubEventHub:BlobContainer = iothub-checkpoints
IotHubEventHub:BlobContainerLogs = iothub-logs-checkpoints
```

? **Parfait !** Vos secrets sont configurés de manière sécurisée !

---

## ?? Vérification et tests

### Étape 10 : Tester la connexion IoT Hub

#### 10.1 - Démarrer l'application

```bash
dotnet run
```

#### 10.2 - Vérifier les logs de démarrage

Vous devriez voir :
```
info: SmartOfficeAPP.Services.ServicesImpl.IotHubService[0]
      Service IoT Hub initialisé avec succès avec Azure Cloud
      
info: SmartOfficeAPP.Services.Background.LogIngestionWorker[0]
      LogIngestionWorker démarré avec succès
```

? Si vous voyez ces messages = **CONNEXION RÉUSSIE** !

#### 10.3 - Tester avec les fichiers .http

Ouvrez le fichier : `SmartOfficeAPP/Tests/09-IoT-Hub.http`

```http
### 1?? Liste tous les devices IoT Hub
GET {{baseUrl}}/api/iothub/devices
Authorization: Bearer {{authToken}}

### 2?? Créer un device de test
POST {{baseUrl}}/api/iothub/devices
Authorization: Bearer {{authToken}}
Content-Type: application/json

{
  "deviceId": "test-device-001"
}

### 3?? Récupérer le device twin
GET {{baseUrl}}/api/iothub/devices/test-device-001/twin
Authorization: Bearer {{authToken}}
```

#### 10.4 - Vérifier dans le portail Azure

1. Retournez dans votre **IoT Hub**
2. Menu : **"Device management"** > **"Devices"**
3. Vous devriez voir vos devices créés depuis l'API

---

## ?? Dépannage

### Problème : "IotHubConnectionString n'est pas configuré"

**Solution** :
```bash
# Vérifiez que le secret existe
dotnet user-secrets list

# Si absent, ajoutez-le
dotnet user-secrets set "IotHubConnectionString" "VOTRE_CONNECTION_STRING"
```

### Problème : "Unauthorized" ou "401"

**Causes possibles** :
1. Connection string incorrecte
2. Clé d'accès expirée
3. Politique d'accès partagée manquante

**Solution** :
1. Vérifiez la connection string dans le portail Azure
2. Re-copiez la **Primary connection string**
3. Mettez à jour le secret :
   ```bash
   dotnet user-secrets set "IotHubConnectionString" "NOUVELLE_CONNECTION_STRING"
   ```

### Problème : Event Hub ne reçoit pas de télémétrie

**Vérifications** :
1. Le tier IoT Hub est bien **Standard** (pas Basic)
2. Les containers Blob existent
3. La connection string Storage Account est correcte

**Test manuel** :
```bash
# Vérifier les endpoints Event Hub
GET https://smartoffice-prod-iothub.azure-devices.net/
```

### Problème : "Device not found"

**Solution** :
1. Créez d'abord le device dans IoT Hub
2. Utilisez l'endpoint POST pour créer un device
3. Synchronisez avec `DeviceSyncService`

---

## ?? Récapitulatif des ressources créées

| Ressource | Nom | Région | Coût estimé |
|-----------|-----|--------|-------------|
| Resource Group | smartoffice-prod-rg | East US | Gratuit |
| IoT Hub (Standard) | smartoffice-prod-iothub | East US | ~25 USD/mois |
| Storage Account (LRS) | smartofficeprodsa | East US | ~0.50 USD/mois |
| Event Hub (intégré) | brainoffice | East US | Inclus dans IoT Hub |
| Blob Containers (2) | checkpoints | East US | ~0.01 USD/mois |
| **TOTAL MENSUEL** | | | **~25.51 USD/mois** |

> ?? **Conseil** : Pensez à supprimer les ressources après votre soutenance si vous ne les utilisez plus !

---

## ?? Checklist finale

Avant de passer à la suite, vérifiez :

- [ ] ? Resource Group créé
- [ ] ? IoT Hub créé (tier **Standard**)
- [ ] ? Storage Account créé
- [ ] ? 2 containers Blob créés
- [ ] ? Connection strings récupérées
- [ ] ? User Secrets configurés
- [ ] ? Application démarre sans erreur
- [ ] ? Logs confirment la connexion IoT Hub
- [ ] ? Tests API fonctionnent

---

## ?? Ressources supplémentaires

- [Documentation Azure IoT Hub](https://learn.microsoft.com/azure/iot-hub/)
- [Tarification IoT Hub](https://azure.microsoft.com/pricing/details/iot-hub/)
- [Event Hubs intégrés](https://learn.microsoft.com/azure/iot-hub/iot-hub-devguide-messages-read-builtin)
- [Sécurité IoT Hub](https://learn.microsoft.com/azure/iot-hub/iot-hub-security-best-practices)

---

## ?? Pour votre soutenance

### Points à mentionner

1. **Architecture cloud-native** :
   - IoT Hub pour la gestion centralisée des appareils
   - Event Hubs pour le streaming de télémétrie en temps réel
   - Storage Account pour la persistance des checkpoints

2. **Scalabilité** :
   - Tier Standard : jusqu'à 8,000 appareils/unité
   - Partitionnement Event Hub (4 partitions)
   - Traitement asynchrone avec background workers

3. **Sécurité** :
   - User Secrets pour le développement
   - Key Vault pour la production (via Bicep)
   - Authentification par clés partagées

4. **Monitoring** :
   - Application Insights intégré
   - Logs structurés
   - Métriques IoT Hub natives

---

## ? Prochaines étapes

Maintenant que votre IoT Hub est configuré, vous pouvez :

1. **Créer des appareils IoT** via l'API
2. **Simuler la télémétrie** avec un script
3. **Tester les commandes** Device-to-Cloud et Cloud-to-Device
4. **Déployer sur Azure App Service** avec le fichier Bicep

Consultez les guides suivants :
- `SmartOfficeAPP/FIRMWARE-V2-BACKEND-COMPLETE-INTEGRATION.md`
- `SmartOfficeAPP/Azure-Function-Telemetry-Ingestion.md`
- `SmartOfficeAPP/AZURE-DEPLOYMENT-GUIDE.md`

---

**Créé pour SmartOfficeAPP - IoT Hub Configuration Guide**  
?? Dernière mise à jour : 2024  
?? Bonne chance pour votre soutenance !
