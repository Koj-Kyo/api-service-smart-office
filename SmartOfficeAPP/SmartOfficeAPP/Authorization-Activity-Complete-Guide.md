# Guide complet : Autorisation granulaire et Historique d'activité

## Vue d'ensemble

Ce document explique le système d'autorisation à plusieurs niveaux implémenté dans SmartOffice API, ainsi que le système de suivi d'activité utilisateur.

## Architecture d'autorisation

### 1. Niveaux d'autorisation

```
???????????????????????????????????????????????????????????????
?                    Niveau 1: Rôles                          ?
?  (Admin, Manager, User, Technician)                         ?
?  ? Détermine l'accès aux endpoints/fonctionnalités          ?
???????????????????????????????????????????????????????????????
                            ?
???????????????????????????????????????????????????????????????
?              Niveau 2: Permissions (optionnel)              ?
?  (devices:read, devices:write, zones:manage)                ?
?  ? Granularité supplémentaire pour les actions              ?
???????????????????????????????????????????????????????????????
                            ?
???????????????????????????????????????????????????????????????
?           Niveau 3: DroitsAcces (par ressource)             ?
?  ? Autorisation sur chaque Zone/Périphérique                ?
?  ? Types: Lecture (consulter) / Controle (agir)             ?
???????????????????????????????????????????????????????????????
```

### 2. Système DroitsAcces

#### Modèle de données

```csharp
public class DroitAcces
{
    public int Id { get; set; }
    public string UserId { get; set; }           // Utilisateur concerné
    public int? ZoneId { get; set; }             // Zone (si applicable)
    public int? PeripheriqueLogiqueId { get; set; } // Périphérique (si applicable)
    public DroitAccesType Type { get; set; }     // Lecture ou Controle
}

public enum DroitAccesType
{
    Lecture,    // Peut consulter (télémétrie, état, logs)
    Controle    // Peut consulter ET agir (commandes, modifications)
}
```

#### Règles de vérification

1. **Vérification directe** : L'utilisateur a-t-il un `DroitAcces` explicite sur la ressource ?
2. **Fallback par zone** : Si la ressource est un périphérique, l'utilisateur a-t-il un droit sur sa zone parente ?
3. **Bypass Admin** : Les administrateurs ont accès à tout automatiquement.

#### Hiérarchie des droits

```
Controle > Lecture
```

- Si on demande `Lecture` : `Lecture` OU `Controle` suffisent
- Si on demande `Controle` : seul `Controle` suffit

### 3. Handler d'autorisation resource-based

#### Implémentation

```csharp
// Dans un contrôleur
var hasAccess = await _authorizationService.CheckPeripheriqueAccessAsync(
    User,
    peripheriqueId,
    DroitAccesType.Lecture);

if (!hasAccess)
    return Forbid();
```

#### Flow d'autorisation

```
1. Requête ? Contrôleur
2. Contrôleur ? IAuthorizationService.CheckPeripheriqueAccessAsync()
3. IAuthorizationService ? ResourceAccessHandler
4. ResourceAccessHandler vérifie:
   a. User est Admin ? ? Success
   b. DroitAcces direct existe ? ? Check type
   c. DroitAcces sur zone parente ? ? Check type
5. Success ? Continue | Fail ? 403 Forbidden
```

## Endpoints sécurisés par DroitsAcces

### Télémétrie (Lecture)

```http
GET /api/Telemetries/peripherique?peripheriqueId=1
```

- **Requis** : `DroitAcces.Lecture` (ou `Controle`) sur le périphérique
- **Ou** : `DroitAcces.Lecture` (ou `Controle`) sur la zone du périphérique
- **Ou** : Rôle `Admin`

### Commandes IoT (Controle)

```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 1,
  "nomCommande": "turn_on",
  "valeur": "1"
}
```

- **Requis** : `DroitAcces.Controle` sur le périphérique
- **Ou** : `DroitAcces.Controle` sur la zone du périphérique
- **Ou** : Rôle `Admin`

### Logs périphériques (Lecture)

