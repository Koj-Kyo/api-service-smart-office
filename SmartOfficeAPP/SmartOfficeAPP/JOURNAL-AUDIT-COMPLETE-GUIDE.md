# ?? Journal d'Audit - API Historique Utilisateur

## ?? Vue d'ensemble

L'API de **Journal d'Audit** permet de consulter l'historique des actions effectuées par les utilisateurs dans l'application SmartOffice. Cette fonctionnalité est essentielle pour :

- ? **Traçabilité** : Suivi de toutes les actions utilisateur
- ? **Sécurité** : Détection d'activités suspectes
- ? **Conformité** : Respect RGPD et audits internes
- ? **Debug** : Analyse des problèmes de production
- ? **Analytics** : Statistiques d'utilisation

---

## ?? Fonctionnalités

### Pour tous les utilisateurs
- ?? Consulter son propre historique d'actions
- ?? Filtrer par période (dates)
- ?? Limiter le nombre de résultats

### Pour les administrateurs
- ?? Consulter l'historique de tous les utilisateurs
- ?? Filtrer par utilisateur spécifique
- ?? Visualiser des statistiques d'utilisation
- ?? Analyser les actions les plus fréquentes

---

## ?? Endpoints disponibles

### 1?? Mon historique (Utilisateur)

```http
GET /api/JournalAudit/my
```

**Authentification** : Bearer Token (tous utilisateurs)

**Paramètres de requête** :

| Paramètre | Type | Requis | Description | Défaut |
|-----------|------|--------|-------------|--------|
| `start` | DateTime | Non | Date de début (ISO 8601) | - |
| `end` | DateTime | Non | Date de fin (ISO 8601) | - |
| `limit` | int | Non | Nombre max de résultats (1-1000) | 100 |

**Exemple de requête** :

```http
GET /api/JournalAudit/my?start=2025-01-01&end=2025-01-31&limit=50
Authorization: Bearer eyJhbGc...
```

**Réponse** (200 OK) :

```json
[
  {
    "horodatage": "2025-01-15T14:30:22.123Z",
    "action": "POST /api/Zone",
    "contexte": "API Request"
  },
  {
    "horodatage": "2025-01-15T14:25:15.456Z",
    "action": "GET /api/Devices",
    "contexte": "API Request"
  },
  {
    "horodatage": "2025-01-15T14:20:10.789Z",
    "action": "PUT /api/Commands/123",
    "contexte": "API Request"
  }
]
```

---

### 2?? Historique complet (Admin)

```http
GET /api/JournalAudit/all
```

**Authentification** : Bearer Token (Admin uniquement)

**Paramètres de requête** :

| Paramètre | Type | Requis | Description | Défaut |
|-----------|------|--------|-------------|--------|
| `userId` | string | Non | Filtrer par utilisateur | - |
| `start` | DateTime | Non | Date de début | - |
| `end` | DateTime | Non | Date de fin | - |
| `limit` | int | Non | Nombre max de résultats (1-2000) | 500 |

**Exemple de requête** :

```http
GET /api/JournalAudit/all?start=2025-01-01&limit=100
Authorization: Bearer eyJhbGc...
```

**Réponse** (200 OK) :

```json
[
  {
    "horodatage": "2025-01-15T14:30:22.123Z",
    "action": "POST /api/Zone",
    "contexte": "API Request",
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "userName": "john.doe",
    "userEmail": "john@smartoffice.com",
    "userFullName": "John Doe"
  },
  {
    "horodatage": "2025-01-15T14:28:10.456Z",
    "action": "DELETE /api/Devices/ESP32-001",
    "contexte": "API Request",
    "userId": "650e8400-e29b-41d4-a716-446655440001",
    "userName": "admin",
    "userEmail": "adminer@smartoffice.com",
    "userFullName": "Administrator"
  }
]
```

---

### 3?? Historique utilisateur spécifique (Admin)

```http
GET /api/JournalAudit/user/{userId}
```

**Authentification** : Bearer Token (Admin uniquement)

**Paramètres de route** :

