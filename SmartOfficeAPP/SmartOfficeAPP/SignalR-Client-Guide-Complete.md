# Guide d'utilisation SignalR pour SmartOffice

## Connexion au Hub

### JavaScript/TypeScript
```javascript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
    .withUrl('/notificationhub', {
        accessTokenFactory: () => localStorage.getItem('accessToken')
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();

// Démarrer la connexion
await connection.start();
console.log("Connexion SignalR établie");
```

### C# (Xamarin/MAUI)
```csharp
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("https://yourapi.com/notificationhub", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(accessToken);
    })
    .WithAutomaticReconnect()
    .Build();

await connection.StartAsync();
```

### React Native
```javascript
import { HubConnectionBuilder } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
    .withUrl('https://yourapi.com/notificationhub', {
        accessTokenFactory: () => getStoredToken()
    })
    .withAutomaticReconnect()
    .build();

await connection.start();
```

## Écoute des événements

### Notifications utilisateur
```javascript
connection.on("ReceiveNotification", (notification) => {
    console.log("Notification reçue:", notification);
    // notification = { title, message, timestamp }
    
    // Afficher la notification dans l'UI
    showNotification(notification.title, notification.message);
});
```

### Notifications de zone
```javascript
connection.on("ReceiveZoneNotification", (notification) => {
    console.log("Notification de zone reçue:", notification);
    // notification = { zoneId, title, message, timestamp }
    
    // Traiter la notification spécifique à la zone
    handleZoneNotification(notification);
});
```

### Mises à jour de périphériques
```javascript
connection.on("DeviceStatusUpdate", (update) => {
    console.log("Mise à jour périphérique:", update);
    // update = { peripheriqueId, status, data, timestamp }
    
    // Mettre à jour l'état du périphérique dans l'UI
    updateDeviceStatus(update.peripheriqueId, update.status, update.data);
});
```

### Alertes d'incident
```javascript
connection.on("IncidentAlert", (alert) => {
    console.log("Alerte incident:", alert);
    // alert = { incidentId, title, description, priority, timestamp }
    
    // Afficher une alerte urgente selon la priorité
    if (alert.priority === "High") {
        showUrgentAlert(alert);
    } else {
        showNormalAlert(alert);
    }
});
```

### Télémétrie temps réel
```javascript
connection.on("TelemetryUpdate", (telemetry) => {
    console.log("Données télémétrie:", telemetry);
    // telemetry = { peripheriqueId, data, timestamp }
    
    // Mettre à jour les graphiques/dashboards
    updateTelemetryCharts(telemetry.peripheriqueId, telemetry.data);
});
```

### Résultats de commandes
```javascript
connection.on("CommandExecutionResult", (result) => {
    console.log("Résultat de commande:", result);
    // result = { commandId, peripheriqueId, status, result, timestamp }
    
    // Notifier l'utilisateur du résultat de sa commande
    notifyCommandResult(result.commandId, result.status, result.result);
});
```

### Notifications broadcast
```javascript
connection.on("ReceiveBroadcast", (broadcast) => {
    console.log("Message broadcast:", broadcast);
    // broadcast = { title, message, timestamp }
    
    // Afficher le message à tous les utilisateurs
    showBroadcastMessage(broadcast.title, broadcast.message);
});
```

## Envoi de messages depuis le client

### Rejoindre un groupe de zone
```javascript
await connection.invoke("JoinZoneGroup", zoneId);
console.log(`Rejoint le groupe de la zone ${zoneId}`);
```

### Quitter un groupe de zone
```javascript
await connection.invoke("LeaveZoneGroup", zoneId);
console.log(`Quitté le groupe de la zone ${zoneId}`);
```

### Envoyer une notification utilisateur (depuis un admin)
```javascript
await connection.invoke("SendNotificationToUser", userId, title, message);
```

### Envoyer une notification de zone
```javascript
await connection.invoke("SendNotificationToZone", zoneId, title, message);
```

## Gestion des erreurs et reconnexion

