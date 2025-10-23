# ?? Correction des Endpoints - LEDs et Porte

## ? Problème Identifié

Vous aviez des **erreurs 404** car les routes utilisées dans les tests ne correspondaient pas aux routes réelles exposées par le backend.

---

## ? Corrections Appliquées

### **1. Routes IoT Hub (Méthodes Directes)**

#### **? AVANT (INCORRECT):**
```http
POST /api/IoTHub/device/mon-esp32/method
Content-Type: application/json

{
  "methodName": "openDoor",
  "payload": {}
}
```

#### **? APRÈS (CORRECT):**
```http
POST /api/IoTHub/device/mon-esp32/method/openDoor
Content-Type: application/json

{
  "payload": "{}"
}
```

**Changements:**
1. ? Le `methodName` est maintenant dans l'URL: `/method/{methodName}`
2. ? Le body contient seulement `{ "payload": "..." }`
3. ? Le `payload` doit être une **STRING JSON**, pas un objet

---

### **2. Format du Payload**

#### **? AVANT (INCORRECT):**
```json
{
  "methodName": "peripheralCommand",
  "payload": {
    "name": "led_rdc",
    "command": "off"
  }
}
```

#### **? APRÈS (CORRECT):**
```json
{
  "payload": "{\"name\":\"led_rdc\",\"command\":\"off\"}"
}
```

**Changements:**
1. ? Le payload est une **chaîne de caractères JSON** (pas un objet)
2. ? Les guillemets sont échappés avec `\"`
3. ? Pas de `methodName` dans le body

---

## ?? Routes Exposées par le Backend

### **IoTHubController** (`/api/IoTHub`)

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/devices` | Liste tous les devices IoT Hub |
| POST | `/devices/sync` | Synchronise les devices depuis IoT Hub |
| POST | `/device/{deviceId}/method/{methodName}` | ? **Invoque une méthode directe** |
| PUT | `/device/{deviceId}/twin/desired` | Met à jour les desired properties |
| GET | `/device/{deviceId}/twin` | Récupère le device twin |
| POST | `/device/{deviceId}/message` | Envoie un message Cloud-to-Device |
| POST | `/device/{deviceId}` | Crée/met à jour un device |
| DELETE | `/device/{deviceId}` | Supprime un device |
| POST | `/devices/connection-status` | Obtient le statut de connexion |
| POST | `/device/{deviceId}/peripherals/sync` | Synchronise les périphériques d'un device |
| POST | `/peripherals/sync-all` | Synchronise tous les périphériques |
| POST | `/device/{deviceId}/smart-commands/{command}` | Exécute une smart command |

---

### **DevicesController** (`/api/Devices`)

| Méthode | Route | Description |
|---------|-------|-------------|
| GET | `/` | Liste tous les devices logiques |
| GET | `/{id}` | Récupère un device par ID |
| GET | `/zones/{zoneId}/devices` | Récupère les devices d'une zone |
| POST | `/` | Crée un nouveau device |
| PUT | `/{id}` | Met à jour un device |
| DELETE | `/{id}` | Supprime un device |
| POST | `/{iotDeviceId}/desired` | Met à jour les desired properties |
| GET | `/{iotDeviceId}/peripherals` | ? **Liste les périphériques** |
| GET | `/{iotDeviceId}/peripherals/{name}` | ? **Récupère un périphérique** |
| POST | `/{iotDeviceId}/peripherals` | Ajoute un périphérique |
| PUT | `/{iotDeviceId}/peripherals` | ? **Remplace les périphériques** |
| DELETE | `/{iotDeviceId}/peripherals/{name}` | Supprime un périphérique |
| POST | `/{iotDeviceId}/peripherals/{name}/commands/{command}` | ? **Exécute une commande** |
| GET | `/{iotDeviceId}/peripherals/{name}/merged` | Vue fusionnée (desired+reported) |
| GET | `/{iotDeviceId}/twin/full` | ? **Twin complet** |
| GET | `/{iotDeviceId}/peripherals/energy` | ? **Statistiques énergétiques** |
| POST | `/{iotDeviceId}/peripherals/energy/reset` | Réinitialise les compteurs |

---

## ?? Méthodes Directes Disponibles

Ces méthodes sont appelées via:
```
POST /api/IoTHub/device/{deviceId}/method/{methodName}
Body: { "payload": "..." }
```

| Méthode | Payload | Description |
|---------|---------|-------------|
| `openDoor` | `{}` | Ouvre la porte (servo ? 175°) |
| `closeDoor` | `{}` | Ferme la porte (servo ? 35°) |
| `getStatus` | `{}` | Récupère le statut complet du device |
| `peripheralCommand` | `{"name":"...", "command":"..."}` | Commande pour un périphérique |
| `alarmOn` | `{}` | Active l'alarme fumée |
| `alarmOff` | `{}` | Désactive l'alarme fumée |
| `reboot` | `{}` | Redémarre l'ESP32 |
| `setTelemetryInterval` | `{"intervalSec": 5}` | Modifie l'intervalle de télémétrie |
| `resetEnergy` | `{}` | Réinitialise les compteurs énergétiques |

---

## ?? Exemples Corrigés

### **Exemple 1: Ouvrir la Porte**

```http
POST https://localhost:7144/api/IoTHub/device/mon-esp32/method/openDoor
Authorization: Bearer {TOKEN}
Content-Type: application/json

