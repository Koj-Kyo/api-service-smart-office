# ?? Guide Rapide: Contrôle LEDs et Porte

## ?? Ce que vous pouvez contrôler

### ?? **3 LEDs disponibles:**
- **LED RDC** (GPIO 27) - Rez-de-chaussée
- **LED Étage 2** (GPIO 26)
- **LED Étage 3** (GPIO 14)

### ?? **1 Porte:**
- **Servo** (GPIO 13)
- Position ouverte: 175°
- Position fermée: 35°

---

## ?? Démarrage Rapide (3 étapes)

### **Étape 1: S'authentifier** ??

```http
POST https://localhost:7169/api/Auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin@123"
}
```

**Copier le token reçu** dans la réponse.

---

### **Étape 2: Allumer une LED** ??

#### **Option A - LED Étage 2:**
```http
POST https://localhost:7169/api/Devices/mon-esp32/peripherals/led_etage2/commands/on
Authorization: Bearer VOTRE_TOKEN_ICI
```

#### **Option B - LED Étage 3:**
```http
POST https://localhost:7169/api/Devices/mon-esp32/peripherals/led_etage3/commands/on
Authorization: Bearer VOTRE_TOKEN_ICI
```

#### **Option C - LED RDC:**
```http
POST https://localhost:7169/api/Devices/mon-esp32/peripherals/led_rdc/commands/on
Authorization: Bearer VOTRE_TOKEN_ICI
```

---

### **Étape 3: Contrôler la porte** ??

#### **Ouvrir la porte:**
```http
POST https://localhost:7169/api/IoTHub/device/mon-esp32/method
Authorization: Bearer VOTRE_TOKEN_ICI
Content-Type: application/json

{
  "methodName": "openDoor",
  "payload": {}
}
```

#### **Fermer la porte:**
```http
POST https://localhost:7169/api/IoTHub/device/mon-esp32/method
Authorization: Bearer VOTRE_TOKEN_ICI
Content-Type: application/json

{
  "methodName": "closeDoor",
  "payload": {}
}
```

---

## ?? Commandes Disponibles

### **Pour les LEDs:**
| Commande | Action | Endpoint |
|----------|--------|----------|
| `on` | Allumer | `POST /api/Devices/mon-esp32/peripherals/{led_name}/commands/on` |
| `off` | Éteindre | `POST /api/Devices/mon-esp32/peripherals/{led_name}/commands/off` |
| `toggle` | Basculer | `POST /api/Devices/mon-esp32/peripherals/{led_name}/commands/toggle` |

**Noms disponibles:** `led_rdc`, `led_etage2`, `led_etage3`

---

### **Pour la porte:**
| Commande | Action | Méthode directe |
|----------|--------|-----------------|
| Ouvrir | Servo ? 175° | `openDoor` |
| Fermer | Servo ? 35° | `closeDoor` |

---

## ?? Exemples de Scénarios

### **Scénario 1: Entrée au bureau** ??
```bash
# 1. Allumer LED Étage 2
POST /api/Devices/mon-esp32/peripherals/led_etage2/commands/on

# 2. Ouvrir la porte
POST /api/IoTHub/device/mon-esp32/method
{ "methodName": "openDoor" }

# 3. Attendre 3 secondes...

# 4. Fermer la porte
POST /api/IoTHub/device/mon-esp32/method
{ "methodName": "closeDoor" }
```

---

### **Scénario 2: Fin de journée** ??
```bash
# 1. Éteindre toutes les LEDs
POST /api/Devices/mon-esp32/peripherals/led_rdc/commands/off
POST /api/Devices/mon-esp32/peripherals/led_etage2/commands/off
POST /api/Devices/mon-esp32/peripherals/led_etage3/commands/off

# 2. Fermer la porte
POST /api/IoTHub/device/mon-esp32/method
{ "methodName": "closeDoor" }
```

---

### **Scénario 3: Mode présentation** ??
```bash
# 1. Allumer toutes les LEDs
POST /api/Devices/mon-esp32/peripherals/led_rdc/commands/on
POST /api/Devices/mon-esp32/peripherals/led_etage2/commands/on
POST /api/Devices/mon-esp32/peripherals/led_etage3/commands/on

# 2. Ouvrir la porte
POST /api/IoTHub/device/mon-esp32/method
{ "methodName": "openDoor" }
```

---

## ?? Vérifier l'État

### **Voir l'état d'une LED:**
```http
GET https://localhost:7169/api/Devices/mon-esp32/peripherals/led_etage2
Authorization: Bearer VOTRE_TOKEN_ICI
```

**Réponse attendue:**
```json
{
  "name": "led_etage2",
  "type": "light",
  "gpio": 26,
  "state": true,          // true = allumée, false = éteinte
  "configured": true,
  "status": "active",
  "lastChange": "2025-01-20T15:30:00Z"
}
```

---

### **Voir l'état de la porte:**
```http
POST https://localhost:7169/api/IoTHub/device/mon-esp32/method
Authorization: Bearer VOTRE_TOKEN_ICI
Content-Type: application/json

{
  "methodName": "getStatus",
  "payload": {}
}
```

