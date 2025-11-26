# ?? Configuration CORS pour Azure Static Web App

## ? Changements appliqués

Votre frontend déployé sur **Azure Static Web App** est maintenant autorisé à communiquer avec le backend API.

### URL du frontend autorisée

```
https://mango-bush-03e4a6703.3.azurestaticapps.net
```

---

## ?? Configuration CORS dans Program.cs

### Politique pour Frontend Web (`AllowFrontend`)

Cette politique autorise les origines suivantes :

| Origine | Description |
|---------|-------------|
| `https://localhost:7152` | Frontend local (HTTPS) |
| `http://localhost:5249` | Frontend local (HTTP) |
| `http://localhost:3000` | Frontend local (dev server React/Vue) |
| `https://localhost:3000` | Frontend local (dev server HTTPS) |
| ? `https://mango-bush-03e4a6703.3.azurestaticapps.net` | **Azure Static Web App** |

**Caractéristiques :**
- ? `AllowAnyHeader()` - Tous les headers HTTP autorisés
- ? `AllowAnyMethod()` - GET, POST, PUT, DELETE, PATCH, OPTIONS
- ? `AllowCredentials()` - Support des cookies et authentification

### Politique pour Mobile (`AllowMobileApp`)

Cette politique autorise également :
- ? Toutes les adresses IP privées (réseau local)
- ? Android Emulator (10.0.2.2, 10.0.3.2)
- ? iOS Simulator
- ? Schémas MAUI personnalisés (app://, file://)
- ? Tous les domaines `azurestaticapps.net` (wildcard)

---

## ?? Déploiement des changements

### Option 1 : Via Visual Studio (Recommandé)

```bash
# 1. Build local
dotnet build --configuration Release

# 2. Dans Visual Studio
# Clic droit sur le projet SmartOfficeAPP ? "Publish..."
# Sélectionnez votre profil Azure ? "Publish"
```

### Option 2 : Via Azure CLI

```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP

# 1. Publier localement
dotnet publish -c Release -o ./publish

# 2. Créer le package zip
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

# 3. Déployer sur Azure
az webapp deployment source config-zip `
  --resource-group smartoffice-free-rg `
  --name smartoffice-api-free `
  --src ./deploy.zip

# 4. Redémarrer l'application
az webapp restart `
  --resource-group smartoffice-free-rg `
  --name smartoffice-api-free
```

### Option 3 : Via GitHub Actions

Si vous avez configuré GitHub Actions, poussez simplement vos changements :

```bash
git add .
git commit -m "feat: add Azure Static Web App to CORS"
git push origin main
```

Le déploiement se fera automatiquement.

---

## ?? Test de la connexion Frontend ? Backend

### Test 1 : Health Check depuis le navigateur

Ouvrez votre frontend :
```
https://mango-bush-03e4a6703.3.azurestaticapps.net
```

Puis ouvrez la console développeur (F12) et testez :

```javascript
fetch('https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/health/ready')
  .then(res => res.text())
  .then(data => console.log(data))
  .catch(err => console.error('CORS Error:', err));
```

**Résultat attendu :**
```
Healthy
```

### Test 2 : Login depuis le frontend

```javascript
fetch('https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/api/Auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    email: 'adminer@smartoffice.com',
    password: 'Admin1234!'
  })
})
.then(res => res.json())
.then(data => console.log('Login successful:', data))
.catch(err => console.error('Login error:', err));
```

**Résultat attendu :**
```json
{
  "token": "eyJhbGc...",
  "refreshToken": "...",
  "email": "adminer@smartoffice.com",
  "userName": "admin"
}
```

### Test 3 : Vérifier les headers CORS

Dans les DevTools Network, vérifiez la réponse du backend :

```
Access-Control-Allow-Origin: https://mango-bush-03e4a6703.3.azurestaticapps.net
Access-Control-Allow-Credentials: true
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
Access-Control-Allow-Headers: *
```

---

## ?? Dépannage

### Erreur : "CORS policy: No 'Access-Control-Allow-Origin' header"

**Cause** : Le backend n'a pas encore été redéployé avec les nouvelles configurations CORS.

**Solution** :
1. Redéployez le backend (voir section Déploiement ci-dessus)
2. Attendez 2-3 minutes que l'App Service redémarre
3. Videz le cache du navigateur (Ctrl + F5)
4. Testez à nouveau

### Erreur : "Request blocked by CORS policy"

**Cause** : L'URL du frontend ne correspond pas exactement à celle configurée.

**Solution** :
Vérifiez que l'URL dans Program.cs est **exactement** :
```
https://mango-bush-03e4a6703.3.azurestaticapps.net
```

Sans slash final `/` et avec le bon protocole `https://`.

