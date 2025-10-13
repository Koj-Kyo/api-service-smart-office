# SmartOffice API - Plateforme IoT Complète

## Vue d'ensemble

SmartOffice API est une plateforme complète de gestion IoT qui intègre :
- **Azure IoT Hub** pour la communication bidirectionnelle avec les périphériques
- **SignalR** pour les notifications temps réel
- **Analyse de consommation énergétique** avec statistiques avancées
- **Détection d'anomalies** automatisée
- **Gestion d'incidents** avec alertes automatiques

## Architecture de la solution

```
???????????????????????????????????????????????????????????????????
?                     SMART OFFICE PLATFORM                       ?
???????????????????????????????????????????????????????????????????
?  Frontend (Web/Mobile)                                          ?
?  ??? React/Vue.js Dashboard                                     ?
?  ??? React Native Mobile App                                    ?
?  ??? SignalR Client (Real-time)                                 ?
???????????????????????????????????????????????????????????????????
?  API Layer (.NET 9)                                            ?
?  ??? Authentication & Authorization (JWT + Identity)           ?
?  ??? Controllers (REST APIs)                                   ?
?  ??? SignalR Hubs (Real-time communication)                    ?
?  ??? Background Services (Telemetry ingestion)                 ?
???????????????????????????????????????????????????????????????????
?  Business Logic                                                ?
?  ??? IoT Device Management                                     ?
?  ??? Energy Consumption Analytics                              ?
?  ??? Anomaly Detection                                         ?
?  ??? Incident Management                                       ?
?  ??? User & Access Rights Management                           ?
???????????????????????????????????????????????????????????????????
?  Data Layer                                                    ?
?  ??? PostgreSQL (Main database)                               ?
?  ??? Entity Framework Core (ORM)                              ?
?  ??? Azure Blob Storage (Checkpoints)                         ?
???????????????????????????????????????????????????????????????????
?  Azure IoT Services                                           ?
?  ??? Azure IoT Hub (Device communication)                     ?
?  ??? Event Hubs (Telemetry ingestion)                        ?
?  ??? Device Twins (Device state management)                   ?
???????????????????????????????????????????????????????????????????
?  IoT Devices                                                  ?
?  ??? ESP32/Arduino devices                                    ?
?  ??? Smart plugs & lights                                     ?
?  ??? Sensors (temperature, humidity, energy)                  ?
?  ??? Custom IoT devices                                       ?
???????????????????????????????????????????????????????????????????
```

## Fonctionnalités principales

### ?? Gestion des espaces
- Création et gestion de zones de bureau
- Attribution de périphériques par zone
- Contrôle d'accès granulaire par zone

### ?? Gestion des périphériques IoT
- **Contrôle temps réel** : Allumer/éteindre, régler la luminosité, etc.
- **Surveillance d'état** : Connexion, dernière activité, statut
- **Configuration à distance** : Device Twins, propriétés désirées
- **Commandes directes** : Méthodes IoT Hub pour actions immédiates

### ? Analyse énergétique avancée
- **Statistiques de consommation** par périphérique et zone
- **Tendances** et prédictions de consommation
- **Calcul de coûts** énergétiques avec tarifs personnalisables
- **Top des consommateurs** pour optimisation
- **Comparaisons** entre périodes

### ?? Détection d'anomalies
- **Surveillance automatique** des patterns de consommation
- **Alertes en temps réel** pour consommations anormales
- **Classification des anomalies** par sévérité
- **Historique des incidents** détectés

### ?? Rapports et tableaux de bord
- **Rapports de consommation** détaillés par période
- **Recommandations d'optimisation** automatiques
- **Données horaires** pour graphiques détaillés
- **Export des données** pour analyses externes

### ?? Notifications temps réel
- **SignalR** pour communication bidirectionnelle
- **Notifications utilisateur** personnalisées
- **Alertes d'incidents** automatiques
- **Mises à jour de statut** des périphériques
- **Télémétrie live** pour dashboards

### ?? Gestion d'incidents
- **Création automatique** de tickets d'incident
- **Workflow de résolution** avec statuts
- **Assignation** et suivi des tickets
- **Historique complet** des interventions

### ?? Gestion des utilisateurs
- **Authentification JWT** sécurisée
- **Rôles et permissions** granulaires
- **Gestion des accès** par zone/périphérique
- **Audit complet** des actions utilisateur

## APIs disponibles

### ?? Authentication (`/api/Auth`)
```http
POST /api/Auth/login          # Connexion utilisateur
POST /api/Auth/register       # Création de compte
POST /api/Auth/refresh        # Renouvellement token
```

