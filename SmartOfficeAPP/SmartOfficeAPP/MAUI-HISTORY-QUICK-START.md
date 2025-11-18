# ? Démarrage Rapide - Historique des Activités MAUI

## ?? En 5 Minutes

### **1. Modèles (2 min)**

#### **UserActivity.cs**
```csharp
namespace SmartOfficeMobile.Models;

public enum ActionType { View = 0, Create = 1, Update = 2, Delete = 3, Execute = 4, Login = 5, Logout = 6 }
public enum ResourceType { Device = 0, Zone = 1, User = 2, Ticket = 3, Command = 4, System = 5, Security = 6, Maintenance = 7 }
public enum ActivityCategory { All = 0, User = 1, Device = 2, Security = 3, System = 4, Ticket = 5, Maintenance = 6 }

public class UserActivity
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public ResourceType ResourceType { get; set; }
    public string? ResourceName { get; set; }
    public bool IsSuccessful { get; set; } = true;
    
    public ActivityCategory Category => ResourceType switch
    {
        ResourceType.User => ActivityCategory.User,
        ResourceType.Device => ActivityCategory.Device,
        ResourceType.Security => ActivityCategory.Security,
        ResourceType.System => ActivityCategory.System,
        ResourceType.Ticket => ActivityCategory.Ticket,
        ResourceType.Maintenance => ActivityCategory.Maintenance,
        _ => ActivityCategory.All
    };
}
```

---

### **2. Service (2 min)**

#### **UserActivityService.cs**
```csharp
namespace SmartOfficeMobile.Services;

public class UserActivityService
{
    private readonly ApiService _api;

    public UserActivityService(ApiService api) => _api = api;

    // Lister mes activités
    public async Task<List<UserActivity>?> GetUserActivitiesAsync(string userId) =>
        await _api.GetAsync<List<UserActivity>>($"/api/UserActivity/user/{Uri.EscapeDataString(userId)}");

    // Activités récentes
    public async Task<List<UserActivity>?> GetRecentActivitiesAsync(int count = 10) =>
        await _api.GetAsync<List<UserActivity>>($"/api/UserActivity/recent?count={count}");

    // Filtrer par période
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

    // Filtrer par catégorie
    public List<UserActivity> FilterByCategory(List<UserActivity> activities, ActivityCategory category)
    {
        if (category == ActivityCategory.All) return activities;
        return activities.Where(a => a.Category == category).ToList();
    }

    // Helpers
    public static string GetCategoryDisplayName(ActivityCategory category) => category switch
    {
        ActivityCategory.User => "UTILISATEUR",
        ActivityCategory.Device => "APPAREIL",
        ActivityCategory.Security => "SÉCURITÉ",
        ActivityCategory.System => "SYSTÈME",
        ActivityCategory.Ticket => "TICKET",
        ActivityCategory.Maintenance => "MAINTENANCE",
        _ => "GÉNÉRAL"
    };

    public static Color GetCategoryColor(ActivityCategory category) => category switch
    {
        ActivityCategory.User => Color.FromArgb("#4A90E2"),
        ActivityCategory.Device => Color.FromArgb("#4ECDC4"),
        ActivityCategory.Security => Color.FromArgb("#FF6B6B"),
        ActivityCategory.System => Color.FromArgb("#6C757D"),
        ActivityCategory.Ticket => Color.FromArgb("#9575CD"),
        ActivityCategory.Maintenance => Color.FromArgb("#FFB74D"),
        _ => Color.FromArgb("#ADB5BD")
    };

    public static string GetRelativeTime(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;
        
        if (timeSpan.TotalMinutes < 1) return "À l'instant";
        if (timeSpan.TotalMinutes < 60) return $"Il y a {(int)timeSpan.TotalMinutes} min";
        if (timeSpan.TotalHours < 24) return $"Il y a {(int)timeSpan.TotalHours}h";
        if (timeSpan.TotalDays < 7) return $"Il y a {(int)timeSpan.TotalDays} jours";
        
        return dateTime.ToString("dd/MM/yyyy");
    }
}
```