### Erreur : "Credentials flag is 'true', but 'Access-Control-Allow-Credentials' header is not present"

**Cause** : Le backend n'utilise pas la bonne politique CORS.

**Solution** :
Vérifiez que dans Program.cs, vous utilisez bien :
```csharp
app.UseCors("AllowFrontend"); // Pas "AllowMobileApp"
```

---

## ?? Configuration frontend (à mettre à jour)

Dans votre frontend (React/Vue/Angular), configurez l'URL de l'API :

### React/Vue/Angular

```javascript
// Dans votre fichier de configuration (ex: config.js, .env)
const API_BASE_URL = 'https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net';

// Exemple d'appel API avec fetch
fetch(`${API_BASE_URL}/api/Auth/login`, {
  method: 'POST',
  credentials: 'include', // ? IMPORTANT pour envoyer les cookies
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({ email, password })
});

// Exemple avec axios
import axios from 'axios';

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true, // ? IMPORTANT pour envoyer les cookies
  headers: {
    'Content-Type': 'application/json'
  }
});

api.post('/api/Auth/login', { email, password });
```

### .env dans Static Web App

Créez un fichier `.env` ou `staticwebapp.config.json` :

```json
{
  "globalHeaders": {
    "Access-Control-Allow-Credentials": "true"
  },
  "routes": [
    {
      "route": "/api/*",
      "rewrite": "https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/api/*"
    }
  ]
}
```

---

## ? Checklist de déploiement

Avant de valider la configuration CORS :

- [ ] ? Code modifié dans `Program.cs` (URL Static Web App ajoutée)
- [ ] ? Build réussi en local (`dotnet build`)
- [ ] ? Backend redéployé sur Azure App Service
- [ ] ? Application redémarrée (via Azure Portal ou CLI)
- [ ] ? Test health check depuis le frontend (console navigateur)
- [ ] ? Test login API depuis le frontend
- [ ] ? Headers CORS vérifiés dans DevTools Network
- [ ] ? Configuration frontend mise à jour avec la bonne URL API

---

## ?? Ressources

- **Documentation CORS ASP.NET Core** : https://learn.microsoft.com/aspnet/core/security/cors
- **Azure Static Web Apps Docs** : https://learn.microsoft.com/azure/static-web-apps/
- **Debugging CORS** : https://developer.mozilla.org/docs/Web/HTTP/CORS

---

## ?? Résultat

Votre frontend Static Web App peut maintenant communiquer avec le backend API sans erreur CORS !

**Architecture finale :**

```
???????????????????????????????????????????????????????????
?  Frontend (Azure Static Web App)                        ?
?  https://mango-bush-03e4a6703.3.azurestaticapps.net    ?
???????????????????????????????????????????????????????????
                   ?
                   ? ? CORS autorisé
                   ?
                   ?
???????????????????????????????????????????????????????????
?  Backend API (Azure App Service F1)                     ?
?  https://smartoffice-api-free-euhzgfb4fzd0ckbh         ?
?  .westeurope-01.azurewebsites.net                      ?
???????????????????????????????????????????????????????????
```

Bonne chance pour votre soutenance ! ??
