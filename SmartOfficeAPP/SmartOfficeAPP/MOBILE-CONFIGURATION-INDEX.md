# ?? Configuration Mobile USB - Index

## ?? Configuration Complète pour Appareil Mobile Physique

Ce dossier contient toute la configuration nécessaire pour connecter votre application mobile MAUI à l'API SmartOffice via USB (même réseau WiFi).

---

## ?? NOUVEAU : Guide Express (COMMENCEZ ICI !)

| Document | Description | Temps |
|----------|-------------|-------|
| **[MOBILE-SETUP-EXPRESS.md](MOBILE-SETUP-EXPRESS.md)** | ?? Guide ultra-rapide avec juste les commandes | ? 3 min |
| **[MOBILE-SETUP-VISUAL-GUIDE.md](MOBILE-SETUP-VISUAL-GUIDE.md)** | ?? Guide visuel avec exemples de code | ?? 5 min |

---

## ?? Documentation Complète

| Document | Description | Temps |
|----------|-------------|-------|
| **[MOBILE-USB-QUICK-START.md](MOBILE-USB-QUICK-START.md)** | Guide rapide 3 minutes | ?? 3 min |
| **[MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md)** | Guide complet avec dépannage | ?? 15 min |
| **[MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md)** | Exemple de code C# pour MAUI | ?? Code |
| **[Tests/40-Mobile-Device-Connection-Tests.http](Tests/40-Mobile-Device-Connection-Tests.http)** | Tests HTTP pour validation | ?? Tests |

---

## ? Démarrage Rapide (3 Minutes)

### 1?? Configurer le Pare-feu

**PowerShell en Administrateur :**
```powershell
cd SmartOfficeAPP
.\configure-firewall.ps1
```

### 2?? Trouver votre IP

```powershell
ipconfig
```
Note : `192.168.X.X`

### 3?? Démarrer le Backend

```bash
cd SmartOfficeAPP
dotnet run --launch-profile http
```

### 4?? Tester depuis le Téléphone

Navigateur mobile : `http://192.168.X.X:5290/health/live`

---

## ?? Modifications Appliquées

### Backend (Automatique)

? **Program.cs** : Politique CORS étendue pour accepter toutes les IPs privées
```csharp
// Accepte : 10.x.x.x, 172.16-31.x.x, 192.168.x.x
SetIsOriginAllowed(origin => origin.StartsWith("http://192.168."))
```

? **launchSettings.json** : Écoute sur toutes les interfaces réseau
```json
"applicationUrl": "http://0.0.0.0:5290"
```

### Application Mobile (Manuel)

?? **ApiService.cs** : Configuration de l'URL de base
```csharp
private const string BaseUrl = "http://192.168.X.X:5290"; // Votre IP
```

Voir : [MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md)

---

## ?? Tests de Validation

### Health Check
```http
GET http://192.168.X.X:5290/health/live
```
? Résultat attendu : `Healthy`

### Authentification
```http
POST http://192.168.X.X:5290/api/Auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin@123"
}
```
? Résultat attendu : Token JWT

### Dashboard Stats
```http
GET http://192.168.X.X:5290/api/Dashboard/stats
Authorization: Bearer {token}
```
? Résultat attendu : Statistiques en JSON

Voir tous les tests : [40-Mobile-Device-Connection-Tests.http](Tests/40-Mobile-Device-Connection-Tests.http)

---

## ??? Outils Fournis

### Script PowerShell : `configure-firewall.ps1`

Configure automatiquement le pare-feu Windows pour autoriser les connexions sur les ports 5290 (HTTP) et 7144 (HTTPS).

**Usage :**
```powershell
# En tant qu'Administrateur
.\configure-firewall.ps1
```

**Actions :**
- ? Supprime les anciennes règles
- ? Crée les règles HTTP/HTTPS
- ? Affiche votre adresse IP
- ? Vérifie la configuration

---

## ?? Checklist Complète

### Configuration Backend
- [x] CORS configuré pour accepter les IPs privées
- [x] Kestrel configuré pour écouter sur `0.0.0.0`
- [x] Ports 5290 (HTTP) et 7144 (HTTPS) ouverts
- [ ] Pare-feu configuré (exécutez `configure-firewall.ps1`)
- [ ] Backend démarré

### Configuration Mobile
- [ ] Adresse IP du PC identifiée
- [ ] `BaseUrl` configurée dans `ApiService.cs`
- [ ] Téléphone et PC sur le même WiFi
- [ ] App déployée sur le téléphone
- [ ] Test de connexion réussi