```http
GET /api/DeviceLogs?deviceId=device001
```

- **À implémenter** : vérification similaire à la télémétrie

## Gestion des DroitsAcces

### Création (Admin uniquement)

```http
POST /api/admin/access-rights
{
  "userId": "user-guid-here",
  "peripheriqueLogiqueId": 1,
  "type": "Lecture"
}
```

### Consultation (utilisateur)

```http
GET /api/user/access-rights/peripheriques
GET /api/user/access-rights/zones
```

### Modification/Suppression (Admin)

```http
PUT /api/admin/access-rights/{id}
DELETE /api/admin/access-rights/{id}
```

## Historique d'activité utilisateur

### Fonctionnalités

#### 1. Consultation de l'historique

```http
GET /api/user/activity/history?pageNumber=1&pageSize=20
```

**Paramètres** :
- `pageNumber` : page à récupérer (défaut: 1)
- `pageSize` : taille de page (défaut: 20, max: 100)
- `startDate` : date de début (optionnel)
- `endDate` : date de fin (optionnel)
- `activityType` : filtrer par type ("Commande", "Audit")

**Réponse** :
```json
{
  "activities": [
    {
      "id": 123,
      "timestamp": "2025-01-15T10:30:00Z",
      "activityType": "Commande",
      "description": "Commande turn_on",
      "targetResource": "Périphérique: Lampe LED",
      "targetResourceId": 1,
      "resourceType": "Périphérique",
      "status": "Succès",
      "details": "Valeur: 1"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 45,
    "totalPages": 3,
    "hasPrevious": false,
    "hasNext": true
  }
}
```

#### 2. Statistiques d'activité

```http
GET /api/user/activity/stats?startDate=2025-01-01&endDate=2025-01-31
```

**Réponse** :
```json
{
  "totalActivities": 145,
  "commandesExecutees": 98,
  "commandesEchouees": 5,
  "consultationsZones": 20,
  "consultationsPeripheriques": 22,
  "activitiesByType": {
    "Commandes": 103,
    "Audit": 42
  },
  "recentActivities": [...],
  "lastActivity": "2025-01-15T14:20:00Z",
  "activitiesByDay": {
    "2025-01-01": 5,
    "2025-01-02": 8,
    ...
  }
}
```

#### 3. Activités récentes (24h)

```http
GET /api/user/activity/recent
```

### Sources de données

L'historique agrège :
1. **Commandes** : table `Commandes` (envoi de commandes IoT)
2. **Audit** : table `JournalAudits` (actions système)

## Scénarios d'usage

### Scénario 1 : Utilisateur mobile consulte ses périphériques

1. **Login** ? Obtient JWT avec rôle `User`
2. **Liste ses droits** :
   ```http
   GET /api/user/access-rights/peripheriques
   ```
3. **Consulte télémétrie** (si `Lecture` ou `Controle`) :
   ```http
   GET /api/Telemetries/peripherique?peripheriqueId=1
   ```
4. **Envoie commande** (si `Controle` uniquement) :
   ```http
   POST /api/Commandes
   ```
5. **Consulte son historique** :
   ```http
   GET /api/user/activity/recent
   ```

### Scénario 2 : Admin attribue droits sur une zone

1. **Admin crée droit sur Zone** :
   ```http
   POST /api/admin/access-rights
   {
     "userId": "user-guid",
     "zoneId": 1,
     "type": "Controle"
   }
   ```
2. **Utilisateur hérite droits sur tous périphériques de la zone**
3. **Vérification automatique** via `ResourceAccessHandler.CheckPeripheriqueAccessAsync()`

### Scénario 3 : Utilisateur sans droit tente d'accéder

1. **Requête** :
   ```http
   GET /api/Telemetries/peripherique?peripheriqueId=999
   ```
2. **Handler vérifie** : pas de droit direct, pas de droit zone
3. **Réponse** : `403 Forbidden`
4. **Log** : Warning dans les logs serveur

## Intégration dans vos contrôleurs

### Exemple : sécuriser un endpoint

