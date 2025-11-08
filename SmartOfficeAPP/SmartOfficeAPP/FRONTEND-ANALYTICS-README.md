# ? BACKEND COMPLET - Analytics Énergétiques

**Date:** 6 janvier 2025  
**Status:** ? **100% PRÊT POUR LE FRONTEND**

---

## ?? Résumé en 30 Secondes

? **Backend:** 9 APIs fonctionnelles pour analytics énergétiques  
? **Calculs:** Statistiques, anomalies, recommandations automatiques  
? **Graphiques:** Données horaires et tendances disponibles  
? **Rapports:** Génération complète avec économies estimées  

---

## ?? APIs Disponibles (9 endpoints)

| Endpoint | Méthode | Description | Status |
|----------|---------|-------------|--------|
| `/api/ConsommationStats/peripherique/{id}` | GET | Stats par périphérique | ? |
| `/api/ConsommationStats/zone/{id}` | GET | Stats par zone | ? |
| `/api/ConsommationStats/peripherique/{id}/horaire` | GET | Données horaires | ? |
| `/api/ConsommationStats/peripherique/{id}/tendances` | GET | Tendances quotidiennes | ? |
| `/api/ConsommationStats/peripherique/{id}/anomalies` | GET | Détection anomalies | ? |
| `/api/ConsommationStats/rapport` | GET | Rapport complet | ? |
| `/api/ConsommationStats/peripherique/{id}/cout` | GET | Calcul coût | ? |
| `/api/ConsommationStats/top-consommateurs` | GET | Top consommateurs | ? |
| `/api/ConsommationStats/peripherique/{id}/comparaison` | GET | Comparaison périodes | ? |

---

## ?? DTOs Principaux

### ConsommationStatsDto
```json
{
  "peripheriqueId": 1,
  "peripheriqueName": "Climatiseur Salon",
  "consommationTotaleMwh": 145.7,
  "puissanceMoyenneMw": 6.2,
  "puissanceMaximaleMw": 8.5,
  "tempsFonctionnementHeures": 168.5,
  "efficaciteEnergetique": 0.78,
  "nombreCycles": 24
}
```

### ConsommationHoraireDto (pour graphiques)
```json
{
  "heure": "2025-01-06T14:00:00Z",
  "consommationMwh": 5.2,
  "puissanceMoyenneMw": 6.1,
  "etatActif": true,
  "nombreActivations": 2
}
```

### TopConsommateursDto
```json
{
  "peripheriqueId": 1,
  "peripheriqueName": "Climatiseur Salon",
  "peripheriqueType": "HVAC",
  "consommationMwh": 45.2,
  "pourcentageTotal": 31.0,
  "coutEstime": 5.89,
  "rang": 1
}
```

### AnomalieConsommationDto
```json
{
  "peripheriqueId": 1,
  "peripheriqueName": "Climatiseur Salon",
  "dateDetection": "2025-01-05T14:30:00Z",
  "typeAnomalie": "ConsommationElevee",
  "description": "Consommation 35% supérieure à la moyenne",
  "ecartPourcentage": 35.3,
  "severite": "High"
}
```

---

## ?? Frontend: À Faire (3-4 heures)

### 1. Service (45 min)
```csharp
// MySmartApp.Ui/Services/ConsommationStatsService.cs
public class ConsommationStatsService : IConsommationStatsService
{
    // 5 méthodes principales à implémenter
}
```

### 2. Remplacement données mockées (1h30)
```csharp
// ? Remplacer
await LoadMockData();
consommationStats = new ConsommationStatsDto(/* hardcoded */);

// ? Par
var response = await ConsommationService.GetConsommationStatsAsync(...);
consommationStats = response.Data;
```

### 3. Validation et erreurs (45 min)
```csharp
private bool ValidateParameters()
{
    if (!startDate.HasValue || !endDate.HasValue)
    {
        Snackbar.Add("?? Sélectionnez une période", Severity.Warning);
        return false;
    }
    return true;
}
```

### 4. Tests (45 min)
- Tester chaque endpoint avec fichier `.http`
- Valider l'affichage des graphiques
- Vérifier les messages d'erreur

---

## ?? Documentation Disponible

| Document | Usage | Temps |
|----------|-------|-------|
| **FRONTEND-ANALYTICS-QUICK-START.md** | Implémentation rapide | 3-4h |
| **FRONTEND-ANALYTICS-COMPLETE-GUIDE.md** | Référence détaillée | 15min lecture |
| **analytics-tests.http** | Tests des APIs | 10min |

---

## ?? Méthodes à Remplacer

| Méthode Actuelle (Mock) | Nouvelle Méthode (API) |
|-------------------------|------------------------|
| `LoadMockData()` | `LoadConsommationStats()` |
| `GenerateMockConsommationHoraire()` | `LoadConsommationHoraire()` |
| Données en dur topConsommateurs | `LoadTopConsommateurs()` |
| ? Pas de vraie détection | `DetectAnomalies()` |
| ? Rapport mocké | `GenererRapport()` |

---

## ?? Quick Start 3 Étapes

### Étape 1: Créer le service (30 min)
```bash
# Créer les fichiers
IConsommationStatsService.cs
ConsommationStatsService.cs

# Enregistrer dans Program.cs
builder.Services.AddScoped<IConsommationStatsService, ConsommationStatsService>();
```

### Étape 2: Remplacer les appels (2h)
```csharp
// Dans Analytics.razor
// Remplacer toutes les méthodes LoadMock*() par les vraies APIs
await ConsommationService.GetConsommationStatsAsync(...)
await ConsommationService.GetTopConsommateursAsync(...)
await ConsommationService.DetectAnomaliesAsync(...)
```

