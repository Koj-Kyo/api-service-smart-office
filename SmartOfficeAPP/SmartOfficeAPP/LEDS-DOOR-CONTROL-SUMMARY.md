# ?? Résumé: Contrôle LEDs et Porte - Prêt à l'Emploi

## ? Fichiers Créés

J'ai créé **3 fichiers complets** pour vous permettre de contrôler facilement les LEDs et la porte :

### **1?? Fichier de Tests HTTP** ??
**Fichier:** `SmartOfficeAPP\Tests\16-LEDs-Door-Control.http`

**Contenu:** 25 tests structurés incluant:
- ? Authentification
- ? Contrôle LED Étage 2 (on/off/toggle)
- ? Contrôle LED Étage 3 (on/off/toggle)
- ? Contrôle LED RDC (on/off/toggle)
- ? Ouverture/fermeture de la porte
- ? 4 scénarios complets prêts à l'emploi
- ? Vérifications d'état
- ? Configuration initiale
- ? Guide de dépannage

**Pour utiliser:**
1. Ouvrir le fichier dans VS Code
2. Installer l'extension "REST Client"
3. Cliquer sur "Send Request" pour exécuter chaque test

---

### **2?? Guide Rapide Visuel** ??
**Fichier:** `SmartOfficeAPP\QUICK-START-LEDS-DOOR.md`

**Contenu:**
- ? Explication visuelle de chaque LED et GPIO
- ? 3 étapes pour démarrer rapidement
- ? Exemples de scénarios (entrée au bureau, fin de journée)
- ? Guide de vérification d'état
- ? Section dépannage détaillée
- ? Checklist complète

**Pour consulter:**
```bash
code SmartOfficeAPP/QUICK-START-LEDS-DOOR.md
```

---

### **3?? Commandes cURL/PowerShell** ??
**Fichier:** `SmartOfficeAPP\CURL-COMMANDS-LEDS-DOOR.md`

**Contenu:**
- ? Commandes cURL pour Linux/Mac
- ? Commandes PowerShell pour Windows
- ? Scripts de scénarios automatisés
- ? Fonctions utilitaires réutilisables
- ? One-liners rapides

**Pour utiliser:**
```bash
# Linux/Mac
code SmartOfficeAPP/CURL-COMMANDS-LEDS-DOOR.md

# Windows
code SmartOfficeAPP\CURL-COMMANDS-LEDS-DOOR.md
```

---

## ?? Démarrage Ultra-Rapide (3 étapes)

### **Étape 1: S'authentifier** ??

**VS Code (REST Client):**
```
Ouvrir: Tests/16-LEDs-Door-Control.http
Exécuter: Test #1 (Authentification)
```

**cURL (Linux/Mac):**
```bash
export TOKEN=$(curl -X POST "https://localhost:7169/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@smartoffice.com","password":"Admin@123"}' \
  -k | jq -r '.token')
```

**PowerShell (Windows):**
```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7169/api/Auth/login" `
  -Method POST -ContentType "application/json" `
  -Body '{"email":"admin@smartoffice.com","password":"Admin@123"}' `
  -SkipCertificateCheck
$TOKEN = $response.token
```

---

### **Étape 2: Allumer une LED** ??

**VS Code (REST Client):**
```
Tests/16-LEDs-Door-Control.http
? Test #3 (LED Étage 2)
? Test #6 (LED Étage 3)
```

**cURL (Linux/Mac):**
```bash
# LED Étage 2
curl -X POST "https://localhost:7169/api/Devices/mon-esp32/peripherals/led_etage2/commands/on" \
  -H "Authorization: Bearer $TOKEN" -k

# LED Étage 3
curl -X POST "https://localhost:7169/api/Devices/mon-esp32/peripherals/led_etage3/commands/on" \
  -H "Authorization: Bearer $TOKEN" -k
```

**PowerShell (Windows):**
```powershell
# LED Étage 2
Invoke-RestMethod -Uri "https://localhost:7169/api/Devices/mon-esp32/peripherals/led_etage2/commands/on" `
  -Method POST -Headers @{"Authorization"="Bearer $TOKEN"} -SkipCertificateCheck

# LED Étage 3
Invoke-RestMethod -Uri "https://localhost:7169/api/Devices/mon-esp32/peripherals/led_etage3/commands/on" `
  -Method POST -Headers @{"Authorization"="Bearer $TOKEN"} -SkipCertificateCheck
```

---

### **Étape 3: Contrôler la porte** ??

**VS Code (REST Client):**
```
Tests/16-LEDs-Door-Control.http
? Test #11 (Ouvrir porte)
? Test #12 (Fermer porte)
```

**cURL (Linux/Mac):**
```bash
# Ouvrir
curl -X POST "https://localhost:7169/api/IoTHub/device/mon-esp32/method" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"methodName":"openDoor","payload":{}}' -k

# Fermer
curl -X POST "https://localhost:7169/api/IoTHub/device/mon-esp32/method" \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"methodName":"closeDoor","payload":{}}' -k
```

**PowerShell (Windows):**
```powershell
# Ouvrir
$body = @{methodName="openDoor";payload=@{}} | ConvertTo-Json
Invoke-RestMethod -Uri "https://localhost:7169/api/IoTHub/device/mon-esp32/method" `
  -Method POST -Headers @{"Authorization"="Bearer $TOKEN"} `
  -ContentType "application/json" -Body $body -SkipCertificateCheck

# Fermer
$body = @{methodName="closeDoor";payload=@{}} | ConvertTo-Json
Invoke-RestMethod -Uri "https://localhost:7169/api/IoTHub/device/mon-esp32/method" `
  -Method POST -Headers @{"Authorization"="Bearer $TOKEN"} `
  -ContentType "application/json" -Body $body -SkipCertificateCheck
