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

| Besoin | Document |
|--------|----------|
| **Juste les commandes** | [MOBILE-SETUP-EXPRESS.md](MOBILE-SETUP-EXPRESS.md) |
| **Guide visuel avec exemples** | [MOBILE-SETUP-VISUAL-GUIDE.md](MOBILE-SETUP-VISUAL-GUIDE.md) |
| **Guide complet** | [MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md) |
| **Exemple de code MAUI** | [MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md) |
| **Index général** | [MOBILE-CONFIGURATION-INDEX.md](MOBILE-CONFIGURATION-INDEX.md) |

---

## ? Checklist Rapide

- [ ] IP trouvée
- [ ] Pare-feu configuré
- [ ] Backend démarré
- [ ] Test navigateur OK
- [ ] IP dans le code MAUI
- [ ] App testée

---

**Temps total : 5 minutes** ??  
**Documentation complète disponible ci-dessus** ??
