# ?? Quick Start - Connexion Mobile USB

## ?? Checklist 3 Minutes

### Étape 1 : Configurer le Pare-feu (1 min)

**Option Automatique (Recommandée)** :
```powershell
# Exécutez PowerShell en tant qu'ADMINISTRATEUR
cd SmartOfficeAPP
.\configure-firewall.ps1
```

**Option Manuelle** :
```powershell
# HTTP
New-NetFirewallRule -DisplayName "SmartOffice API HTTP" -Direction Inbound -LocalPort 5290 -Protocol TCP -Action Allow

# HTTPS
New-NetFirewallRule -DisplayName "SmartOffice API HTTPS" -Direction Inbound -LocalPort 7144 -Protocol TCP -Action Allow
```

---

### Étape 2 : Trouver votre IP (30 sec)

```powershell
ipconfig
```

Cherchez : `Adresse IPv4. . . . . . . . . . . . . : 192.168.X.X`

---

### Étape 3 : Démarrer le Backend (30 sec)

```bash
cd SmartOfficeAPP
dotnet run --launch-profile http
```

Attendez : `Now listening on: http://0.0.0.0:5290` ?

---

### Étape 4 : Tester depuis le Téléphone (1 min)

1. **Connectez le téléphone au même WiFi que le PC**
2. **Ouvrez le navigateur mobile**
3. **Allez sur** : `http://192.168.X.X:5290/health/live` (remplacez par votre IP)
4. **Résultat attendu** : `Healthy` ?

---

### Étape 5 : Configurer l'App Mobile

Dans votre code MAUI :

```csharp
// ? Ancien
private const string BaseUrl = "http://localhost:5290";

// ? Nouveau
private const string BaseUrl = "http://192.168.X.X:5290"; // Votre IP
```

---

## ?? Résolution Rapide des Problèmes

| Problème | Solution |
|----------|----------|
| **Connection Refused** | Vérifiez que le backend est démarré |
| **Timeout** | Vérifiez que téléphone et PC sont sur le même WiFi |
| **Firewall Error** | Exécutez le script PowerShell en admin |
| **CORS Error** | Configuration déjà appliquée, redémarrez le backend |

---

## ?? Test Rapide de Connexion

```http
POST http://192.168.X.X:5290/api/Auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin@123"
}
```

Résultat attendu : Token JWT ?

---

## ?? Configuration Appliquée

? **launchSettings.json** : Écoute sur `0.0.0.0`  
? **CORS Policy** : Accepte toutes les IPs privées (`192.168.x.x`, `10.x.x.x`, `172.16-31.x.x`)  
? **Ports ouverts** : 5290 (HTTP), 7144 (HTTPS)  

---

## ?? Documentation Complète

- ?? **Guide détaillé** : `MOBILE-USB-DEVICE-SETUP.md`
- ?? **Tests HTTP** : `Tests/40-Mobile-Device-Connection-Tests.http`
- ?? **Script pare-feu** : `configure-firewall.ps1`

---

## ?? Support

Si problème persiste :
1. Vérifiez les logs du backend
2. Désactivez temporairement le pare-feu pour tester
3. Vérifiez que l'antivirus ne bloque pas la connexion
4. Utilisez `telnet 192.168.X.X 5290` depuis le PC pour tester le port

---

**Temps total : ~3 minutes** ??  
**Prêt à développer ! ??**