### Étape 3: Tester (1h)
```bash
# Lancer le backend
cd SmartOfficeAPP
dotnet run

# Tester avec analytics-tests.http
# Vérifier chaque endpoint retourne des données

# Lancer le frontend et tester
cd MySmartApp.Ui
dotnet run
```

---

## ? Checklist de Validation

### Backend (Terminé)
- [x] ? 9 APIs fonctionnelles
- [x] ? Calculs statistiques automatiques
- [x] ? Détection d'anomalies ML
- [x] ? Génération de recommandations
- [x] ? Support graphiques temps réel

### Frontend (À faire)
- [ ] Service créé et enregistré
- [ ] Méthodes mockées remplacées
- [ ] Validation des paramètres
- [ ] Gestion d'erreurs avec Snackbar
- [ ] Graphiques connectés aux vraies données
- [ ] Tests complets réussis

---

## ?? Points d'Attention

### 1. Format des dates
```csharp
// ? Correct
?startDate=2025-01-01&endDate=2025-01-07

// ? Incorrect
?startDate=01/01/2025  // Format invalide
```

### 2. Validation période
```csharp
// Maximum 365 jours
if ((endDate - startDate).TotalDays > 365)
{
    return BadRequest("Période trop longue");
}
```

### 3. Gestion des listes vides
```csharp
// Toujours vérifier avant d'utiliser
if (!consommationHoraire.Any())
{
    Logger.LogWarning("Aucune donnée horaire");
    return new List<ChartSeries>();
}
```

### 4. Types d'anomalies
```csharp
// 3 types possibles
"ConsommationElevee"    // Pic anormal
"ConsommationAnormale"  // Pattern étrange
"ArretInattendu"        // Arrêt soudain
```

### 5. Niveaux de sévérité
```csharp
// 3 niveaux
"High"    // Critique (rouge)
"Medium"  // Moyen (orange)
"Low"     // Faible (bleu)
```

---

## ?? Comparaison Avant/Après

### Avant (Données Mockées)
```csharp
consommationStats = new ConsommationStatsDto(
    PeripheriqueId: 1,
    PeripheriqueName: "Climatiseur Salon",
    ConsommationTotaleMwh: 145.7,  // ? Hardcodée
    // ...
);
```

### Après (Données Réelles)
```csharp
var response = await ConsommationService.GetConsommationStatsAsync(
    peripheriqueId, startDate, endDate);

if (response.IsSuccess)
{
    consommationStats = response.Data;  // ? Vraies données
}
```

---

## ?? Exemples de Requêtes

### Stats périphérique
```http
GET https://localhost:7144/api/ConsommationStats/peripherique/1
    ?startDate=2025-01-01
    &endDate=2025-01-07
```

### Top consommateurs
```http
GET https://localhost:7144/api/ConsommationStats/top-consommateurs
    ?startDate=2025-01-01
    &endDate=2025-01-07
    &top=10
```

### Anomalies
```http
GET https://localhost:7144/api/ConsommationStats/peripherique/1/anomalies
    ?startDate=2025-01-01
    &endDate=2025-01-07
```

### Rapport complet
```http
GET https://localhost:7144/api/ConsommationStats/rapport
    ?zoneId=1
    &startDate=2025-01-01
    &endDate=2025-01-07
```

---

## ?? Conseils Pratiques

### 1. Commencer simple
- Implémenter d'abord `GetConsommationStatsAsync()`
- Tester avec un seul périphérique
- Ajouter progressivement les autres fonctionnalités

### 2. Logger généreusement
```csharp
Logger.LogInformation("Chargement stats: peripherique={Id}, période={Days}j", 
    peripheriqueId, (endDate - startDate).TotalDays);
```

### 3. Feedback utilisateur
```csharp
Snackbar.Add("? Données chargées", Severity.Success);
Snackbar.Add("?? Aucune donnée", Severity.Warning);
Snackbar.Add("? Erreur de chargement", Severity.Error);
```

### 4. Gestion des cas limites
```csharp
// Pas de données horaires?
if (!consommationHoraire.Any())
{
    // Afficher un message plutôt qu'un graphique vide
    return;
}

// Pas d'anomalies?
if (!anomalies.Any())
{
    Snackbar.Add("? Aucune anomalie", Severity.Success);
}
```

---

## ?? Support

### Questions Fréquentes

**Q: Combien de temps pour implémenter?**  
A: 3-4 heures avec le Quick Start

**Q: Les APIs sont-elles toutes prêtes?**  
A: ? Oui, 100% fonctionnelles et testées

**Q: Faut-il des données en base?**  
A: Oui, il faut des télémétries pour avoir des statistiques

**Q: Comment tester sans données?**  
A: Utiliser les endpoints de seeding pour créer des données de test

**Q: Les graphiques fonctionnent-ils?**  
A: Oui, une fois les données chargées via les APIs

---

## ?? Résumé

### ? Vous AVEZ
- 9 APIs analytics complètes
- Calculs statistiques automatiques
- Détection d'anomalies ML
- Génération de rapports avec recommandations
- Support graphiques temps réel

### ?? Vous DEVEZ
1. Créer le service frontend (45 min)
2. Remplacer les données mockées (1h30)
3. Ajouter validation et erreurs (45 min)
4. Tester (45 min)

### ?? Résultat Final
Une page Analytics complète avec:
- Statistiques en temps réel
- Graphiques interactifs
- Détection d'anomalies
- Top consommateurs
- Rapports détaillés avec recommandations

---

**?? TIP:** Commencer par `FRONTEND-ANALYTICS-QUICK-START.md` pour une implémentation guidée pas-à-pas !

**?? Aide:** Consulter `FRONTEND-ANALYTICS-COMPLETE-GUIDE.md` pour la documentation complète des APIs.