**Réponse attendue:**
```json
{
  "firmware": {
    "version": "1.3.0",
    "commit": "prod2025"
  },
  "door": "open",      // "open" ou "closed"
  "alarm": "inactive",
  "rssi": -45,
  "heap": 245632,
  "peripheralsCount": 4,
  "uptime": 3600
}
```

---

## ?? Points Importants

### **1. Alarme Fumée ??**
Si l'alarme est active, les commandes sont **BLOQUÉES**.

**Désactiver l'alarme:**
```http
POST https://localhost:7169/api/IoTHub/device/mon-esp32/method
Content-Type: application/json

{
  "methodName": "alarmOff",
  "payload": {}
}
```

---

### **2. Délais Entre Commandes ??**
Attendre **1-2 secondes** entre chaque commande pour permettre au firmware de traiter.

---

### **3. Vérification de Connexion ??**
Avant d'envoyer des commandes, vérifier que l'ESP32 est connecté:

```http
GET https://localhost:7169/api/Devices/mon-esp32/twin/full
```

Vérifier: `"connectionState": "Connected"`

---

## ??? Dépannage Rapide

### **Problème: LED ne s'allume pas** ?

**Causes possibles:**
1. ? Device non connecté ? Vérifier `connectionState`
2. ? Alarme active ? Désactiver avec `alarmOff`
3. ? Configuration incorrecte ? Vérifier `configured: true`
4. ? Token expiré ? Se réauthentifier

---

### **Problème: Porte ne bouge pas** ?

**Causes possibles:**
1. ? Alarme active ? Désactiver avec `alarmOff`
2. ? Device offline ? Vérifier connexion IoT Hub
3. ? Servo non alimenté ? Vérifier câblage physique

---

### **Problème: Erreur 401 Unauthorized** ?

**Solution:**
```http
# 1. Se réauthentifier
POST https://localhost:7169/api/Auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin@123"
}

# 2. Copier le nouveau token
# 3. Réessayer la commande
```

---

## ?? Utilisation avec Visual Studio Code

### **Méthode Recommandée:**

1. **Ouvrir le fichier de tests:**
   ```
   SmartOfficeAPP\Tests\16-LEDs-Door-Control.http
   ```

2. **Installer l'extension REST Client:**
   - Chercher "REST Client" dans les extensions VS Code
   - Installer

3. **Exécuter les tests:**
   - Cliquer sur "Send Request" au-dessus de chaque requête
   - Les résultats apparaissent dans un nouvel onglet

---

## ?? Résumé des Endpoints Clés

| Action | Méthode | URL |
|--------|---------|-----|
| Authentification | POST | `/api/Auth/login` |
| Allumer LED | POST | `/api/Devices/{id}/peripherals/{name}/commands/on` |
| Éteindre LED | POST | `/api/Devices/{id}/peripherals/{name}/commands/off` |
| Toggle LED | POST | `/api/Devices/{id}/peripherals/{name}/commands/toggle` |
| Ouvrir Porte | POST | `/api/IoTHub/device/{id}/method` + `openDoor` |
| Fermer Porte | POST | `/api/IoTHub/device/{id}/method` + `closeDoor` |
| État LED | GET | `/api/Devices/{id}/peripherals/{name}` |
| État Device | POST | `/api/IoTHub/device/{id}/method` + `getStatus` |

---

## ?? Ressources

- **Fichier de tests complet:** `Tests/16-LEDs-Door-Control.http`
- **Documentation firmware:** `FIRMWARE-BACKEND-COMPATIBILITY-GUIDE.md`
- **Mappings détaillés:** `FIRMWARE-BACKEND-MAPPINGS.md`
- **Guide général:** `FIRMWARE-BACKEND-README.md`

---

## ? Checklist Rapide

Avant de commencer:
- [ ] ESP32 flashé avec firmware v1.3.0
- [ ] Device connecté à IoT Hub
- [ ] Backend en cours d'exécution (port 7169)
- [ ] Token d'authentification obtenu
- [ ] Extension REST Client installée (VS Code)

Actions disponibles:
- [ ] Allumer LED Étage 2
- [ ] Allumer LED Étage 3
- [ ] Éteindre toutes les LEDs
- [ ] Ouvrir la porte
- [ ] Fermer la porte
- [ ] Vérifier l'état

---

**Version:** 1.0.0  
**Date:** 20 Janvier 2025  
**Compatibilité:** Firmware v1.3.0 + Backend .NET 9

---

## ?? C'est Parti !

Vous êtes prêt à contrôler vos LEDs et votre porte ! ??

**Commencez par:**
1. Ouvrir `Tests/16-LEDs-Door-Control.http`
2. Exécuter le test #1 (Authentification)
3. Exécuter le test #3 (Allumer LED Étage 2)
4. Exécuter le test #11 (Ouvrir la porte)

**Bon contrôle !** ????
