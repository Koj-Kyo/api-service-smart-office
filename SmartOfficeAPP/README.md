# ?? SmartOffice API - Système de Gestion Intelligente de Bureau

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4?logo=microsoft-azure)](https://azure.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?logo=postgresql)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis)](https://redis.io/)
[![CI/CD](https://github.com/Koj-Kyo/api-service-smart-office/actions/workflows/azure-deploy.yml/badge.svg)](https://github.com/Koj-Kyo/api-service-smart-office/actions/workflows/azure-deploy.yml)
[![License](https://img.shields.io/badge/License-Academic-green)](LICENSE)

## ?? Description

SmartOffice API est une plateforme complète de gestion intelligente d'espaces de bureaux intégrant :
- ??? Gestion IoT (température, humidité, éclairage, RFID)
- ?? Gestion des utilisateurs et droits d'accès
- ?? Analytics et statistiques en temps réel
- ?? Notifications temps réel via SignalR
- ?? Système de tickets d'incidents
- ?? Authentification JWT sécurisée

## ??? Architecture Technique

### Stack Technologique

| Couche | Technologies |
|--------|--------------|
| **Backend** | .NET 9, ASP.NET Core Web API |
| **Base de données** | PostgreSQL 16 (Flexible Server) |
| **Cache** | Redis (Azure Cache for Redis) |
| **IoT** | Azure IoT Hub, Event Hubs |
| **Temps réel** | SignalR |
| **Cloud** | Azure (App Service, Key Vault, Application Insights) |
| **Infrastructure** | Bicep (IaC), Azure Developer CLI (azd) |
| **CI/CD** | GitHub Actions, Azure DevOps |

### Architecture Azure

```
???????????????????????????????????????????????????????????????
?                     Azure Cloud (East US)                    ?
?                                                               ?
?  ????????????????      ???????????????????                  ?
?  ?  Azure App   ????????  Key Vault      ?                  ?
?  ?  Service     ?      ?  (Secrets)      ?                  ?
?  ?  (.NET 9)    ?      ???????????????????                  ?
?  ????????????????                                            ?
?         ?                                                     ?
?         ???????????? ????????????????????                   ?
?         ?            ?  PostgreSQL      ?                   ?
?         ?            ?  Flexible Server ?                   ?
?         ?            ????????????????????                   ?
?         ?                                                     ?
?         ???????????? ????????????????????                   ?
?         ?            ?  Redis Cache     ?                   ?
?         ?            ?  (Distributed)   ?                   ?
?         ?            ????????????????????                   ?
?         ?                                                     ?
?         ???????????? ????????????????????                   ?
?         ?            ?  IoT Hub         ?                   ?
?         ?            ?  (brainoffice)   ?                   ?
?         ?            ????????????????????                   ?
?         ?                                                     ?
?         ???????????? ????????????????????                   ?
?                      ?  Application     ?                   ?
?                      ?  Insights        ?                   ?
?                      ????????????????????                   ?
???????????????????????????????????????????????????????????????
```

## ?? Déploiement Rapide

### Option 1 : Déploiement CI/CD avec GitHub Actions (Recommandé)

**Déploiement automatique à chaque push sur `main`** ??

```powershell
# 1. Configurer le Service Principal et les secrets GitHub
.\setup-github-actions.ps1

# 2. Commit et push
git add .
git commit -m "feat: Add CI/CD pipeline"
git push origin main

# 3. Le déploiement se fait automatiquement ! ?
```

**Résultat** : À chaque push, votre API est automatiquement :
- ? Buildée et testée
- ? Déployée sur Azure
- ? Migrée (base de données)
- ? Vérifiée (health check)

?? **[Guide CI/CD Complet](GITHUB-ACTIONS-CICD-COMPLETE-GUIDE.md)** | ?? **[Quick Start CI/CD](GITHUB-ACTIONS-QUICK-START.md)**

### Option 2 : Déploiement Manuel avec azd

```powershell
# 1. Cloner le repository
git clone https://github.com/Koj-Kyo/api-service-smart-office.git
cd api-service-smart-office/SmartOfficeAPP

# 2. Configurer les secrets
Copy-Item .env.example .env
# Éditer .env avec vos valeurs

# 3. Déployer !
azd up
```

**C'est tout !** Votre API sera déployée sur Azure en ~5 minutes.

?? **[Guide de Déploiement Complet](AZURE-DEPLOYMENT-COMPLETE-GUIDE.md)** | ?? **[Quick Start](DEPLOYMENT-QUICK-START.md)**

## ?? Documentation

### Guides de Déploiement

#### CI/CD GitHub Actions
- [?? Guide CI/CD Complet](GITHUB-ACTIONS-CICD-COMPLETE-GUIDE.md)
- [? Quick Start CI/CD (5 min)](GITHUB-ACTIONS-QUICK-START.md)
- [?? Script Setup Automatique](setup-github-actions.ps1)

#### Déploiement Manuel (azd)
- [?? Quick Start (5 min)](DEPLOYMENT-QUICK-START.md)
- [?? Guide Complet](AZURE-DEPLOYMENT-COMPLETE-GUIDE.md)
- [?? Commandes Utiles](AZURE-COMMANDS-CHEATSHEET.md)
- [? Checklist](DEPLOYMENT-CHECKLIST.md)

### Documentation Fonctionnelle

- [?? API Documentation Complète](SmartOfficeAPP/API-COMPLETE-DOCUMENTATION.md)
- [?? Système d'Incidents](SmartOfficeAPP/FRONTEND-INCIDENTS-INDEX.md)
- [?? Dashboard Analytics](SmartOfficeAPP/DASHBOARD-INDEX.md)
- [?? Droits d'Accès](SmartOfficeAPP/ACCESS-RIGHTS-INDEX.md)
- [??? Backend Implementation](SmartOfficeAPP/IMPLEMENTATION-BACKEND-COMPLETE.md)

### Documentation Technique

- [?? Tests API](SmartOfficeAPP/Tests/README.md)
- [?? SignalR Guide](SmartOfficeAPP/SignalR-Client-Guide.md)
- [?? Redis Configuration](SmartOfficeAPP/REDIS-QUICK-REFERENCE.md)
- [?? Azure IoT Production](SmartOfficeAPP/Azure-IoT-Production-Ready.md)

## ?? Fonctionnalités Principales

### ?? Authentification & Autorisation
- ? JWT Bearer tokens
- ? Role-based access control (RBAC)
- ? Resource-level authorization
- ? Azure AD integration ready

### ??? Gestion IoT
- ? Connexion Azure IoT Hub
- ? Gestion des appareils (ESP32, Raspberry Pi)
- ? Télémétrie en temps réel (température, humidité, CO2)
- ? Commandes à distance (relais, RGB, servomoteurs)
- ? RFID badge management
- ? Détection fumée et alarmes

### ?? Analytics & Monitoring
- ? Dashboard statistiques temps réel
- ? Consommation énergétique
- ? Activity logs & audit trail
- ? Application Insights integration
- ? Health checks (`/health/ready`, `/health/live`)

### ?? Notifications Temps Réel
- ? SignalR WebSocket connections
- ? Push notifications
- ? Alertes critiques (fumée, température)
- ? Support mobile (MAUI, React Native)

### ?? Gestion d'Incidents
- ? Création/modification tickets
- ? Workflow de résolution
- ? Statistiques et rapports
- ? Assignation automatique

### ?? Zones & Périphériques
- ? Gestion hiérarchique des zones
- ? Configuration périphériques logiques
- ? Mapping Device ? Zone
- ? Contrôle d'accès par zone

## ??? Développement Local

### Setup Environnement

```powershell
# 1. Cloner le repo
git clone https://github.com/Koj-Kyo/api-service-smart-office.git
cd api-service-smart-office/SmartOfficeAPP

# 2. Restaurer les packages
dotnet restore SmartOfficeAPP/SmartOfficeAPP.csproj

# 3. Configurer la base de données
# Éditer appsettings.Development.json avec votre PostgreSQL local

# 4. Appliquer les migrations
cd SmartOfficeAPP
dotnet ef database update

# 5. Lancer l'API
dotnet run

# L'API sera accessible sur :
# https://localhost:7152
# http://localhost:5249
```

### Swagger UI

Ouvrir dans le navigateur :
```
https://localhost:7152/swagger
```

### Tests

```powershell
# Tous les tests
dotnet test

# Tests d'une catégorie spécifique
dotnet test --filter Category=Integration
```

## ?? Endpoints API Principaux

### Authentication
- `POST /api/Auth/register` - Créer un compte
- `POST /api/Auth/login` - Se connecter
- `GET /api/Auth/profile` - Profil utilisateur

### IoT Management
- `GET /api/Device` - Liste des devices
- `POST /api/Commande/send` - Envoyer une commande
- `GET /api/Telemetrie/latest` - Dernière télémétrie

### Dashboard
- `GET /api/Dashboard/stats` - Statistiques globales
- `GET /api/Dashboard/energy-consumption` - Consommation
- `GET /api/Dashboard/device-activity` - Activité devices

### Incidents
- `GET /api/IncidentTickets` - Liste tickets
- `POST /api/IncidentTickets` - Créer ticket
- `PUT /api/IncidentTickets/{id}` - Modifier ticket

Voir la [documentation complète des endpoints](SmartOfficeAPP/API-COMPLETE-DOCUMENTATION.md).

## ?? Sécurité

### Mesures de sécurité implémentées

? **Secrets Management** : Azure Key Vault avec RBAC  
? **Identité Managée** : Pas de credentials en dur  
? **HTTPS Obligatoire** : Certificat SSL automatique  
? **Chiffrement** : TLS 1.2 minimum sur tous les services  
? **Firewall** : PostgreSQL avec règles d'accès  
? **JWT** : Tokens signés avec validation stricte  
? **CORS** : Configuration restrictive  
? **Rate Limiting** : Protection contre DDoS  

### Bonnes pratiques

- ?? Ne jamais committer `.env` ou `appsettings.Development.json`
- ?? Utiliser Azure Key Vault pour les secrets en production
- ?? Activer les règles firewall PostgreSQL restrictives
- ?? Renouveler régulièrement les JWT secrets
- ?? Monitorer les logs Application Insights

## ?? Coûts Azure (Estimation)

| Service | SKU | Coût mensuel |
|---------|-----|--------------|
| App Service Plan | Standard S1 | ~70 USD |
| PostgreSQL Flexible | Burstable B2s | ~30 USD |
| Redis Cache | Basic C0 | ~17 USD |
| Key Vault | Standard | ~1 USD |
| Application Insights | Pay-as-you-go | ~5 USD |
| **Total** | | **~123 USD/mois** |

?? **Astuce** : Arrêtez les services après la soutenance pour économiser.

```powershell
# Arrêter l'App Service
az webapp stop --name smartoffice-prod-api --resource-group smartoffice-prod-rg
```

## ?? Pour la Soutenance

### Démonstration Live

```powershell
# 1. Démarrer l'API
az webapp start --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# 2. Ouvrir Swagger UI
$url = azd env get-value AZURE_WEBAPP_URL
Start-Process "$url/swagger"

# 3. Monitorer en temps réel
azd monitor

# 4. Voir les logs live
azd monitor --logs
```

### Points clés à présenter

? **Architecture Cloud-Native** : PaaS managé, scalable  
? **Infrastructure as Code** : Reproductible, versionné  
? **Sécurité** : Key Vault, Managed Identity, HTTPS  
? **Monitoring** : Application Insights, Health checks  
? **Performance** : Redis cache, Output cache  
? **IoT Integration** : Azure IoT Hub, télémétrie temps réel  
? **CI/CD** : GitHub Actions / Azure DevOps  

### Captures d'écran recommandées

1. Portail Azure - Vue Resource Group
2. Application Insights - Live Metrics
3. Swagger UI - Endpoints disponibles
4. SignalR - Notifications temps réel
5. IoT Hub - Devices connectés

## ?? Contribution

Ce projet est développé dans le cadre d'un mémoire académique.

## ?? License

Academic use only - Non-commercial

## ????? Auteur

Développé pour un projet de mémoire de Master en Systèmes Informatiques.

## ?? Support

Pour toute question sur le déploiement :
- ?? [Guide de Déploiement Complet](AZURE-DEPLOYMENT-COMPLETE-GUIDE.md)
- ?? [Quick Start](DEPLOYMENT-QUICK-START.md)
- ?? Email : [votre-email@example.com]

---

**?? Prêt à déployer votre API SmartOffice sur Azure !**

```powershell
azd up
```