### Validation
- [ ] Health check OK depuis le navigateur mobile
- [ ] Authentification réussie depuis l'app
- [ ] Données récupérées depuis l'API

---

## ?? Résolution de Problèmes

| Problème | Solution | Document |
|----------|----------|----------|
| **Connection Refused** | Vérifiez que le backend est démarré | [Quick Start](MOBILE-USB-QUICK-START.md) |
| **Timeout** | Vérifiez le WiFi (même réseau) | [Setup Guide](MOBILE-USB-DEVICE-SETUP.md) |
| **Firewall Error** | Exécutez `configure-firewall.ps1` en admin | [Setup Guide](MOBILE-USB-DEVICE-SETUP.md) |
| **CORS Error** | Configuration déjà appliquée, redémarrez | [Setup Guide](MOBILE-USB-DEVICE-SETUP.md) |
| **SSL Error** | Utilisez HTTP en dev ou configurez certificat | [API Client Example](MOBILE-API-CLIENT-EXAMPLE.md) |

---

## ?? Guide Détaillé par Étape

### Pour les Débutants
?? Commencez par : [MOBILE-USB-QUICK-START.md](MOBILE-USB-QUICK-START.md)

### Pour Plus de Détails
?? Consultez : [MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md)

### Pour le Code MAUI
?? Voir : [MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md)

### Pour les Tests
?? Utilisez : [Tests/40-Mobile-Device-Connection-Tests.http](Tests/40-Mobile-Device-Connection-Tests.http)

---

## ?? Sécurité

### ?? Configuration de Développement

Cette configuration est pour le **développement uniquement** :
- Accepte toutes les IPs privées (192.168.x.x, 10.x.x.x)
- Écoute sur toutes les interfaces (`0.0.0.0`)
- Permet HTTP sans HTTPS

### ? Pour la Production

En production, vous devez :
1. Utiliser **HTTPS uniquement** avec certificat valide
2. Configurer CORS avec **origines spécifiques**
3. Utiliser un **reverse proxy** (nginx, Azure App Service)
4. Ne **jamais** exposer `0.0.0.0` sur Internet
5. Activer **rate limiting** et **authentification forte**

---

## ?? Architecture de Connexion

```
???????????????????         WiFi          ????????????????????
?   Téléphone     ?????????????????????????   PC (Backend)   ?
?   ?? MAUI App   ?   192.168.X.X:5290   ?   ??? ASP.NET API ?
???????????????????                        ????????????????????
         ?                                          ?
         ?                                          ?
         ?         HTTP/HTTPS + JWT                 ?
         ?         SignalR WebSocket                ?
         ?                                          ?
         ????????????????????????????????????????????
```

---

## ?? Ports Utilisés

| Port | Protocole | Usage |
|------|-----------|-------|
| **5290** | HTTP | API REST |
| **7144** | HTTPS | API REST (SSL) |
| **5290** | WebSocket | SignalR Notifications |

---

## ?? Commandes Rapides

### Démarrer le Backend
```bash
cd SmartOfficeAPP
dotnet run --launch-profile http
```

### Configurer le Pare-feu
```powershell
.\configure-firewall.ps1
```

### Trouver l'IP
```powershell
ipconfig | findstr "IPv4"
```

### Tester la Connexion
```powershell
curl http://192.168.X.X:5290/health/live
```

---

## ?? Support

Si vous rencontrez des problèmes :

1. **Vérifiez les logs du backend** pour les erreurs
2. **Consultez** [MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md) section "Dépannage"
3. **Testez** avec les fichiers HTTP fournis
4. **Vérifiez** que pare-feu et antivirus ne bloquent pas

---

## ? Validation Finale

Avant de commencer le développement, vérifiez :

- [ ] Backend démarre sans erreur
- [ ] Health check répond depuis le navigateur PC
- [ ] Health check répond depuis le navigateur mobile
- [ ] Authentification fonctionne depuis l'app mobile
- [ ] Données sont récupérées depuis l'app mobile
- [ ] SignalR se connecte (si utilisé)

**Temps total de configuration : ~5 minutes**

---

## ?? Ressources Additionnelles

- [ASP.NET Core CORS Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [.NET MAUI HttpClient](https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/rest)
- [SignalR .NET Client](https://learn.microsoft.com/en-us/aspnet/core/signalr/dotnet-client)

---

**Configuration terminée ! Bon développement ! ??**