### ?? Zones (`/api/Zone`)
```http
GET    /api/Zone              # Liste des zones
POST   /api/Zone              # Créer une zone
PUT    /api/Zone/{id}         # Modifier une zone
DELETE /api/Zone/{id}         # Supprimer une zone
```

### ?? Périphériques (`/api/Device`)
```http
GET    /api/Device            # Liste des périphériques
POST   /api/Device            # Ajouter un périphérique
PUT    /api/Device/{id}       # Modifier un périphérique
DELETE /api/Device/{id}       # Supprimer un périphérique
GET    /api/Device/zone/{id}  # Périphériques par zone
```

### ?? Contrôle IoT (`/api/IoTHub`)
```http
POST /api/IoTHub/device/{deviceId}/method/{methodName}           # Méthode directe
PUT  /api/IoTHub/device/{deviceId}/twin/desired                  # Mettre à jour Device Twin
GET  /api/IoTHub/device/{deviceId}/twin                          # Lire Device Twin
POST /api/IoTHub/device/{deviceId}/message                       # Message Cloud-to-Device
POST /api/IoTHub/device/{deviceId}/smart-commands/{command}      # Commandes prédéfinies
POST /api/IoTHub/devices/connection-status                       # Statut de connexion
```

### ? Statistiques énergétiques (`/api/ConsommationStats`)
```http
GET /api/ConsommationStats/peripherique/{id}              # Stats par périphérique
GET /api/ConsommationStats/zone/{id}                      # Stats par zone
GET /api/ConsommationStats/peripherique/{id}/horaire      # Données horaires
GET /api/ConsommationStats/peripherique/{id}/tendances    # Tendances
GET /api/ConsommationStats/peripherique/{id}/anomalies    # Détection anomalies
GET /api/ConsommationStats/rapport                        # Rapport détaillé
GET /api/ConsommationStats/peripherique/{id}/cout         # Calcul coûts
GET /api/ConsommationStats/top-consommateurs              # Top consommateurs
GET /api/ConsommationStats/peripherique/{id}/comparaison  # Comparaison périodes
```

### ?? Télémétrie (`/api/Telemetrie`)
```http
GET /api/Telemetrie/peripherique/{id}  # Télémétrie par périphérique
GET /api/Telemetrie/historical         # Données historiques
```

### ?? Commandes (`/api/Commande`)
```http
POST /api/Commande/send                # Envoyer une commande
```

### ?? Incidents (`/api/IncidentTickets`)
```http
GET    /api/IncidentTickets           # Liste des tickets
POST   /api/IncidentTickets           # Créer un ticket
PUT    /api/IncidentTickets/{id}      # Modifier un ticket
DELETE /api/IncidentTickets/{id}      # Supprimer un ticket
```

### ?? Notifications (`/api/Notification`)
```http
GET  /api/Notification/user/{id}      # Notifications utilisateur
POST /api/Notification/send           # Envoyer notification
PUT  /api/Notification/{id}/read      # Marquer comme lue
```

### ?? Gestion utilisateurs (`/api/User`)
```http
GET  /api/User/profile                # Profil utilisateur
PUT  /api/User/profile                # Modifier profil
POST /api/User/admin/users/{id}/roles # Attribuer rôle (Admin)
```

## Intégration temps réel (SignalR)

### Événements côté client
```javascript
// Notifications utilisateur
connection.on("ReceiveNotification", (notification) => {});

// Mises à jour périphériques
connection.on("DeviceStatusUpdate", (update) => {});

// Télémétrie temps réel
connection.on("TelemetryUpdate", (telemetry) => {});

// Alertes d'incidents
connection.on("IncidentAlert", (alert) => {});

// Résultats de commandes
connection.on("CommandExecutionResult", (result) => {});
```

### Tests SignalR (`/api/SignalRTest`)
```http
POST /api/SignalRTest/test-user-notification      # Test notification utilisateur
POST /api/SignalRTest/test-broadcast              # Test broadcast
POST /api/SignalRTest/test-device-update          # Test mise à jour périphérique
POST /api/SignalRTest/test-telemetry-update       # Test télémétrie
POST /api/SignalRTest/test-incident-alert         # Test alerte incident
```

## Configuration requise

### Variables d'environnement
```bash
# Base de données
ConnectionStrings__DefaultConnection="Host=localhost;Database=SmartOfficeDB;Username=postgres;Password=yourpassword"

# JWT
JwtSettings__Secret="your-256-bit-secret"
JwtSettings__Issuer="SmartOfficeAPI"
JwtSettings__Audience="SmartOfficeClient"

# Azure IoT Hub
IotHubConnectionString="HostName=yourhub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=yourkey"

# Event Hub pour télémétrie
IotHubEventHub__ConnectionString="Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=service;SharedAccessKey=key;EntityPath=hub"
IotHubEventHub__Name="yourhub"
IotHubEventHub__BlobConnectionString="DefaultEndpointsProtocol=https;AccountName=storage;AccountKey=key"
IotHubEventHub__BlobContainer="iothub-checkpoints"
```

