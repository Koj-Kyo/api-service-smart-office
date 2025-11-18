# ?? CORRECTION - API Historique des Activités (Backend Réel)

## ?? **AVERTISSEMENT**

Le guide `MAUI-HISTORY-COMPLETE-GUIDE.md` contient des **erreurs**. Les endpoints documentés **NE CORRESPONDENT PAS** à l'API réelle du backend.

Ce document fournit la **correction complète** basée sur l'implémentation réelle du backend.

---

## ?? **DIFFÉRENCES PRINCIPALES**

| Aspect | Guide MAUI (Incorrect) | Backend Réel (Correct) |
|--------|------------------------|------------------------|
| **Route de base** | `/api/UserActivity` | `/api/user/activity` |
| **Authentification** | User ID en paramètre | User ID depuis JWT (automatique) |
| **Modèle de données** | Enums complexes | Strings simples |
| **Filtrage** | Par enums | Par query params |

---

## ?? **ENDPOINTS API RÉELS**

### **1. Historique Paginé**

```http
GET /api/user/activity/history
Authorization: Bearer {token}
```

**Query Parameters:**
- `pageNumber` (int, défaut: 1)
- `pageSize` (int, défaut: 20, max: 100)
- `startDate` (DateTime?, optionnel)
- `endDate` (DateTime?, optionnel)
- `activityType` (string?, optionnel) : "Commande", "Audit"

**Réponse:**
```json
{
  "activities": [
    {
      "id": 1,
      "timestamp": "2025-01-15T10:30:00Z",
      "activityType": "Commande",
      "description": "Allumage lampe LED",
      "targetResource": "Périphérique: Lampe LED",
      "targetResourceId": 5,
      "resourceType": "Périphérique",
      "status": "Succès",
      "details": null
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

### **2. Statistiques**

```http
GET /api/user/activity/stats?startDate=2025-01-01&endDate=2025-01-31
Authorization: Bearer {token}
```

**Réponse:**
```json
{
  "totalActivities": 45,
  "commandesExecutees": 38,
  "commandesEchouees": 2,
  "consultationsZones": 10,
  "consultationsPeripheriques": 15,
  "activitiesByType": {
    "Commande": 40,
    "Consultation": 5
  },
  "recentActivities": [...],
  "lastActivity": "2025-01-15T14:30:00Z",
  "activitiesByDay": {
    "2025-01-15": 5,
    "2025-01-14": 8
  }
}
```

### **3. Activités Récentes (24h)**

```http
GET /api/user/activity/recent
Authorization: Bearer {token}
```

**Réponse:**
```json
[
  {
    "id": 1,
    "timestamp": "2025-01-15T14:30:00Z",
    "activityType": "Commande",
    "description": "Extinction climatisation",
    "targetResource": "Zone: Bureau principal",
    "status": "Succès"
  }
]
```

---

## ?? **MODÈLES DE DONNÉES CORRIGÉS**

### **UserActivity.cs** (Version Corrigée)

```csharp
namespace SmartOfficeMobile.Models;

public class UserActivity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty; // "Commande", "Consultation", "Modification"
    public string Description { get; set; } = string.Empty;
    public string? TargetResource { get; set; } // "Zone: Bureau 1", "Périphérique: Lampe LED"
    public int? TargetResourceId { get; set; }
    public string? ResourceType { get; set; } // "Zone", "Périphérique"
    public string Status { get; set; } = string.Empty; // "Succès", "Échec"
    public string? Details { get; set; }
}
```

### **ActivityStats.cs** (Version Corrigée)

```csharp
namespace SmartOfficeMobile.Models;

public class ActivityStats
{
    public int TotalActivities { get; set; }
    public int CommandesExecutees { get; set; }
    public int CommandesEchouees { get; set; }
    public int ConsultationsZones { get; set; }
    public int ConsultationsPeripheriques { get; set; }
    public Dictionary<string, int> ActivitiesByType { get; set; } = new();
    public List<UserActivity> RecentActivities { get; set; } = new();
    public DateTime? LastActivity { get; set; }
    public Dictionary<string, int> ActivitiesByDay { get; set; } = new();
}
```

---

## ?? **SERVICE CORRIGÉ**

### **UserActivityService.cs** (Version Corrigée)

```csharp
namespace SmartOfficeMobile.Services;

