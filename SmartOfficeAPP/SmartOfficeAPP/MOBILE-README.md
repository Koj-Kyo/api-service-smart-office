# ?? Configuration Mobile - Démarrage Rapide

## ?? Objectif
Connecter votre application mobile MAUI à l'API SmartOffice sur votre téléphone physique connecté en USB.

---

## ? Guide Ultra-Rapide (3 Commandes)

### 1. Trouvez votre IP
```powershell
ipconfig | findstr "IPv4"
```
?? Notez l'adresse (ex: `192.168.1.100`)

### 2. Autorisez les ports (PowerShell Admin)
```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
.\configure-firewall.ps1
```

### 3. Démarrez le backend
```bash
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
dotnet run --launch-profile http
```

### 4. Dans votre code MAUI
```csharp
private const string BaseUrl = "http://192.168.1.100:5290"; // VOTRE IP
```

### 5. Testez
Navigateur mobile : `http://192.168.1.100:5290/health/live`

---

## ?? Documentation Détaillée

### **?? Configuration Réseau**

| Besoin | Document |
|--------|----------|
| **Juste les commandes** | [MOBILE-SETUP-EXPRESS.md](MOBILE-SETUP-EXPRESS.md) |
| **Guide visuel avec exemples** | [MOBILE-SETUP-VISUAL-GUIDE.md](MOBILE-SETUP-VISUAL-GUIDE.md) |
| **Guide complet** | [MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md) |
| **Index configuration** | [MOBILE-CONFIGURATION-INDEX.md](MOBILE-CONFIGURATION-INDEX.md) |

### **?? Contrôle des Périphériques**

| Besoin | Document |
|--------|----------|
| **Démarrage rapide (5 min)** | [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) ? |
| **Guide complet** | [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ?? |
| **Référence des commandes** | [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) ?? |
| **Documentation API** | [MOBILE-DEVICES-API-COMPLETE-GUIDE.md](MOBILE-DEVICES-API-COMPLETE-GUIDE.md) ?? |

### **?? Exemples de Code**

| Besoin | Document |
|--------|----------|
| **Exemple ApiService** | [MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md) |
| **Classe complète** | [MAUI-ApiService-Example.cs](MAUI-ApiService-Example.cs) |

### **?? Navigation Générale**

| Besoin | Document |
|--------|----------|
| **Index complet mobile** | [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md) ?? |
| **Index configuration** | [MOBILE-CONFIGURATION-INDEX.md](MOBILE-CONFIGURATION-INDEX.md) |

---

## ?? Parcours Recommandé

### **Je démarre de zéro**
1. ? Ce fichier (MOBILE-README.md) ? Configuration réseau
2. ? [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) ? Code minimal
3. ? Tester l'app
4. ? [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Fonctionnalités avancées

### **Je veux contrôler les périphériques**
1. ? [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) ? Code de base
2. ? [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) ? Toutes les commandes
3. ? Tester avec [Tests/29-Firmware-v2-Complete-Commands.http](Tests/29-Firmware-v2-Complete-Commands.http)

### **J'ai un problème**
1. ? [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Section "Dépannage"
2. ? Checklist de validation
3. ? [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md) ? Support rapide

---

## ? Checklist Rapide

- [ ] IP trouvée
- [ ] Pare-feu configuré
- [ ] Backend démarré
- [ ] Test navigateur OK
- [ ] IP dans le code MAUI
- [ ] App testée
- [ ] Périphériques listés
- [ ] Commandes fonctionnent

---

## ?? Commandes Disponibles

### **Commandes de Base**
- **ON/OFF** ? `setState`
- **Luminosité** ? `setBrightness`
- **Couleur** ? `setColor`
- **Température** ? `setTemperature`

### **Commandes Avancées**
- **Verrouillage** ? `lock/unlock`
- **Alarme** ? `arm/disarm`
- **Multimédia** ? `setVolume/playPause/mute`
- **Robot** ? `start/stop/dock`

?? **Voir toutes les commandes:** [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md)

---

## ?? Tests

| Fichier | Description |
|---------|-------------|
| [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http) | Tests API périphériques |
| [Tests/39-Mobile-Devices-Enriched-Tests.http](Tests/39-Mobile-Devices-Enriched-Tests.http) | Tests complets |
| [Tests/40-Mobile-Device-Connection-Tests.http](Tests/40-Mobile-Device-Connection-Tests.http) | Tests connexion |
| [Tests/29-Firmware-v2-Complete-Commands.http](Tests/29-Firmware-v2-Complete-Commands.http) | Tests commandes |

---

**Temps total : 5 minutes** ??  
**Documentation complète disponible ci-dessus** ??  
**Index complet : [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md)** ??
