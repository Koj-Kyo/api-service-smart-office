# ?? Quick Start Guide - Tests API Smart Office

## Démarrage rapide (5 minutes)

### 1. Prérequis
- Application Smart Office démarrée
- VS Code avec extension REST Client
- Base de données configurée

### 2. Premier test - Authentification
```http
POST https://localhost:7242/api/auth/register
Content-Type: application/json

{
  "email": "test@smartoffice.com",
  "password": "TestPassword123!",
  "confirmPassword": "TestPassword123!",
  "fullName": "Test User"
}
```

### 3. Connexion
```http
POST https://localhost:7242/api/auth/login
Content-Type: application/json

{
  "email": "test@smartoffice.com",
  "password": "TestPassword123!"
}
```

### 4. Copier le token JWT
Copier le token retourné et l'utiliser dans les autres tests :
```
Authorization: Bearer votre_token_ici
```

## ?? Ordre de test recommandé

1. **Authentication** ? Obtenir tokens
2. **Zones** ? Créer zones test
3. **Devices** ? Ajouter périphériques
4. **Telemetry** ? Données de test
5. **Commands** ? Contrôle périphériques
6. **Analytics** ? Analyses énergétiques

## ?? Configuration rapide

### Variables à modifier dans chaque fichier :
```http
@baseUrl = https://localhost:7242
@adminToken = TOKEN_ADMIN_ICI
@userToken = TOKEN_USER_ICI
```

### URLs principales :
- **Auth** : `/api/auth/*`
- **Zones** : `/api/zones/*`
- **Devices** : `/api/devices/*`
- **Telemetry** : `/api/telemetries/*`
- **IoT Hub** : `/api/iothub/*`

## ? Tests essentiels

### Test de santé de l'API
```http
GET https://localhost:7242/api/zones
Authorization: Bearer {{adminToken}}
```

### Test télémétrie
```http
POST https://localhost:7242/api/telemetries/add
Authorization: Bearer {{userToken}}
Content-Type: application/json

{
  "peripheriqueLogiqueId": 1,
  "horodatage": "2024-01-15T10:30:00Z",
  "typeEvenement": "Test",
  "valeur": 100.0
}
```

## ??? Dépannage rapide

### Erreur 401 Unauthorized
- Vérifier le token JWT
- Token expiré ? Se reconnecter

### Erreur 403 Forbidden
- Vérifier les rôles utilisateur
- Utiliser token admin si nécessaire

### Erreur 404 Not Found
- Vérifier l'URL de base
- Vérifier que l'API est démarrée

### Erreur 500 Internal Server Error
- Vérifier les logs application
- Vérifier la connexion base de données

## ?? Résultats attendus

### Authentification réussie
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "test@smartoffice.com",
  "roles": ["User"]
}
```

### Création zone réussie
```json
{
  "id": 1,
  "name": "Bureau Principal"
}
```

### Ajout télémétrie réussi
```json
{
  "id": 1,
  "peripheriqueLogiqueId": 1,
  "typeEvenement": "Test",
  "valeur": 100.0
}
```