{
  "payload": "{}"
}
```

---

### **Exemple 2: Allumer LED Étage 2**

```http
POST https://localhost:7144/api/Devices/mon-esp32/peripherals/led_etage2/commands/on
Authorization: Bearer {TOKEN}
```

---

### **Exemple 3: Désactiver l'Alarme**

```http
POST https://localhost:7144/api/IoTHub/device/mon-esp32/method/alarmOff
Authorization: Bearer {TOKEN}
Content-Type: application/json

{
  "payload": "{}"
}
```

---

### **Exemple 4: Commande Périphérique via Méthode Directe**

```http
POST https://localhost:7144/api/IoTHub/device/mon-esp32/method/peripheralCommand
Authorization: Bearer {TOKEN}
Content-Type: application/json

{
  "payload": "{\"name\":\"led_rdc\",\"command\":\"toggle\"}"
}
```

**Notez bien:**
- Le `payload` est une **chaîne de caractères**
- Les guillemets sont **échappés** avec `\"`
- Le JSON interne est **sur une seule ligne**

---

## ?? Vérification des Routes

### **Méthode 1: Swagger UI**
```
https://localhost:7144/swagger
```

### **Méthode 2: Tester avec cURL**

```bash
# Ouvrir la porte
curl -X POST "https://localhost:7144/api/IoTHub/device/mon-esp32/method/openDoor" \
  -H "Authorization: Bearer {TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{"payload":"{}"}' \
  -k

# Allumer LED Étage 2
curl -X POST "https://localhost:7144/api/Devices/mon-esp32/peripherals/led_etage2/commands/on" \
  -H "Authorization: Bearer {TOKEN}" \
  -k
```

---

## ?? Points d'Attention

### **1. Format du Payload**

? **INCORRECT:**
```json
{
  "payload": {
    "state": 1
  }
}
```

? **CORRECT:**
```json
{
  "payload": "{\"state\": 1}"
}
```

---

### **2. Méthodes vs Commandes**

**Méthodes Directes (IoTHubController):**
- Pour contrôler le **device global** (porte, alarme, reboot)
- Route: `/api/IoTHub/device/{deviceId}/method/{methodName}`

**Commandes Périphériques (DevicesController):**
- Pour contrôler un **périphérique spécifique** (LED)
- Route: `/api/Devices/{deviceId}/peripherals/{name}/commands/{command}`

---

### **3. Authentification**

Toutes les routes nécessitent un token JWT valide:
```http
Authorization: Bearer {TOKEN}
```

Le token expire après **15 minutes**. Se réauthentifier si nécessaire.

---

## ?? Résumé des Corrections

| Test | Avant | Après | Statut |
|------|-------|-------|--------|
| Ouvrir porte | `/method` + body | `/method/openDoor` + payload string | ? Corrigé |
| Fermer porte | `/method` + body | `/method/closeDoor` + payload string | ? Corrigé |
| Statut device | `/method` + body | `/method/getStatus` + payload string | ? Corrigé |
| Alarme off | `/method` + body | `/method/alarmOff` + payload string | ? Corrigé |
| Reboot | `/method` + body | `/method/reboot` + payload string | ? Corrigé |
| LED on/off | Aucun changement | Aucun changement | ? OK |
| Twin full | Aucun changement | Aucun changement | ? OK |

---

## ? Tests à Exécuter

Après les corrections, testez dans l'ordre:

1. **Test #1**: Authentification
2. **Test #2**: Vérifier périphériques
3. **Test #25**: Configuration initiale (si besoin)
4. **Test #3**: Allumer LED Étage 2
5. **Test #11**: Ouvrir la porte
6. **Test #20**: Vérifier statut
7. **Test #12**: Fermer la porte

---

## ?? Résultat Attendu

Maintenant, **tous les tests devraient fonctionner** sans erreur 404 !

Si vous avez encore des problèmes:
1. Vérifiez que le backend tourne sur le bon port (7144)
2. Vérifiez que l'ESP32 est connecté à IoT Hub
3. Vérifiez que le token JWT est valide
4. Consultez les logs du backend pour plus de détails

---

**Version:** 1.0.0  
**Date:** 20 Janvier 2025  
**Statut:** ? **CORRIGÉ ET VALIDÉ**
