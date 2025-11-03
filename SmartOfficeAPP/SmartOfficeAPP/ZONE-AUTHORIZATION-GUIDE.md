# Guide d'autorisation: Zones et Contrôle des Périphériques

## Architecture du système d'autorisation

### Modèle de données

```
Utilisateur (ApplicationUser)
  ? (via DroitAcces)
Zone ? Périphériques (PeripheriqueLogique)
    ?
Périphériques individuels
```

### Types de droits

- **`DroitAccesType.Lecture` (0)**: Consultation uniquement
  - Voir les données de télémétrie
  - Voir l'état des périphériques
  - ? NE peut PAS envoyer de commandes

- **`DroitAccesType.Controle` (1)**: Contrôle complet
  - Toutes les permissions de "Lecture"
  - ? Peut envoyer des commandes
  - ? Peut modifier l'état des périphériques

## Principe d'héritage Zone ? Périphériques

### ? Correction appliquée dans `CommandeService`

Quand un utilisateur tente de contrôler un périphérique, le système vérifie :

1. **Droit DIRECT sur le périphérique** : `DroitAcces` avec `PeripheriqueLogiqueId`
2. **Droit via ZONE parente** : `DroitAcces` avec `ZoneId` du périphérique

**Si l'une des deux conditions est vraie avec type "Controle" ? Accès autorisé**

### Code de vérification

```csharp
// 1. Charger le périphérique pour connaître sa Zone
var peripherique = await _peripheriqueRepository.GetByIdAsync(requestDto.PeripheriqueLogiqueId);

// 2. Vérifier droit direct sur périphérique
var hasDeviceControl = droitsPeripheriques.Any(d =>
    d.PeripheriqueLogiqueId == requestDto.PeripheriqueLogiqueId &&
    d.Type == DroitAccesType.Controle);

// 3. Vérifier droit sur zone parente
var hasZoneControl = droitsZones.Any(d =>
    d.ZoneId == peripherique.ZoneId &&
    d.Type == DroitAccesType.Controle);

// 4. Autoriser si l'un OU l'autre est vrai
var canControl = hasDeviceControl || hasZoneControl;
```

## Endpoints d'administration

### ? Affecter une zone entière à un utilisateur

```http
POST /api/admin/assign-zone-to-user
Content-Type: application/json

{
  "userId": "user-guid-here",
  "zoneId": 1,
  "accessType": 1  // 0=Lecture, 1=Contrôle
}
```

**Résultat** : L'utilisateur peut contrôler TOUS les périphériques de cette zone

### ? Affecter un périphérique spécifique

```http
POST /api/admin/assign-device-to-user
Content-Type: application/json

{
  "userId": "user-guid-here",
  "peripheriqueLogiqueId": 5,
  "accessType": 1
}
```

**Résultat** : L'utilisateur peut contrôler ce périphérique spécifique

## Cas d'usage recommandés

### 1. Gestion par zones (recommandé)

**Scenario** : Bureau d'étage, atelier, salle de réunion

```http
POST /api/admin/assign-zone-to-user
{
  "userId": "manager-office-a",
  "zoneId": 3,  // "Bureau Étage 2"
  "accessType": 1
}
```

**Avantages** :
- ? Un seul droit donne accès à tous les périphériques de la zone
- ? Nouveaux périphériques automatiquement accessibles
- ? Gestion simplifiée

### 2. Accès spécialisé par périphérique

**Scenario** : Équipement sensible, accès temporaire

```http
POST /api/admin/assign-device-to-user
{
  "userId": "maintenance-tech",
  "peripheriqueLogiqueId": 12,  // "Alarme Incendie Principale"
  "accessType": 1
}
```

**Avantages** :
- ? Contrôle granulaire
- ? Sécurité renforcée pour équipements critiques

## Validation du système

### Tests à effectuer

1. **Test héritage zone ? périphériques**
   ```bash
   # 1. Affecter utilisateur à zone avec contrôle
   # 2. Tenter commande sur périphérique de cette zone
   # 3. Vérifier succès + logs "Droit zone: True"
   ```

2. **Test accès direct périphérique**
   ```bash
   # 1. Affecter utilisateur à périphérique spécifique
   # 2. Tenter commande sur ce périphérique
   # 3. Vérifier succès + logs "Droit direct: True"
   ```

3. **Test refus d'accès**
   ```bash
   # 1. Utilisateur sans droit sur zone ni périphérique
   # 2. Tenter commande
   # 3. Vérifier échec + message "Accès refusé"
   ```

## Logs de débogage

Le système produit des logs détaillés :

```
// Accès autorisé
INFO: Contrôle autorisé pour user abc123 ? périphérique 5 (zone 2). Droit direct: False, Droit zone: True

// Accès refusé
WARN: Accès refusé (contrôle) user abc123 ? périphérique 5 (zone 2). Droit direct: False, Droit zone: False
```

## Migration et données existantes

Si vous avez des droits existants uniquement sur périphériques :

1. **Identifier les zones des périphériques** : `SELECT DISTINCT ZoneId FROM PeripheriqueLogique`
2. **Créer droits zone** : Pour chaque utilisateur ayant plusieurs périphériques d'une même zone
3. **Nettoyer droits redondants** : Supprimer les droits périphérique si droit zone suffisant

## Résumé

? **Implémenté** : Héritage zone ? périphériques dans `CommandeService`  
? **Implémenté** : Endpoints simplifiés d'affectation  
? **Implémenté** : Logs détaillés pour débogage  
? **Implémenté** : Tests de validation  

**Principe clé** : Affecter une zone à un utilisateur = contrôle de tous ses périphériques