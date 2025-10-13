# Configuration SignalR pour SmartOffice

## Vue d'ensemble

SignalR a été intégré dans l'API SmartOffice pour permettre la communication temps réel entre le serveur et les clients (Web, Mobile). Cette fonctionnalité permet d'envoyer des notifications, des mises à jour de statut d'équipements, des alertes d'incidents et des données de télémétrie en temps réel.

## Architecture

### Composants principaux

1. **NotificationHub** (`/Hubs/NotificationHub.cs`)
   - Hub principal SignalR avec authentification JWT
   - Gestion des groupes d'utilisateurs et de zones
   - Méthodes pour envoyer différents types de messages

2. **ISignalRService & SignalRService** (`/Services/`)
   - Service d'abstraction pour les opérations SignalR
   - Logging intégré et gestion d'erreurs
   - Méthodes typées pour chaque type de message

3. **Configuration** (`Program.cs`)
   - Configuration avancée de SignalR
   - Authentification JWT intégrée
   - CORS configuré pour les clients web/mobile

## Types de messages supportés

### 1. Notifications utilisateur
- **Événement client** : `ReceiveNotification`
- **Utilisation** : Notifications personnalisées pour un utilisateur spécifique
- **Format** : `{ title, message, timestamp }`

### 2. Notifications de zone
- **Événement client** : `ReceiveZoneNotification`
- **Utilisation** : Messages pour tous les utilisateurs d'une zone spécifique
- **Format** : `{ zoneId, title, message, timestamp }`

### 3. Mises à jour de périphériques
- **Événement client** : `DeviceStatusUpdate`
- **Utilisation** : Changements d'état des équipements IoT
- **Format** : `{ peripheriqueId, status, data, timestamp }`

### 4. Télémétrie temps réel
- **Événement client** : `TelemetryUpdate`
- **Utilisation** : Données de capteurs en temps réel
- **Format** : `{ peripheriqueId, data, timestamp }`

### 5. Alertes d'incidents
- **Événement client** : `IncidentAlert`
- **Utilisation** : Notifications urgentes d'incidents
- **Format** : `{ incidentId, title, description, priority, timestamp }`

### 6. Résultats de commandes
- **Événement client** : `CommandExecutionResult`
- **Utilisation** : Feedback sur l'exécution des commandes IoT
- **Format** : `{ commandId, peripheriqueId, status, result, timestamp }`

### 7. Messages broadcast
- **Événement client** : `ReceiveBroadcast`
- **Utilisation** : Messages pour tous les utilisateurs connectés
- **Format** : `{ title, message, timestamp }`

## Intégration dans les services

### TelemetrieService
```csharp
// Envoi automatique des données télémétrie via SignalR
await _signalRService.SendTelemetryUpdateAsync(telemetrie.PeripheriqueLogiqueId, telemetrieDto);
```

### IncidentTicketService
```csharp
// Alerte automatique lors de création d'incident
await _signalRService.SendIncidentAlertAsync(ticket.Id, ticket.Sujet, ticket.Description, priority);
```

### CommandeService
```csharp
// Notification du résultat de commande
await _signalRService.SendCommandExecutionResultAsync(commande.Id, peripheriqueId, status, message);
```

### NotificationService
```csharp
// Envoi de notifications utilisateur
await _signalRService.SendNotificationToUserAsync(userId, title, message);
```

## Configuration côté client

### Installation des packages
```bash
# Pour les clients JavaScript/TypeScript
npm install @microsoft/signalr

# Pour les clients .NET (Xamarin/MAUI)
Install-Package Microsoft.AspNetCore.SignalR.Client
```

### Connexion au hub
```javascript
const connection = new HubConnectionBuilder()
    .withUrl('/notificationhub', {
        accessTokenFactory: () => localStorage.getItem('accessToken')
    })
    .withAutomaticReconnect()
    .build();

await connection.start();
```

### Écoute des événements
```javascript
// Notifications utilisateur
connection.on("ReceiveNotification", (notification) => {
    showUserNotification(notification.title, notification.message);
});

// Mises à jour de périphériques
connection.on("DeviceStatusUpdate", (update) => {
    updateDeviceUI(update.peripheriqueId, update.status, update.data);
});

// Télémétrie temps réel
connection.on("TelemetryUpdate", (telemetry) => {
    updateCharts(telemetry.peripheriqueId, telemetry.data);
});
```

## Authentification et sécurité

### JWT Token
- Toutes les connexions SignalR requièrent un token JWT valide
- Le token peut être fourni via query parameter `access_token`
- Support de l'authentification dans l'en-tête Authorization

### Autorisation
- Les utilisateurs ne reçoivent que les messages pour lesquels ils ont les droits
- Les groupes de zones respectent les autorisations
- Logging de sécurité intégré

## Tests et développement

### Endpoints de test
L'API fournit des endpoints de test dans `SignalRTestController` :
- `/api/SignalRTest/test-user-notification`
- `/api/SignalRTest/test-broadcast`
- `/api/SignalRTest/test-device-update`
- `/api/SignalRTest/test-incident-alert`
- `/api/SignalRTest/test-telemetry-update`
- `/api/SignalRTest/test-command-result`
- `/api/SignalRTest/test-zone-notification`

### Fichiers de test HTTP
Utilisez `SignalR-Tests.http` pour tester les fonctionnalités via REST Client.

## Monitoring et logging

### Logs disponibles
- Connexions/déconnexions d'utilisateurs
- Envoi de messages par type
- Erreurs de transmission
- Statistiques d'utilisation

### Métriques
- Nombre d'utilisateurs connectés
- Fréquence des messages par type
- Latence de transmission
- Taux d'erreur

## Configuration de production

### Recommandations
1. **Scale-out** : Utiliser Azure SignalR Service pour la production
2. **Monitoring** : Intégrer Application Insights
3. **Rate limiting** : Limiter la fréquence des messages
4. **Compression** : Activer la compression des messages
5. **Sticky sessions** : Configurer pour les déploiements multi-instances

### Variables d'environnement
```
SIGNALR_KEEPALIVE_INTERVAL=15
SIGNALR_CLIENT_TIMEOUT=30
SIGNALR_HANDSHAKE_TIMEOUT=15
SIGNALR_MAX_MESSAGE_SIZE=65536
```

## Dépannage

### Problèmes courants
1. **Connexion refusée** : Vérifier le token JWT
2. **Messages non reçus** : Vérifier les groupes et autorisations
3. **Déconnexions fréquentes** : Augmenter les timeouts
4. **Performance** : Optimiser la fréquence des messages

### Debug
Activer les logs détaillés en développement :
```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
```

## Évolutions futures

### Fonctionnalités prévues
1. Support des notifications push mobiles
2. Persistance des messages hors ligne
3. Groupes dynamiques basés sur les rôles
4. Métriques temps réel dans l'admin
5. API GraphQL pour les subscriptions

### Intégrations possibles
- Azure Notification Hubs pour mobile
- Redis Backplane pour le scale-out
- WebRTC pour la communication audio/vidéo
- Chatbot intégré