### Packages NuGet principaux
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
<PackageReference Include="Microsoft.Azure.Devices" Version="1.39.0" />
<PackageReference Include="Azure.Messaging.EventHubs.Processor" Version="5.9.3" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
```

## Format des données IoT

### Télémétrie (Device-to-Cloud)
```json
{
  "deviceId": "smartoffice-device-001",
  "timestamp": "2024-01-15T14:30:00Z",
  "peripherals": [
    {
      "name": "led_strip_1",
      "type": "lighting",
      "gpio": 2,
      "state": 1,
      "p_mw": 850,        // Puissance instantanée en milliwatts
      "e_mwh": 12450,     // Énergie cumulée en milliwatt-heures
      "brightness": 75,
      "temperature": 23.5
    }
  ]
}
```

### Commandes (Cloud-to-Device)
```json
// Allumer/éteindre
{"state": 1}

// Régler luminosité
{"brightness": 75}

// Régler température
{"temperature": 22.5}
```

## Déploiement

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SmartOfficeAPP.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartOfficeAPP.dll"]
```

### Azure Container Apps
```bash
# Build et push
docker build -t smartoffice-api .
docker tag smartoffice-api youracr.azurecr.io/smartoffice-api:latest
docker push youracr.azurecr.io/smartoffice-api:latest

# Déploiement
az containerapp create \
  --name smartoffice-api \
  --resource-group rg-smartoffice \
  --environment smartoffice-env \
  --image youracr.azurecr.io/smartoffice-api:latest \
  --target-port 80 \
  --ingress external
```

## Tests et validation

### Collection Postman/REST Client
Utilisez les fichiers de test fournis :
- `IoT-Tests.http` - Tests complets IoT et statistiques
- `SignalR-Tests.http` - Tests SignalR
- `api-tests.http` - Tests APIs générales

### Tests de charge
```bash
# Installer k6
curl https://github.com/grafana/k6/releases/download/v0.45.0/k6-v0.45.0-linux-amd64.tar.gz

# Test de charge télémétrie
k6 run --vus 50 --duration 30s telemetry-load-test.js
```

## Monitoring et observabilité

### Application Insights
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Métriques personnalisées
- Nombre de périphériques connectés
- Volume de télémétrie ingérée
- Temps de réponse des commandes IoT
- Anomalies détectées par heure

### Alertes recommandées
- Périphérique déconnecté > 5 minutes
- Consommation > 150% de la moyenne
- Échec de commande > 3 tentatives consécutives
- Latence SignalR > 2 secondes

## Sécurité

### Authentification et autorisation
- JWT avec refresh tokens
- Rôles granulaires (Admin, User, Viewer)
- Permissions par zone/périphérique
- Audit complet des actions

### Communication IoT
- Certificats X.509 pour les périphériques
- Chiffrement TLS 1.2+ obligatoire
- Rotation des clés automatique
- Isolation réseau des périphériques

### Protection des données
- Chiffrement des données sensibles
- Logs anonymisés
- Conformité RGPD
- Sauvegarde chiffrée

## Évolutions futures

### Intelligence artificielle
- Prédiction de pannes équipements
- Optimisation automatique consommation
- Détection d'anomalies par ML
- Suggestions automatiques d'actions

### Intégrations supplémentaires
- Microsoft Teams pour alertes
- Power BI pour reporting avancé
- Azure Digital Twins pour modélisation 3D
- Intégration BIM (Building Information Modeling)

### Mobile et edge
- Application mobile native (Flutter/React Native)
- Edge computing avec Azure IoT Edge
- Synchronisation offline
- Réalité augmentée pour maintenance

## Support et contribution

### Documentation
- [Guide Azure IoT Hub](Azure-IoT-Hub-Documentation.md)
- [Guide SignalR Client](SignalR-Client-Guide-Complete.md)
- [Documentation SignalR](SignalR-Documentation.md)

### Tests
- Tests HTTP complets dans `IoT-Tests.http`
- Collection Postman disponible
- Tests d'intégration automatisés

### Contribution
1. Fork le repository
2. Créer une branche feature
3. Commits avec messages descriptifs
4. Tests passing obligatoires
5. Pull request avec documentation

## Licence

MIT License - Voir LICENSE file pour détails.

---

**SmartOffice API** - Une plateforme IoT complète pour la gestion intelligente des espaces de bureau. ?????