| Paramètre | Type | Requis | Description |
|-----------|------|--------|-------------|
| `userId` | string | Oui | ID de l'utilisateur |

**Paramètres de requête** :

| Paramètre | Type | Requis | Description | Défaut |
|-----------|------|--------|-------------|--------|
| `start` | DateTime | Non | Date de début | - |
| `end` | DateTime | Non | Date de fin | - |
| `limit` | int | Non | Nombre max de résultats (1-1000) | 200 |

**Exemple de requête** :

```http
GET /api/JournalAudit/user/550e8400-e29b-41d4-a716-446655440000?limit=50
Authorization: Bearer eyJhbGc...
```

---

### 4?? Statistiques d'audit (Admin)

```http
GET /api/JournalAudit/stats
```

**Authentification** : Bearer Token (Admin uniquement)

**Paramètres de requête** :

| Paramètre | Type | Requis | Description | Défaut |
|-----------|------|--------|-------------|--------|
| `days` | int | Non | Nombre de jours à analyser (1-365) | 30 |

**Exemple de requête** :

```http
GET /api/JournalAudit/stats?days=7
Authorization: Bearer eyJhbGc...
```

**Réponse** (200 OK) :

```json
{
  "totalActions": 1250,
  "uniqueUsers": 12,
  "periodDays": 7,
  "startDate": "2025-01-08T00:00:00Z",
  "endDate": "2025-01-15T14:30:22Z",
  "topActions": [
    {
      "action": "GET /api/Devices",
      "count": 450
    },
    {
      "action": "POST /api/Commands",
      "count": 320
    },
    {
      "action": "GET /api/Telemetry",
      "count": 185
    }
  ],
  "actionsByDay": [
    {
      "date": "2025-01-15T00:00:00Z",
      "count": 85
    },
    {
      "date": "2025-01-14T00:00:00Z",
      "count": 72
    }
  ]
}
```

---

## ?? Configuration Backend

### Étape 1 : Activer la journalisation automatique

Pour enregistrer **automatiquement** toutes les actions API, ajoutez le filtre global dans `Program.cs` :

```csharp
// Dans Program.cs, après builder.Services.AddControllers()
builder.Services.AddControllers(options =>
{
    // ? Active la journalisation automatique de toutes les requêtes API
    options.Filters.Add<SmartOfficeAPP.Filters.JournalAuditFilter>();
});
```

### Étape 2 : Journalisation manuelle (optionnel)

Si vous voulez journaliser des actions spécifiques dans votre code :

```csharp
public class MonService
{
    private readonly IJournalAuditService _auditService;

    public async Task ExecuterActionImportante(string userId)
    {
        // ... votre logique métier ...

        // ? Journaliser l'action
        await _auditService.LogActionAsync(
            userId: userId,
            action: "Action critique exécutée",
            contexte: "Service métier"
        );
    }
}
```

### Étape 3 : Optimisation base de données

Pour améliorer les performances sur de gros volumes, ajoutez un index :

```sql
-- Migration EF Core ou script SQL manuel
CREATE INDEX IX_JournalAudit_Horodatage 
ON JournalAudits(Horodatage DESC);

CREATE INDEX IX_JournalAudit_UserId_Horodatage 
ON JournalAudits(UserId, Horodatage DESC);
```

---

## ?? Intégration Frontend

### Exemple JavaScript/TypeScript