### JavaScript/TypeScript
```javascript
// Gestion des erreurs de connexion
connection.onclose(async () => {
    console.log("Connexion fermée");
    await startConnection();
});

// Gestion de la reconnexion
connection.onreconnecting(() => {
    console.log("Reconnexion en cours...");
});

connection.onreconnected(() => {
    console.log("Reconnecté avec succès");
    // Rejoindre les groupes nécessaires
    rejoinGroups();
});

async function startConnection() {
    try {
        await connection.start();
        console.log("Connexion SignalR établie");
    } catch (err) {
        console.error("Erreur de connexion:", err);
        setTimeout(startConnection, 5000);
    }
}
```

### C# (Xamarin/MAUI)
```csharp
connection.Closed += async (error) =>
{
    await Task.Delay(5000);
    await connection.StartAsync();
};

connection.Reconnecting += (error) =>
{
    Debug.WriteLine("Reconnexion en cours...");
    return Task.CompletedTask;
};

connection.Reconnected += (connectionId) =>
{
    Debug.WriteLine("Reconnecté avec succès");
    return Task.CompletedTask;
};
```

## Exemples d'intégration

### React Component
```jsx
import React, { useEffect, useState } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';

function NotificationComponent() {
    const [connection, setConnection] = useState(null);
    const [notifications, setNotifications] = useState([]);

    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl('/notificationhub', {
                accessTokenFactory: () => localStorage.getItem('accessToken')
            })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);

        newConnection.start()
            .then(() => {
                console.log('Connecté à SignalR');
                
                // Écouter les notifications
                newConnection.on('ReceiveNotification', (notification) => {
                    setNotifications(prev => [...prev, notification]);
                });
                
                // Écouter les mises à jour de périphériques
                newConnection.on('DeviceStatusUpdate', (update) => {
                    // Mettre à jour l'état des périphériques
                });
            })
            .catch(e => console.log('Erreur de connexion: ', e));

        return () => {
            if (newConnection) {
                newConnection.stop();
            }
        };
    }, []);

    return (
        <div>
            {notifications.map((notif, index) => (
                <div key={index} className="notification">
                    <h4>{notif.title}</h4>
                    <p>{notif.message}</p>
                </div>
            ))}
        </div>
    );
}
```

### Vue.js Component
```vue
<template>
  <div>
    <div v-for="notification in notifications" :key="notification.id">
      <h4>{{ notification.title }}</h4>
      <p>{{ notification.message }}</p>
    </div>
  </div>
</template>

<script>
import { HubConnectionBuilder } from '@microsoft/signalr';

export default {
  data() {
    return {
      connection: null,
      notifications: []
    };
  },
  
  async mounted() {
    this.connection = new HubConnectionBuilder()
      .withUrl('/notificationhub', {
        accessTokenFactory: () => localStorage.getItem('accessToken')
      })
      .withAutomaticReconnect()
      .build();

    await this.connection.start();
    
    this.connection.on('ReceiveNotification', (notification) => {
      this.notifications.push(notification);
    });
  },
  
  beforeUnmount() {
    if (this.connection) {
      this.connection.stop();
    }
  }
};
</script>
```

## Tests des fonctionnalités

Utilisez les endpoints de test disponibles dans l'API :

### Test de notification utilisateur
```http
POST /api/SignalRTest/test-user-notification
{
    "userId": "user-id-here",
    "title": "Test Notification",
    "message": "Ceci est un test"
}
```

### Test de broadcast
```http
POST /api/SignalRTest/test-broadcast
{
    "title": "Message Global",
    "message": "Message pour tous les utilisateurs"
}
```

### Test de mise à jour de périphérique
```http
POST /api/SignalRTest/test-device-update
{
    "peripheriqueId": 1,
    "status": "online",
    "data": { "temperature": 22.5 }
}
```

## Bonnes pratiques

1. **Authentification** : Toujours inclure le token JWT
2. **Reconnexion** : Implémenter la reconnexion automatique
3. **Gestion d'erreurs** : Prévoir des fallbacks en cas de perte de connexion
4. **Performance** : Éviter d'écouter trop d'événements simultanément
5. **Nettoyage** : Toujours fermer les connexions lors de la destruction des composants
6. **Groupes** : Rejoindre seulement les groupes nécessaires pour l'utilisateur

## Sécurité

- Les connexions SignalR sont authentifiées via JWT
- Seuls les utilisateurs authentifiés peuvent se connecter au hub
- Les notifications sont filtrées selon les droits d'accès de l'utilisateur
- Les groupes de zones respectent les autorisations des utilisateurs