---

### **3. Utilisation (1 min)**

#### **Lister les activités**
```csharp
var activityService = new UserActivityService(apiService);

var activities = await activityService.GetUserActivitiesAsync("user@smartoffice.com");

foreach (var activity in activities)
{
    Console.WriteLine($"{activity.ActionDescription} - {activity.Timestamp:HH:mm}");
}
```

#### **Filtrer par période**
```csharp
// Activités aujourd'hui seulement
var todayActivities = activityService.FilterByPeriod(activities, "today");

// Activités de la semaine
var weekActivities = activityService.FilterByPeriod(activities, "week");
```

#### **Filtrer par catégorie**
```csharp
// Activités utilisateur uniquement
var userActivities = activityService.FilterByCategory(activities, ActivityCategory.User);

// Activités de sécurité
var securityActivities = activityService.FilterByCategory(activities, ActivityCategory.Security);
```

---

## ?? **Interface Utilisateur Minimale**

### **Page d'Historique (Code-Behind)**

```csharp
public class HistoryPage : ContentPage
{
    private readonly UserActivityService _activityService;
    private StackLayout _activityStack;
    private List<UserActivity> _allActivities = new();

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
        var mainStack = new VerticalStackLayout { Padding = 20, Spacing = 16 };

        // Header
        mainStack.Children.Add(new Label
        {
            Text = "Historique des activités",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold
        });

        // Filtres
        var filterGrid = new Grid { ColumnDefinitions = { new(), new() }, ColumnSpacing = 12 };

        var todayBtn = new Button
        {
            Text = "Aujourd'hui",
            BackgroundColor = Color.FromHex("#4A90E2"),
            TextColor = Colors.White
        };
        todayBtn.Clicked += (s, e) => FilterByPeriod("today");

        var weekBtn = new Button
        {
            Text = "Cette semaine",
            BackgroundColor = Color.FromHex("#F0F4F8"),
            TextColor = Color.FromHex("#1A1A1A")
        };
        weekBtn.Clicked += (s, e) => FilterByPeriod("week");

        Grid.SetColumn(todayBtn, 0);
        Grid.SetColumn(weekBtn, 1);
        filterGrid.Children.Add(todayBtn);
        filterGrid.Children.Add(weekBtn);
        mainStack.Children.Add(filterGrid);

        // Liste
        _activityStack = new StackLayout { Spacing = 8 };
        mainStack.Children.Add(_activityStack);

        scrollView.Content = mainStack;
        Content = scrollView;
    }

    private async void LoadActivities()
    {
        try
        {
            _allActivities = await _activityService.GetUserActivitiesAsync("user@smartoffice.com") ?? new();
            DisplayActivities("today");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private void FilterByPeriod(string period)
    {
        DisplayActivities(period);
    }

    private void DisplayActivities(string period)
    {
        _activityStack.Children.Clear();

        var filtered = _activityService.FilterByPeriod(_allActivities, period);

        if (!filtered.Any())
        {
            _activityStack.Children.Add(new Label
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
        var summary = new Frame
        {
            BackgroundColor = Color.FromHex("#4A90E2"),
            CornerRadius = 12,
            Padding = 16,
            Margin = new Thickness(0, 0, 0, 10),
            Content = new Label
            {
                Text = $"{filtered.Count} activités {GetPeriodText(period)}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            }
        };
        _activityStack.Children.Add(summary);

        // Activités
        foreach (var activity in filtered.OrderByDescending(a => a.Timestamp))
        {
            _activityStack.Children.Add(CreateActivityCard(activity));
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
            ColumnDefinitions = { new() { Width = GridLength.Auto }, new() { Width = new GridLength(1, GridUnitType.Star) }, new() { Width = GridLength.Auto } }
        };

        // Icône
        var icon = new Frame
        {
            BackgroundColor = UserActivityService.GetCategoryColor(activity.Category),
            WidthRequest = 40,
            HeightRequest = 40,
            CornerRadius = 20,
            Padding = 0,
            Content = new Label
            {
                Text = GetCategoryIcon(activity.Category),
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
                new Label { Text = activity.ActionDescription, FontSize = 16, FontAttributes = FontAttributes.Bold },
                new Label { Text = activity.ResourceName ?? "N/A", FontSize = 12, TextColor = Color.FromHex("#6C757D") }
            }
        };

        // Heure
        var time = new Label
        {
            Text = UserActivityService.GetRelativeTime(activity.Timestamp),
            FontSize = 12,
            TextColor = Color.FromHex("#ADB5BD")
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

    private string GetPeriodText(string period) => period switch
    {
        "today" => "aujourd'hui",
        "week" => "cette semaine",
        "month" => "ce mois",
        _ => "au total"
    };

    private string GetCategoryIcon(ActivityCategory category) => category switch
    {
        ActivityCategory.User => "??",
        ActivityCategory.Device => "??",
        ActivityCategory.Security => "??",
        ActivityCategory.System => "??",
        ActivityCategory.Ticket => "??",
        ActivityCategory.Maintenance => "??",
        _ => "??"
    };
}
```

