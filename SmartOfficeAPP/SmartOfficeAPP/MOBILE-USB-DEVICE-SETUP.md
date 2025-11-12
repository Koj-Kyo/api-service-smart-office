# ?? Configuration pour Appareil Mobile Physique (USB)

## ? Configuration Backend Appliquée

Le backend a été configuré pour accepter les connexions depuis des appareils mobiles physiques connectés en USB.

### Modifications Effectuées

1. **launchSettings.json** - Écoute sur toutes les interfaces réseau (`0.0.0.0`)
2. **Program.cs** - Politique CORS étendue pour accepter les IPs privées

---

## ?? Configuration Requise

### 1?? Trouver l'adresse IP de votre PC

Ouvrez PowerShell ou CMD et exécutez :

```powershell
ipconfig
```

Cherchez l'adresse IPv4 de votre adaptateur réseau actif (WiFi ou Ethernet).

**Exemple :**
```
Carte réseau sans fil Wi-Fi :
   Adresse IPv4. . . . . . . . . . . . . : 192.168.1.100
```

> ?? **Important** : Notez cette adresse IP, vous en aurez besoin dans votre application mobile.

---

### 2?? Configurer le Pare-feu Windows

Le backend écoute sur les ports **5290 (HTTP)** et **7144 (HTTPS)**. Vous devez autoriser ces ports dans le pare-feu.

#### Option A : PowerShell (Administrateur)

```powershell
# Autoriser le port HTTP 5290
New-NetFirewallRule -DisplayName "SmartOffice API HTTP" -Direction Inbound -LocalPort 5290 -Protocol TCP -Action Allow

# Autoriser le port HTTPS 7144
New-NetFirewallRule -DisplayName "SmartOffice API HTTPS" -Direction Inbound -LocalPort 7144 -Protocol TCP -Action Allow
```

#### Option B : Interface Graphique

1. Ouvrez **Pare-feu Windows Defender**
2. Cliquez sur **Paramètres avancés**
3. Cliquez sur **Règles de trafic entrant** ? **Nouvelle règle...**
4. Sélectionnez **Port** ? Suivant
5. Sélectionnez **TCP** et entrez le port **5290** ? Suivant
6. Sélectionnez **Autoriser la connexion** ? Suivant
7. Appliquez à tous les profils ? Suivant
8. Nommez la règle "SmartOffice API HTTP" ? Terminer
9. **Répétez pour le port 7144** (HTTPS)

---

### 3?? Lancer le Backend

Démarrez l'application en mode HTTP (recommandé pour les tests) :

```bash
cd SmartOfficeAPP
dotnet run --launch-profile http
```

Ou en mode HTTPS (nécessite un certificat de développement) :

```bash
dotnet run --launch-profile https
```

Vous devriez voir :

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5290
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

---

### 4?? Configurer l'Application Mobile

Dans votre application mobile MAUI, configurez l'URL de base de l'API avec l'adresse IP de votre PC :

#### ? Au lieu de :
```csharp
private const string BaseUrl = "http://localhost:5290";
```

#### ? Utilisez :
```csharp
private const string BaseUrl = "http://192.168.1.100:5290"; // Remplacez par votre IP
```

---

## ?? Tester la Connexion

### Test 1 : Depuis le navigateur de votre PC

Ouvrez votre navigateur et allez sur :
```
http://192.168.1.100:5290/health/live
```

Vous devriez voir :
```
Healthy
```

### Test 2 : Depuis le navigateur de votre téléphone

1. Connectez votre téléphone au **même réseau WiFi** que votre PC
2. Ouvrez le navigateur mobile
3. Allez sur : `http://192.168.1.100:5290/health/live`

Si vous voyez "Healthy", la connexion fonctionne ! ?

### Test 3 : API Test avec Authentification

```http
POST http://192.168.1.100:5290/api/Auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin@123"
}
```

---

## ?? Dépannage

### Problème : "ERR_CONNECTION_REFUSED"

**Solutions :**
1. Vérifiez que le backend est démarré
2. Vérifiez que le pare-feu autorise les ports 5290 et 7144
3. Vérifiez que vous utilisez la bonne adresse IP

### Problème : "ERR_CONNECTION_TIMED_OUT"

**Solutions :**
1. Assurez-vous que votre téléphone et votre PC sont sur le **même réseau WiFi**
2. Désactivez temporairement le pare-feu pour tester
3. Vérifiez qu'aucun antivirus ne bloque la connexion

### Problème : CORS Error (dans les logs du backend)

**Solutions :**
1. Vérifiez que la politique CORS `AllowMobileApp` est active
2. Le backend autorise maintenant toutes les IPs privées :
   - `10.x.x.x`
   - `172.16.x.x` à `172.31.x.x`
   - `192.168.x.x`

---

## ?? URLs Disponibles

Une fois le backend démarré, ces endpoints sont accessibles :

| Endpoint | URL |
|----------|-----|
| Health Check | `http://192.168.1.100:5290/health/live` |
| Swagger UI | `http://192.168.1.100:5290/swagger` |
| API Auth | `http://192.168.1.100:5290/api/Auth/login` |
| SignalR Hub | `http://192.168.1.100:5290/notificationhub` |

---

## ?? Note de Sécurité

Cette configuration est pour le **développement uniquement**. En production :

1. N'utilisez **que HTTPS** avec des certificats valides
2. Configurez CORS avec des origines spécifiques
3. Ne pas exposer `0.0.0.0` sur Internet
4. Utilisez un reverse proxy (comme nginx ou Azure App Service)

---

## ?? Checklist Rapide

- [ ] Backend démarré avec `dotnet run --launch-profile http`
- [ ] Adresse IP du PC identifiée avec `ipconfig`
- [ ] Ports 5290 et 7144 autorisés dans le pare-feu
- [ ] Téléphone et PC sur le même réseau WiFi
- [ ] URL de base configurée dans l'app mobile avec l'IP du PC
- [ ] Test de connexion réussi depuis le navigateur mobile

---

## ?? Prochaines Étapes

1. **Test l'authentification** depuis l'app mobile
2. **Vérifiez SignalR** pour les notifications en temps réel
3. **Testez les endpoints** de gestion des dispositifs

Bon développement ! ??