```

---

## ?? Référence Rapide

### **LEDs Disponibles:**
| LED | GPIO | Nom API | Commandes |
|-----|------|---------|-----------|
| RDC | 27 | `led_rdc` | on, off, toggle |
| Étage 2 | 26 | `led_etage2` | on, off, toggle |
| Étage 3 | 14 | `led_etage3` | on, off, toggle |

### **Porte:**
| Action | Méthode | Servo |
|--------|---------|-------|
| Ouvrir | `openDoor` | 175° |
| Fermer | `closeDoor` | 35° |

---

## ?? Scénarios Prêts à l'Emploi

### **Scénario 1: Entrée au Bureau** ??
```
1. Allumer LED Étage 2
2. Ouvrir la porte
3. [Attendre 3 secondes]
4. Fermer la porte
```

**Exécuter dans VS Code:**
```
Tests/16-LEDs-Door-Control.http
? Test #14 (Scénario complet)
```

---

### **Scénario 2: Fin de Journée** ??
```
1. Éteindre toutes les LEDs (RDC, Étage 2, Étage 3)
2. Fermer la porte
```

**Exécuter dans VS Code:**
```
Tests/16-LEDs-Door-Control.http
? Test #17 (Scénario complet)
```

---

### **Scénario 3: Mode Présentation** ??
```
1. Allumer toutes les LEDs
2. Ouvrir la porte
```

**Exécuter dans VS Code:**
```
Tests/16-LEDs-Door-Control.http
? Test #16 (Scénario complet)
```

---

## ?? Vérifier l'État

### **État d'une LED:**
```http
GET /api/Devices/mon-esp32/peripherals/led_etage2
```

**Réponse attendue:**
```json
{
  "name": "led_etage2",
  "gpio": 26,
  "state": true,          // true = allumée
  "configured": true,
  "status": "active"
}
```

---

### **État du Device Complet:**
```http
POST /api/IoTHub/device/mon-esp32/method
{ "methodName": "getStatus" }
```

**Réponse attendue:**
```json
{
  "door": "open",         // "open" ou "closed"
  "alarm": "inactive",
  "firmware": {
    "version": "1.3.0"
  }
}
```

---

## ?? Points Importants

### **1. Alarme Fumée** ??
Si l'alarme est active, les commandes sont **BLOQUÉES**.

**Désactiver:**
```http
POST /api/IoTHub/device/mon-esp32/method
{ "methodName": "alarmOff" }
```

### **2. Délais Entre Commandes** ??
Attendre **1-2 secondes** entre chaque commande.

### **3. Token d'Authentification** ??
Le token expire après **15 minutes**. Se réauthentifier si nécessaire.

---

## ??? Dépannage Rapide

### **Problème: LED ne s'allume pas** ?

**Solutions:**
1. Vérifier connexion device: `GET /twin/full` ? `connectionState: "Connected"`
2. Vérifier alarme: `POST /method` + `getStatus` ? `alarm: "inactive"`
3. Vérifier configuration: `GET /peripherals/led_etage2` ? `configured: true`

---

### **Problème: Porte ne bouge pas** ?

**Solutions:**
1. Désactiver alarme: `POST /method` + `alarmOff`
2. Vérifier connexion IoT Hub
3. Vérifier alimentation servo (câblage physique)

---

### **Problème: Erreur 401** ?

**Solution:** Se réauthentifier (token expiré)

---

## ?? Documentation Complète

| Fichier | Description | Utilisation |
|---------|-------------|-------------|
| `Tests/16-LEDs-Door-Control.http` | 25 tests structurés | VS Code + REST Client |
| `QUICK-START-LEDS-DOOR.md` | Guide visuel complet | Lecture, formation |
| `CURL-COMMANDS-LEDS-DOOR.md` | Commandes terminal | Ligne de commande, scripts |

---

## ? Checklist Avant de Commencer

- [ ] ESP32 flashé avec firmware v1.3.0
- [ ] Device `mon-esp32` connecté à IoT Hub
- [ ] Backend en cours d'exécution (port 7169)
- [ ] Extension REST Client installée (VS Code)
- [ ] Aucune alarme fumée active

---

## ?? Recommandations

### **Pour Débuter:**
1. Utiliser **VS Code + REST Client** (plus simple)
2. Commencer par **Test #1-3** (authentification + LED)
3. Tester **Test #11** (ouvrir porte)

### **Pour Automatiser:**
1. Utiliser les **scripts cURL/PowerShell**
2. Créer des **fonctions personnalisées**
3. Planifier avec **cron** (Linux) ou **Task Scheduler** (Windows)

### **Pour Déboguer:**
1. Consulter **Test #18-24** (vérifications)
2. Vérifier les **logs firmware** via télémétrie
3. Utiliser **Test #20** (`getStatus`) pour diagnostic

---

## ?? C'est Parti !

Vous avez maintenant **tout ce qu'il faut** pour contrôler vos LEDs et votre porte ! ??

**Commencez par:**
1. Ouvrir `Tests/16-LEDs-Door-Control.http`
2. Exécuter Test #1 (Authentification)
3. Exécuter Test #3 (Allumer LED Étage 2)
4. Exécuter Test #11 (Ouvrir la porte)

**Bon contrôle !** ?????

---

**Version:** 1.0.0  
**Date:** 20 Janvier 2025  
**Compatibilité:** Firmware ESP32 v1.3.0 + Backend .NET 9  
**Statut:** ? **PRÊT À L'EMPLOI**