```typescript
// Service API
class JournalAuditService {
  private baseUrl = 'https://smartoffice-api-free.azurewebsites.net/api';
  
  async getMyHistory(params?: {
    start?: string;
    end?: string;
    limit?: number;
  }): Promise<JournalAuditDto[]> {
    const token = localStorage.getItem('token');
    const queryParams = new URLSearchParams(params as any).toString();
    
    const response = await fetch(
      `${this.baseUrl}/JournalAudit/my?${queryParams}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );
    
    if (!response.ok) {
      throw new Error('Erreur lors de la récupération de l\'historique');
    }
    
    return response.json();
  }
  
  // Pour Admin uniquement
  async getAllHistory(params?: {
    userId?: string;
    start?: string;
    end?: string;
    limit?: number;
  }): Promise<JournalAuditDetailDto[]> {
    const token = localStorage.getItem('token');
    const queryParams = new URLSearchParams(params as any).toString();
    
    const response = await fetch(
      `${this.baseUrl}/JournalAudit/all?${queryParams}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    
    return response.json();
  }
  
  async getStats(days: number = 30): Promise<AuditStatsDto> {
    const token = localStorage.getItem('token');
    
    const response = await fetch(
      `${this.baseUrl}/JournalAudit/stats?days=${days}`,
      {
        headers: {
          'Authorization': `Bearer ${token}`
        }
      }
    );
    
    return response.json();
  }
}

// Types TypeScript
interface JournalAuditDto {
  horodatage: string;
  action: string;
  contexte: string;
}

interface JournalAuditDetailDto extends JournalAuditDto {
  userId: string;
  userName?: string;
  userEmail?: string;
  userFullName?: string;
}

interface AuditStatsDto {
  totalActions: number;
  uniqueUsers: number;
  periodDays: number;
  startDate: string;
  endDate: string;
  topActions: ActionCount[];
  actionsByDay: DailyActionCount[];
}

interface ActionCount {
  action: string;
  count: number;
}

interface DailyActionCount {
  date: string;
  count: number;
}
```

### Exemple React Component

```tsx
import React, { useEffect, useState } from 'react';

const UserHistoryPage: React.FC = () => {
  const [history, setHistory] = useState<JournalAuditDto[]>([]);
  const [loading, setLoading] = useState(true);
  
  useEffect(() => {
    const fetchHistory = async () => {
      try {
        const service = new JournalAuditService();
        const data = await service.getMyHistory({ limit: 50 });
        setHistory(data);
      } catch (error) {
        console.error('Erreur:', error);
      } finally {
        setLoading(false);
      }
    };
    
    fetchHistory();
  }, []);
  
  if (loading) return <div>Chargement...</div>;
  
  return (
    <div className="history-container">
      <h1>Mon Historique</h1>
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Action</th>
            <th>Contexte</th>
          </tr>
        </thead>
        <tbody>
          {history.map((entry, index) => (
            <tr key={index}>
              <td>{new Date(entry.horodatage).toLocaleString()}</td>
              <td>{entry.action}</td>
              <td>{entry.contexte}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
```

---

## ?? Intégration MAUI (.NET)

```csharp
// Service API
public class JournalAuditApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://smartoffice-api-free.azurewebsites.net/api";
    
    public JournalAuditApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<JournalAuditDto>> GetMyHistoryAsync(
        DateTime? start = null,
        DateTime? end = null,
        int limit = 100)
    {
        var queryParams = new List<string>();
        
        if (start.HasValue)
            queryParams.Add($"start={start.Value:yyyy-MM-ddTHH:mm:ssZ}");
        if (end.HasValue)
            queryParams.Add($"end={end.Value:yyyy-MM-ddTHH:mm:ssZ}");
        queryParams.Add($"limit={limit}");
        
        var query = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"{_baseUrl}/JournalAudit/my?{query}");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<List<JournalAuditDto>>();
    }
}

// Page XAML
public partial class HistoryPage : ContentPage
{
    private readonly JournalAuditApiService _apiService;
    
    public HistoryPage(JournalAuditApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistoryAsync();
    }
    
    private async Task LoadHistoryAsync()
    {
        try
        {
            var history = await _apiService.GetMyHistoryAsync(limit: 50);
            HistoryListView.ItemsSource = history;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger l'historique: {ex.Message}", "OK");
        }
    }
}
```

---

## ??? Sécurité & RGPD

### Permissions

| Endpoint | Rôle requis | Description |
|----------|-------------|-------------|
| `GET /my` | Utilisateur authentifié | Accès à son propre historique |
| `GET /all` | Admin | Accès à tout l'historique |
| `GET /user/{userId}` | Admin | Accès à l'historique d'un utilisateur |
| `GET /stats` | Admin | Statistiques globales |

### Conformité RGPD

? **Droit d'accès** : Utilisateurs peuvent consulter leurs données (`/my`)  
? **Droit à l'oubli** : Implémenter suppression sur demande (à ajouter)  
? **Limitation de conservation** : Archiver après 365 jours  
? **Minimisation** : Seules actions nécessaires journalisées

**Recommandation** : Ajouter un endpoint de suppression pour conformité RGPD :

```csharp
[HttpDelete("my")]
[Authorize]
public async Task<IActionResult> DeleteMyHistory()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    await _journalAuditService.DeleteUserHistoryAsync(userId);
    return NoContent();
}
```

---

## ?? Déploiement Azure

### Variables d'environnement (optionnel)

Si vous voulez configurer la rétention d'historique :

```bash
# Dans Azure App Service ? Configuration ? Application Settings
Name  : JournalAudit__RetentionDays
Value : 365

Name  : JournalAudit__ArchiveOldLogs
Value : true
```

### Logs applicatifs

Activer les logs Azure pour surveiller l'usage :

```bash
az webapp log config \
  --name smartoffice-api-free \
  --resource-group smartoffice-free-rg \
  --application-logging filesystem
```

---

## ?? Monitoring & Analytics

### Requêtes utiles pour analyse

```sql
-- Top 10 actions les plus fréquentes
SELECT Action, COUNT(*) as Count
FROM JournalAudits
WHERE Horodatage >= DATEADD(day, -30, GETUTCDATE())
GROUP BY Action
ORDER BY Count DESC
LIMIT 10;

-- Utilisateurs les plus actifs
SELECT UserId, COUNT(*) as ActionCount
FROM JournalAudits
WHERE Horodatage >= DATEADD(day, -7, GETUTCDATE())
GROUP BY UserId
ORDER BY ActionCount DESC
LIMIT 10;

-- Actions par heure de la journée
SELECT DATEPART(HOUR, Horodatage) as Hour, COUNT(*) as Count
FROM JournalAudits
WHERE Horodatage >= DATEADD(day, -1, GETUTCDATE())
GROUP BY DATEPART(HOUR, Horodatage)
ORDER BY Hour;
```

---

## ? FAQ

### Q : Toutes les actions sont-elles enregistrées automatiquement ?

**R** : Seulement si vous activez le `JournalAuditFilter` global dans `Program.cs`. Sinon, seules les actions journalisées manuellement avec `LogActionAsync()` sont enregistrées.

### Q : Quelle est la durée de conservation recommandée ?

**R** : 90-365 jours selon la réglementation de votre secteur. Pour RGPD, maximum 13 mois sauf obligation légale spécifique.

### Q : Peut-on anonymiser les historiques ?

**R** : Oui, remplacer `UserId` par un hash ou supprimer les champs identifiants après la période légale.

### Q : Comment gérer les volumes importants ?

**R** :
- Ajouter des index sur `Horodatage` et `UserId`
- Archiver les logs anciens dans Azure Blob Storage
- Utiliser une table partitionnée par mois
- Limiter les requêtes avec pagination stricte

---

## ?? Ressources

- **Tests HTTP** : `/Tests/41-JournalAudit-History.http`
- **Modèle** : `/Models/JournalAudit.cs`
- **Service** : `/Services/ServicesImpl/JournalAuditService.cs`
- **Contrôleur** : `/Controllers/JournalAuditController.cs`

---

## ? Checklist de déploiement

- [ ] Activer `JournalAuditFilter` dans `Program.cs`
- [ ] Ajouter index sur `Horodatage` et `UserId`
- [ ] Tester les 4 endpoints avec différents rôles
- [ ] Configurer rétention d'historique (365 jours)
- [ ] Documenter politique RGPD interne
- [ ] Implémenter archivage automatique (optionnel)
- [ ] Tester performances avec gros volumes (>10k entrées)
- [ ] Configurer alertes Azure sur erreurs 500

---

**?? API Journal d'Audit opérationnelle !**

Vos utilisateurs peuvent maintenant consulter leur historique d'actions, et les admins disposent d'outils d'analyse complets pour la traçabilité et la sécurité.
