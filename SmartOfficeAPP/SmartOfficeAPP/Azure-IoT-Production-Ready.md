# ?? Smart Office API - Configuration Azure IoT Hub Production

## Vue d'ensemble

L'application Smart Office API est maintenant configurée pour fonctionner avec Azure IoT Hub en mode production. **Aucune simulation** - tous les appels interagissent directement avec votre infrastructure Azure.

## ? Corrections apportées

### 1. **Service IoT Hub refactorisé**
- ? **Supprimé** : Mode simulation
- ? **Ajouté** : Connexion directe obligatoire à Azure IoT Hub
- ? **Ajouté** : Messages d'erreur clairs si mal configuré
- ? **Ajouté** : Logging détaillé des interactions Azure

### 2. **Configuration via User Secrets**
- ? Configuration sécurisée hors du code source
- ? Séparation dev/prod naturelle
- ? Pas de risque de commit de secrets

### 3. **Gestion d'erreurs robuste**
- ? Exceptions claires en cas de mauvaise configuration
- ? Logging détaillé pour le debugging
- ? Gestion des devices inexistants

## ?? Configuration requise

### 1. Configurer User Secrets

**Option A : Script automatique**
```powershell
.\setup-secrets.ps1
```

**Option B : Configuration manuelle**
```bash
# Initialiser
dotnet user-secrets init

# Azure IoT Hub (OBLIGATOIRE)
dotnet user-secrets set "IotHubConnectionString" "HostName=votre-iothub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=votre-clé"

# Base de données
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=SmartOfficeDB;Username=postgres;Password=votre-mot-de-passe"
```

### 2. Permissions Azure requises

Dans votre IoT Hub, la policy doit avoir :
- ? **Service Connect** (commandes cloud-to-device)
- ? **Registry Read** (lecture des devices)
- ? **Registry Write** (création/modification devices)

## ?? Tests avec Azure

### Tests disponibles
1. **`IoT-Hub-Fix-Test.http`** - Tests spécifiques Azure
2. **`09-IoT-Hub.http`** - Suite complète IoT Hub
3. **`10-Integration-Tests.http`** - Tests d'intégration

### Fonctionnalités testées
- ? Création de devices dans Azure IoT Hub
- ? Méthodes directes vers devices réels
- ? Mise à jour des Device Twins
- ? Messages Cloud-to-Device
- ? Statuts de connexion en temps réel

## ?? Monitoring et Logging

### Logs détaillés activés pour :
- `Microsoft.Azure.Devices` - SDK Azure IoT
- `SmartOfficeAPP.Services.ServicesImpl.IotHubService` - Notre service
- `Microsoft.Azure.Devices.Client` - Client SDK

### Exemples de logs attendus :
```
[INFO] Service IoT Hub initialisé avec succès avec Azure Cloud
[INFO] Invocation de la méthode setState sur le device smartoffice-device-001
[INFO] Méthode setState sur smartoffice-device-001: Status=200, Response={"result":"success"}
[INFO] Device smartoffice-device-001 créé avec succès dans Azure IoT Hub
```

## ?? Workflow DevOps

### Développement Local
```bash
# 1. Configurer User Secrets
dotnet user-secrets set "IotHubConnectionString" "votre-dev-iothub"

# 2. Démarrer l'app
dotnet run

# 3. Tester avec vrais devices Azure
# Les tests créent/modifient des devices dans votre IoT Hub
```

### Production
- ? Variables d'environnement dans Azure App Service
- ? Azure Key Vault pour les secrets sensibles
- ? IoT Hub de production séparé

## ?? Sécurité

### ? Bonnes pratiques appliquées
- Secrets hors du code source
- Validation des chaînes de connexion
- Logging sans exposition des secrets
- Gestion d'erreurs sans leak d'info

### ?? Points d'attention
- Ne pas commiter les User Secrets
- Utiliser des IoT Hubs séparés dev/prod
- Surveiller les quotas Azure IoT Hub
- Gérer les devices de test séparément

## ?? Fonctionnalités IoT Hub utilisées

### Device Management
- ? Création/suppression de devices
- ? Activation/désactivation
- ? Statuts de connexion

### Communication
- ? Méthodes directes (Device Methods)
- ? Messages Cloud-to-Device
- ? Device Twins (desired/reported properties)

### Monitoring
- ? États de connexion
- ? Dernière activité
- ? Versions des twins

## ?? Prochaines étapes

1. **Configurer vos User Secrets** avec votre IoT Hub Azure
2. **Tester avec des devices réels** via les fichiers HTTP
3. **Intégrer avec vos devices IoT** physiques
4. **Monitorer les performances** Azure IoT Hub

## ?? Support

Si vous rencontrez des problèmes :
1. Vérifiez les logs de l'application
2. Validez votre chaîne de connexion Azure
3. Confirmez les permissions IoT Hub
4. Testez la connectivité Azure

---

**?? Votre Smart Office API est maintenant prête pour Azure IoT Hub en production !**