---

## ?? **Endpoints API**

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| **GET** | `/api/UserActivity/user/{userId}` | Mes activités |
| **GET** | `/api/UserActivity/{id}` | Détails d'une activité |
| **GET** | `/api/UserActivity/recent?count=10` | Activités récentes |
| **GET** | `/api/UserActivity/action/{actionType}` | Filtrer par type d'action (0-6) |
| **GET** | `/api/UserActivity/resource/{resourceType}` | Filtrer par type de ressource (0-9) |
| **GET** | `/api/UserActivity/stats` | Statistiques |

---

## ?? **Valeurs des Enums**

### **ActionType**
- `0` = View (Consultation)
- `1` = Create (Création)
- `2` = Update (Modification)
- `3` = Delete (Suppression)
- `4` = Execute (Exécution)
- `5` = Login (Connexion)
- `6` = Logout (Déconnexion)

### **ResourceType**
- `0` = Device (Appareil)
- `1` = Zone (Zone)
- `2` = User (Utilisateur)
- `3` = Ticket (Ticket)
- `4` = Command (Commande)
- `5` = System (Système)
- `6` = Security (Sécurité)
- `7` = Maintenance (Maintenance)
- `8` = Report (Rapport)
- `9` = Settings (Paramètres)

### **ActivityCategory**
- `0` = All (Tout)
- `1` = User (Utilisateur) ??
- `2` = Device (Appareil) ??
- `3` = Security (Sécurité) ??
- `4` = System (Système) ?
- `5` = Ticket (Ticket) ??
- `6` = Maintenance (Maintenance) ??

---

## ? Checklist Rapide

- [ ] Modèles créés (`UserActivity`, Enums)
- [ ] `UserActivityService` créé
- [ ] HistoryPage créée
- [ ] Test liste activités OK
- [ ] Test filtre today OK
- [ ] Test filtre week OK
- [ ] Test détails activité OK

---

## ?? Dépannage Express

| Problème | Solution |
|----------|----------|
| "Activities list empty" | Vérifier User ID, créer des activités de test |
| "Cannot load activities" | Token expiré, backend démarré, IP correcte |
| "Filter not working" | Vérifier timestamps, logique de filtrage |

---

## ?? Documentation Complète

Pour plus de détails :
- [MAUI-HISTORY-COMPLETE-GUIDE.md](MAUI-HISTORY-COMPLETE-GUIDE.md) ? Guide complet avec MVVM, recherche, export
- [Authorization-Activity-Complete-Guide.md](Authorization-Activity-Complete-Guide.md) ? Documentation backend
- [Tests/13-User-Activity-Authorization.http](Tests/13-User-Activity-Authorization.http) ? Tests HTTP

---

**?? C'est tout ! Vous pouvez maintenant afficher l'historique depuis MAUI !**

**Temps total:** 5 minutes ??