```csharp
[HttpGet("peripherique-data/{id}")]
public async Task<IActionResult> GetPeripheriqueData(int id)
{
    // Vérification des droits
    var hasAccess = await _authorizationService.CheckPeripheriqueAccessAsync(
        User,
        id,
        DroitAccesType.Lecture);

    if (!hasAccess)
    {
        _logger.LogWarning(
            "Accès refusé pour userId={UserId} sur périphérique {PeripheriqueId}",
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            id);
        return Forbid();
    }

    // Logique métier...
    var data = await _service.GetDataAsync(id);
    return Ok(data);
}
```

### Extensions disponibles

```csharp
// Pour périphériques
await _authorizationService.CheckPeripheriqueAccessAsync(User, peripheriqueId, DroitAccesType.Lecture);

// Pour zones
await _authorizationService.CheckZoneAccessAsync(User, zoneId, DroitAccesType.Controle);
```

## Configuration requise

### Program.cs

```csharp
// Enregistrer le handler
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, ResourceAccessHandler>();

// Enregistrer le service d'activité
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
```

### Usings requis

```csharp
using Microsoft.AspNetCore.Authorization;
using SmartOfficeAPP.Authorization;
using SmartOfficeAPP.Enums;
using SmartOfficeAPP.Extensions;
using System.Security.Claims;
```

## Tests

Utilisez le fichier `Tests/13-User-Activity-Authorization.http` pour :
- Tester l'historique d'activité
- Vérifier l'autorisation basée sur les ressources
- Tester le fallback par zone
- Valider les permissions Admin

## Bonnes pratiques

1. **Toujours vérifier les droits** avant d'accéder à une ressource sensible
2. **Logger les refus d'accès** pour audit et sécurité
3. **Privilégier le fallback zone** pour simplifier la gestion
4. **Admin bypass** : toujours permettre aux admins d'accéder
5. **Pagination** : limiter les résultats d'historique (max 100 par page)
6. **Filtrage** : permettre aux utilisateurs de filtrer leur historique

## Cas d'usage avancés

### Délégation temporaire

Pour implémenter une délégation temporaire de droits :
1. Ajouter champs `DateDebut` et `DateFin` à `DroitAcces`
2. Modifier `ResourceAccessHandler` pour vérifier les dates
3. Job de nettoyage des droits expirés

### Droits granulaires par action

Pour différencier lecture/écriture/suppression :
1. Étendre `DroitAccesType` avec plus de valeurs
2. Adapter les handlers
3. Vérifier le type d'action dans les contrôleurs

### Audit complet

Pour un audit détaillé :
1. Middleware qui enregistre toutes les requêtes
2. Intercepter les succès/échecs d'autorisation
3. Créer des rapports d'accès pour les admins

## Dépannage

### "403 Forbidden" inattendu
- Vérifier que l'utilisateur a bien un `DroitAcces` dans la DB
- Vérifier le type de droit (`Lecture` vs `Controle`)
- Consulter les logs serveur pour voir la raison du refus

### Fallback zone ne fonctionne pas
- Vérifier que `PeripheriqueLogique.ZoneId` est correctement défini
- Vérifier que le `DroitAcces` sur la zone existe
- Debug dans `ResourceAccessHandler.CheckPeripheriqueAccessAsync()`

### Historique vide
- Vérifier que des `Commandes` ou `JournalAudits` existent pour l'utilisateur
- Vérifier les filtres de date
- Consulter les logs du service

## Ressources

- **Models** : `SmartOfficeAPP/Models/DroitAcces.cs`
- **Handler** : `SmartOfficeAPP/Authorization/ResourceAccessHandler.cs`
- **Extensions** : `SmartOfficeAPP/Extensions/ResourceAuthorizationExtensions.cs`
- **Service activité** : `SmartOfficeAPP/Services/ServicesImpl/UserActivityService.cs`
- **Contrôleur** : `SmartOfficeAPP/Controllers/UserActivityController.cs`
- **Tests** : `SmartOfficeAPP/Tests/13-User-Activity-Authorization.http`
