# ?? URLs de Déploiement SmartOffice

## ?? Backend API (Azure App Service)

**URL principale :**
```
https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net
```

**Endpoints importants :**

| Endpoint | URL complète | Description |
|----------|--------------|-------------|
| Health Check | `https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/health/ready` | Vérifier que l'API est en ligne |
| Swagger | `https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/swagger` | Documentation API interactive |
| Login | `https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/api/Auth/login` | Authentification utilisateur |

---

## ?? Frontend (Azure Static Web App)

**URL principale :**
```
https://mango-bush-03e4a6703.3.azurestaticapps.net
```

---

## ?? Configuration Frontend

### Fichier .env (à créer à la racine du projet frontend)

```env
# Backend API URL
VITE_API_BASE_URL=https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net

# ou pour Create React App
REACT_APP_API_BASE_URL=https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net

# ou pour Next.js
NEXT_PUBLIC_API_BASE_URL=https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net
```

### Exemple de service API (TypeScript)

```typescript
// services/api.config.ts
export const API_BASE_URL = 'https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net';

// services/api.service.ts
import axios from 'axios';
import { API_BASE_URL } from './api.config';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  withCredentials: true, // ? Important pour les cookies/JWT
  headers: {
    'Content-Type': 'application/json',
  },
});

// Ajouter automatiquement le token JWT
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default apiClient;
```

---

## ?? Tests de connexion

### Test 1 : Health Check (dans la console navigateur)

```javascript
fetch('https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/health/ready')
  .then(res => res.text())
  .then(data => console.log('Backend status:', data)); // "Healthy"
```

### Test 2 : Login (dans la console navigateur)

```javascript
fetch('https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/api/Auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include',
  body: JSON.stringify({
    email: 'adminer@smartoffice.com',
    password: 'Admin1234!'
  })
})
.then(res => res.json())
.then(data => console.log('Login response:', data));
```

---

## ?? Credentials par défaut (pour tests)

| Rôle | Email | Mot de passe |
|------|-------|--------------|
| Admin | `adminer@smartoffice.com` | `Admin1234!` |
| Gestionnaire | `manager@smartoffice.com` | `Manager1234!` |
| Utilisateur | `user@smartoffice.com` | `User1234!` |

---

## ?? Architecture

```
????????????????????????????????????????????????????????
?  Frontend (Azure Static Web App)                     ?
?  https://mango-bush-03e4a6703.3.azurestaticapps.net ?
????????????????????????????????????????????????????????
                 ?
                 ? ? CORS autorisé
                 ? ? JWT Bearer Token
                 ?
                 ?
????????????????????????????????????????????????????????
?  Backend API (Azure App Service F1 Free)             ?
?  https://smartoffice-api-free-euhzgfb4fzd0ckbh      ?
?  .westeurope-01.azurewebsites.net                   ?
????????????????????????????????????????????????????????
         ?               ?               ?
         ?               ?               ?
   ???????????    ????????????    ????????????
   ? IoT Hub ?    ? Storage  ?    ? Railway  ?
   ? Azure   ?    ? Azure    ?    ?PostgreSQL?
   ???????????    ????????????    ????????????
```

---

## ?? Dépannage

### Erreur CORS

**Symptôme :** `Access-Control-Allow-Origin` error

**Solution :**
1. Vérifiez que le backend est déployé avec les modifications CORS
2. Videz le cache navigateur (Ctrl + Shift + Delete)
3. Vérifiez que `withCredentials: true` est dans vos requêtes

### Erreur 401 Unauthorized

**Symptôme :** Les requêtes retournent 401

**Solution :**
1. Vérifiez que le token JWT est stocké : `localStorage.getItem('authToken')`
2. Vérifiez que le header Authorization est envoyé : `Bearer YOUR_TOKEN`
3. Connectez-vous à nouveau (token expiré)

### Backend non accessible

**Symptôme :** Timeout ou 503 Service Unavailable

**Solution :**
1. Vérifiez le status dans Azure Portal (App Service ? Overview)
2. Redémarrez l'application : `az webapp restart --name smartoffice-api-free --resource-group smartoffice-free-rg`
3. Attendez 1-2 minutes (cold start du plan F1 Free)

---

## ?? Documentation complète

- **Backend API** : `/swagger` endpoint
- **Guide CORS** : `CORS-STATIC-WEB-APP-CONFIG.md`
- **Guide Déploiement** : `AZURE-FREE-DEPLOYMENT-GUIDE.md`

---

**Dernière mise à jour :** $(Get-Date -Format "yyyy-MM-dd HH:mm")