public class UserActivityService
{
    private readonly ApiService _apiService;

    public UserActivityService(ApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Récupérer l'historique paginé de l'utilisateur connecté
    /// GET /api/user/activity/history
    /// </summary>
    public async Task<(List<UserActivity> activities, int totalCount)?> GetUserActivitiesAsync(
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? activityType = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ssZ}");

            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ssZ}");

            if (!string.IsNullOrEmpty(activityType))
                queryParams.Add($"activityType={Uri.EscapeDataString(activityType)}");

            var endpoint = $"/api/user/activity/history?{string.Join("&", queryParams)}";
            
            var response = await _apiService.GetAsync<ActivityResponse>(endpoint);
            
            if (response != null)
            {
                return (response.Activities, response.Pagination.TotalCount);
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération activités: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Récupérer les activités récentes (24h)
    /// GET /api/user/activity/recent
    /// </summary>
    public async Task<List<UserActivity>?> GetRecentActivitiesAsync()
    {
        try
        {
            return await _apiService.GetAsync<List<UserActivity>>("/api/user/activity/recent");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération activités récentes: {ex.Message}");
            return new List<UserActivity>();
        }
    }

    /// <summary>
    /// Récupérer les statistiques d'activité
    /// GET /api/user/activity/stats
    /// </summary>
    public async Task<ActivityStats?> GetActivityStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var queryParams = new List<string>();

            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-ddTHH:mm:ssZ}");

            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-ddTHH:mm:ssZ}");

            var endpoint = queryParams.Any()
                ? $"/api/user/activity/stats?{string.Join("&", queryParams)}"
                : "/api/user/activity/stats";

            return await _apiService.GetAsync<ActivityStats>(endpoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération stats: {ex.Message}");
            return null;
        }
    }

    // =============================
    // FILTRAGE LOCAL
    // =============================

    /// <summary>
    /// Filtrer les activités par période (côté client)
    /// </summary>
    public List<UserActivity> FilterByPeriod(List<UserActivity> activities, string period)
    {
        var now = DateTime.Now;
        return period switch
        {
            "today" => activities.Where(a => a.Timestamp.Date == now.Date).ToList(),
            "week" => activities.Where(a => a.Timestamp >= now.AddDays(-7)).ToList(),
            "month" => activities.Where(a => a.Timestamp >= now.AddMonths(-1)).ToList(),
            _ => activities
        };
    }

    /// <summary>
    /// Filtrer les activités par type (côté client)
    /// </summary>
    public List<UserActivity> FilterByType(List<UserActivity> activities, string activityType)
    {
        if (string.IsNullOrEmpty(activityType))
            return activities;

        return activities.Where(a => a.ActivityType.Equals(activityType, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Rechercher dans les activités
    /// </summary>
    public List<UserActivity> Search(List<UserActivity> activities, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return activities;

        searchTerm = searchTerm.ToLower();
        return activities.Where(a =>
            a.Description.ToLower().Contains(searchTerm) ||
            (a.TargetResource?.ToLower().Contains(searchTerm) ?? false)
        ).ToList();
    }

    // =============================
    // HELPERS
    // =============================

    /// <summary>
    /// Obtenir la couleur d'un type d'activité
    /// </summary>
    public static Color GetActivityTypeColor(string activityType)
    {
        return activityType switch
        {
            "Commande" => Color.FromArgb("#4A90E2"),      // Bleu
            "Consultation" => Color.FromArgb("#4ECDC4"),  // Turquoise
            "Modification" => Color.FromArgb("#FFB74D"),  // Orange
            "Audit" => Color.FromArgb("#9575CD"),         // Violet
            _ => Color.FromArgb("#6C757D")                // Gris
        };
    }

    /// <summary>
    /// Obtenir l'icône d'un type d'activité
    /// </summary>
    public static string GetActivityTypeIcon(string activityType)
    {
        return activityType switch
        {
            "Commande" => "??",
            "Consultation" => "???",
            "Modification" => "??",
            "Audit" => "??",
            _ => "??"
        };
    }

    /// <summary>
    /// Obtenir la couleur du statut
    /// </summary>
    public static Color GetStatusColor(string status)
    {
        return status switch
        {
            "Succès" => Color.FromArgb("#4ECDC4"),  // Vert
            "Échec" => Color.FromArgb("#FF6B6B"),   // Rouge
            _ => Color.FromArgb("#ADB5BD")          // Gris
        };
    }

    /// <summary>
    /// Formater le temps relatif
    /// </summary>
    public static string GetRelativeTime(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "À l'instant";
        if (timeSpan.TotalMinutes < 60)
            return $"Il y a {(int)timeSpan.TotalMinutes} min";
        if (timeSpan.TotalHours < 24)
            return $"Il y a {(int)timeSpan.TotalHours}h";
        if (timeSpan.TotalDays < 7)
            return $"Il y a {(int)timeSpan.TotalDays} jours";

        return dateTime.ToString("dd/MM/yyyy");
    }
}

// =============================
// CLASSE AUXILIAIRE POUR LA RÉPONSE PAGINÉE
// =============================

public class ActivityResponse
{
    public List<UserActivity> Activities { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}
```

---

## ?? **EXEMPLE D'UTILISATION CORRIGÉ**

### **Récupérer l'historique**

```csharp
// Page 1, 20 éléments
var result = await activityService.GetUserActivitiesAsync(pageNumber: 1, pageSize: 20);

if (result != null)
{
    var (activities, totalCount) = result.Value;
    Console.WriteLine($"Total: {totalCount} activités");
    
    foreach (var activity in activities)
    {
        Console.WriteLine($"{activity.Timestamp:HH:mm} - {activity.Description}");
    }
}
```

### **Récupérer avec filtres**

```csharp
// Activités des 7 derniers jours, type "Commande"
var startDate = DateTime.UtcNow.AddDays(-7);
var result = await activityService.GetUserActivitiesAsync(
    pageNumber: 1,
    pageSize: 50,
    startDate: startDate,
    activityType: "Commande");
```

### **Récupérer les statistiques**

```csharp
// Stats des 30 derniers jours
var startDate = DateTime.UtcNow.AddDays(-30);
var stats = await activityService.GetActivityStatsAsync(startDate: startDate);

if (stats != null)
{
    Console.WriteLine($"Total: {stats.TotalActivities}");
    Console.WriteLine($"Commandes réussies: {stats.CommandesExecutees}");
    Console.WriteLine($"Commandes échouées: {stats.CommandesEchouees}");
}
```

### **Récupérer les activités récentes**

```csharp
// Dernières 24 heures
var recentActivities = await activityService.GetRecentActivitiesAsync();

foreach (var activity in recentActivities ?? new List<UserActivity>())
{
    Console.WriteLine($"{activity.ActivityType} - {activity.Description}");
}
```

---

## ?? **PAGE CODE-BEHIND CORRIGÉE**

```csharp
namespace SmartOfficeMobile.Pages;

public class HistoryPage : ContentPage
{
    private readonly UserActivityService _activityService;
    private StackLayout _historyStack;
    private List<UserActivity> _currentActivities = new();
    private int _currentPage = 1;
    private int _totalPages = 1;

    public HistoryPage(UserActivityService activityService)
    {
        _activityService = activityService;
        Title = "Historique";
        BackgroundColor = Color.FromHex("#F5F5F7");
        
        CreateUI();
        LoadActivities();
    }

    private void CreateUI()
    {
        var scrollView = new ScrollView();
        var mainStack = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 20
        };

        // Header
        mainStack.Children.Add(new Label
        {
            Text = "Historique des activités",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold
        });

        // Filtres de période
        var filterGrid = new Grid
        {
            ColumnDefinitions = { new(), new(), new() },
            ColumnSpacing = 8
        };

        var todayBtn = new Button
        {
            Text = "Aujourd'hui",
            BackgroundColor = Color.FromHex("#4A90E2"),
            TextColor = Colors.White,
            FontSize = 12,
            CornerRadius = 8
        };
        todayBtn.Clicked += (s, e) => FilterByPeriod("today");

        var weekBtn = new Button
        {
            Text = "Semaine",
            BackgroundColor = Color.FromHex("#F0F4F8"),
            TextColor = Color.FromHex("#1A1A1A"),
            FontSize = 12,
            CornerRadius = 8
        };
        weekBtn.Clicked += (s, e) => FilterByPeriod("week");

        var monthBtn = new Button
        {
            Text = "Mois",
            BackgroundColor = Color.FromHex("#F0F4F8"),
            TextColor = Color.FromHex("#1A1A1A"),
            FontSize = 12,
            CornerRadius = 8
        };
        monthBtn.Clicked += (s, e) => FilterByPeriod("month");

        Grid.SetColumn(todayBtn, 0);
        Grid.SetColumn(weekBtn, 1);
        Grid.SetColumn(monthBtn, 2);
        filterGrid.Children.Add(todayBtn);
        filterGrid.Children.Add(weekBtn);
        filterGrid.Children.Add(monthBtn);
        mainStack.Children.Add(filterGrid);

        // Liste
        _historyStack = new StackLayout { Spacing = 8 };
        mainStack.Children.Add(_historyStack);

        // Pagination
        var paginationStack = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var prevBtn = new Button
        {
            Text = "< Précédent",
            BackgroundColor = Color.FromHex("#F0F4F8"),
            TextColor = Color.FromHex("#1A1A1A"),
            FontSize = 12
        };
        prevBtn.Clicked += OnPreviousPageClicked;

        var nextBtn = new Button
        {
            Text = "Suivant >",
            BackgroundColor = Color.FromHex("#4A90E2"),
            TextColor = Colors.White,
            FontSize = 12
        };
        nextBtn.Clicked += OnNextPageClicked;

        paginationStack.Children.Add(prevBtn);
        paginationStack.Children.Add(nextBtn);
        mainStack.Children.Add(paginationStack);

        scrollView.Content = mainStack;
        Content = scrollView;
    }

    private async void LoadActivities()
    {
        try
        {
            var result = await _activityService.GetUserActivitiesAsync(_currentPage, 20);

            if (result != null)
            {
                var (activities, totalCount) = result.Value;
                _currentActivities = activities;
                _totalPages = (int)Math.Ceiling(totalCount / 20.0);

                DisplayActivities();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger l'historique: {ex.Message}", "OK");
        }
    }

    private void FilterByPeriod(string period)
    {
        var filtered = _activityService.FilterByPeriod(_currentActivities, period);
        DisplayActivities(filtered);
    }

    private void DisplayActivities(List<UserActivity>? activities = null)
    {
        _historyStack.Children.Clear();

        var displayList = activities ?? _currentActivities;

        if (!displayList.Any())
        {
            _historyStack.Children.Add(new Label
            {
                Text = "Aucune activité trouvée",
                FontSize = 16,
                TextColor = Color.FromHex("#6C757D"),
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 50)
            });
            return;
        }

        // Résumé
        _historyStack.Children.Add(new Frame
        {
            BackgroundColor = Color.FromHex("#4A90E2"),
            CornerRadius = 12,
            Padding = 16,
            Margin = new Thickness(0, 0, 0, 10),
            Content = new Label
            {
                Text = $"{displayList.Count} activités - Page {_currentPage}/{_totalPages}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            }
        });

        // Activités
        foreach (var activity in displayList)
        {
            _historyStack.Children.Add(CreateActivityCard(activity));
        }
    }

    private Frame CreateActivityCard(UserActivity activity)
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 12,
            Padding = 16,
            HasShadow = true,
            Margin = new Thickness(0, 4)
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        // Icône
        var icon = new Frame
        {
            BackgroundColor = UserActivityService.GetActivityTypeColor(activity.ActivityType),
            WidthRequest = 40,
            HeightRequest = 40,
            CornerRadius = 20,
            Padding = 0,
            Content = new Label
            {
                Text = UserActivityService.GetActivityTypeIcon(activity.ActivityType),
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        // Contenu
        var content = new VerticalStackLayout
        {
            Spacing = 4,
            Margin = new Thickness(12, 0, 0, 0),
            Children =
            {
                new Label
                {
                    Text = activity.Description,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold
                },
                new Label
                {
                    Text = activity.TargetResource ?? "N/A",
                    FontSize = 12,
                    TextColor = Color.FromHex("#6C757D")
                },
                new Label
                {
                    Text = activity.Status,
                    FontSize = 10,
                    TextColor = UserActivityService.GetStatusColor(activity.Status),
                    FontAttributes = FontAttributes.Bold
                }
            }
        };

        // Heure
        var time = new Label
        {
            Text = UserActivityService.GetRelativeTime(activity.Timestamp),
            FontSize = 12,
            TextColor = Color.FromHex("#ADB5BD"),
            VerticalOptions = LayoutOptions.Center
        };

        Grid.SetColumn(icon, 0);
        Grid.SetColumn(content, 1);
        Grid.SetColumn(time, 2);

        grid.Children.Add(icon);
        grid.Children.Add(content);
        grid.Children.Add(time);

        frame.Content = grid;

        return frame;
    }

    private async void OnPreviousPageClicked(object sender, EventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            await LoadActivitiesAsync();
        }
    }

    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            await LoadActivitiesAsync();
        }
    }

    private async Task LoadActivitiesAsync()
    {
        // Même logique que LoadActivities() mais async
        try
        {
            var result = await _activityService.GetUserActivitiesAsync(_currentPage, 20);

            if (result != null)
            {
                var (activities, totalCount) = result.Value;
                _currentActivities = activities;
                _totalPages = (int)Math.Ceiling(totalCount / 20.0);

                DisplayActivities();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger l'historique: {ex.Message}", "OK");
        }
    }
}
```

---

## ? **CHECKLIST DE VALIDATION**

```
? Configuration
[ ] Backend démarré (dotnet run --launch-profile http)
[ ] Token JWT valide (via login)

? Code MAUI Corrigé
[ ] UserActivity.cs avec string ActivityType
[ ] ActivityStats.cs avec structure correcte
[ ] UserActivityService.cs avec bonnes routes
[ ] ActivityResponse et PaginationInfo créés

? Tests API
[ ] GET /api/user/activity/history fonctionne
[ ] GET /api/user/activity/stats fonctionne
[ ] GET /api/user/activity/recent fonctionne
[ ] Pagination fonctionne

? Interface
[ ] Liste paginée affichée
[ ] Filtrage local fonctionne
[ ] Navigation entre pages
[ ] Affichage correct des types
```

---

## ?? **POINTS CRITIQUES**

### **1. User ID Automatique**

? **N'envoyez PAS** le User ID en paramètre  
? **Le backend récupère** automatiquement l'User ID depuis le token JWT

### **2. Types d'Activité**

Les seuls types valides sont :
- `"Commande"`
- `"Consultation"`
- `"Modification"`
- `"Audit"`

### **3. Statut**

Les seuls statuts valides sont :
- `"Succès"`
- `"Échec"`

### **4. Pagination Obligatoire**

L'endpoint principal retourne **toujours** une réponse paginée. Vous devez gérer :
- `currentPage`
- `pageSize`
- `totalCount`
- `totalPages`
- `hasPrevious`
- `hasNext`

---

## ?? **RÉSUMÉ DES CHANGEMENTS**

| Composant | Action Requise |
|-----------|----------------|
| `UserActivity.cs` | Remplacer enums par strings |
| `ActivityStats.cs` | Utiliser structure du backend |
| `UserActivityService.cs` | Changer toutes les routes |
| `ActivityFilter.cs` | **Supprimer** (inutile) |
| `ActivityPeriodFilter.cs` | **Supprimer** (inutile) |
| `HistoryPage` | Ajouter gestion pagination |

---

**? Utilisez CE document au lieu du guide MAUI original !**

**Date de correction:** 2025-01-15  
**Basé sur:** Backend SmartOfficeAPP réel  
**Version API:** /api/user/activity/*
