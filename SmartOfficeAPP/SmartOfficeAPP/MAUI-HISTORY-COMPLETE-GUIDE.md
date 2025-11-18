# ?? Guide Complet - Historique des Activités depuis MAUI

## ?? **OBJECTIF**

Permettre à votre application mobile MAUI de :
- ? Afficher l'historique des activités utilisateur
- ? Filtrer par période (Aujourd'hui, Cette semaine, Mois, Tout)
- ? Filtrer par catégorie (Utilisateur, Appareil, Sécurité, Système, etc.)
- ? Voir les détails d'une activité
- ? Rechercher dans l'historique
- ? Exporter l'historique (CSV, PDF)

---

## ?? **TABLE DES MATIÈRES**

1. [Architecture](#architecture)
2. [Configuration Initiale](#configuration)
3. [Modèles de Données](#modeles)
4. [Service Historique](#service-historique)
5. [Interface Utilisateur](#interface-utilisateur)
6. [Exemples de Code Complets](#exemples-code)
7. [Tests et Validation](#tests-validation)
8. [Dépannage](#depannage)

---

## ??? **1. ARCHITECTURE** {#architecture}

```
???????????????????????????????????
?   APP MOBILE MAUI               ?
?                                 ?
?  ????????????????????????????   ?
?  ?  Pages                   ?   ?
?  ?  - HistoryPage           ?   ?
?  ?  - ActivityDetailsPage   ?   ?
?  ????????????????????????????   ?
?             ?                   ?
?  ????????????????????????????   ?
?  ?  Services                ?   ?
?  ?  - UserActivityService   ?   ?
?  ?  - ApiService            ?   ?
?  ????????????????????????????   ?
?             ?                   ?
?  ????????????????????????????   ?
?  ?  Models                  ?   ?
?  ?  - UserActivity          ?   ?
?  ?  - ActivityCategory      ?   ?
?  ?  - ActivityFilter        ?   ?
?  ????????????????????????????   ?
???????????????????????????????????
             ?
             ? HTTP REST
             ?
???????????????????????????????????
?   BACKEND API (.NET 9)          ?
?   /api/UserActivity             ?
???????????????????????????????????
```

---

## ?? **2. CONFIGURATION INITIALE** {#configuration}

### **2.1 Packages NuGet Requis**

```xml
<ItemGroup>
  <PackageReference Include="System.Net.Http.Json" Version="9.0.0" />
  <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.0" />
  <PackageReference Include="System.Text.Json" Version="9.0.0" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
</ItemGroup>
```

### **2.2 Configuration AppConfig**

Si vous n'avez pas encore `AppConfig.cs`, créez-le :

```csharp
namespace SmartOfficeMobile.Configuration;

public static class AppConfig
{
    // ? CHANGEZ CETTE IP
    private const string BaseUrl = "http://192.168.1.100:5290";
    
    public static string ApiBaseUrl => BaseUrl;
    public static string SignalRHubUrl => $"{BaseUrl}/hubs/notifications";
}
```

---

## ?? **3. MODÈLES DE DONNÉES** {#modeles}

### **3.1 Enums**

Créez `Models/Enums/ActivityEnums.cs` :

```csharp
namespace SmartOfficeMobile.Models;

/// <summary>
/// Type d'action effectuée
/// </summary>
public enum ActionType
{
    View = 0,       // Consultation
    Create = 1,     // Création
    Update = 2,     // Modification
    Delete = 3,     // Suppression
    Execute = 4,    // Exécution
    Login = 5,      // Connexion
    Logout = 6      // Déconnexion
}

/// <summary>
/// Type de ressource affectée
/// </summary>
public enum ResourceType
{
    Device = 0,          // Appareil/Périphérique
    Zone = 1,            // Zone
    User = 2,            // Utilisateur
    Ticket = 3,          // Ticket/Incident
    Command = 4,         // Commande
    System = 5,          // Système
    Security = 6,        // Sécurité
    Maintenance = 7,     // Maintenance
    Report = 8,          // Rapport
    Settings = 9         // Paramètres
}

/// <summary>
/// Catégorie d'activité (pour filtrage)
/// </summary>
public enum ActivityCategory
{
    All = 0,
    User = 1,
    Device = 2,
    Security = 3,
    System = 4,
    Ticket = 5,
    Maintenance = 6
}
```

### **3.2 Modèle UserActivity**

Créez `Models/UserActivity.cs` :

```csharp
namespace SmartOfficeMobile.Models;

public class UserActivity
{
    // =============================
    // IDENTITÉ
    // =============================
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    // =============================
    // ACTION
    // =============================
    public ActionType ActionType { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    // =============================
    // RESSOURCE AFFECTÉE
    // =============================
    public ResourceType ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? ResourceName { get; set; }

    // =============================
    // CONTEXTE
    // =============================
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }

    // =============================
    // MÉTADONNÉES
    // =============================
    public Dictionary<string, string>? Metadata { get; set; }
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }

    // =============================
    // HELPERS (Computed Properties)
    // =============================
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

    public string DisplayIcon => Category switch
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

### **3.3 Filtres**

Créez `Models/ActivityFilter.cs` :

```csharp
namespace SmartOfficeMobile.Models;

public class ActivityFilter
{
    public string? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ActionType? ActionType { get; set; }
    public ResourceType? ResourceType { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class ActivityPeriodFilter
{
    public static ActivityFilter Today()
    {
        return new ActivityFilter
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1)
        };
    }

    public static ActivityFilter ThisWeek()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        return new ActivityFilter
        {
            StartDate = startOfWeek,
            EndDate = startOfWeek.AddDays(7)
        };
    }

    public static ActivityFilter ThisMonth()
    {
        var today = DateTime.Today;
        return new ActivityFilter
        {
            StartDate = new DateTime(today.Year, today.Month, 1),
            EndDate = new DateTime(today.Year, today.Month, 1).AddMonths(1)
        };
    }

    public static ActivityFilter Last7Days()
    {
        return new ActivityFilter
        {
            StartDate = DateTime.Today.AddDays(-7),
            EndDate = DateTime.Today.AddDays(1)
        };
    }

    public static ActivityFilter Last30Days()
    {
        return new ActivityFilter
        {
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today.AddDays(1)
        };
    }
}
```

### **3.4 Statistiques**

Créez `Models/ActivityStats.cs` :

```csharp
namespace SmartOfficeMobile.Models;

public class ActivityStats
{
    public int TotalActivities { get; set; }
    public int TodayActivities { get; set; }
    public int ThisWeekActivities { get; set; }
    
    public Dictionary<ActionType, int> ByActionType { get; set; } = new();
    public Dictionary<ResourceType, int> ByResourceType { get; set; } = new();
    public Dictionary<ActivityCategory, int> ByCategory { get; set; } = new();
    
    public DateTime? LastActivityTime { get; set; }
    public string? MostActiveUser { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

---

## ?? **4. SERVICE HISTORIQUE** {#service-historique}

### **4.1 Service Complet**

Créez `Services/UserActivityService.cs` :

```csharp
namespace SmartOfficeMobile.Services;

public class UserActivityService
{
    private readonly ApiService _apiService;

    public UserActivityService(ApiService apiService)
    {
        _apiService = apiService;
    }

    // =============================
    // RÉCUPÉRATION
    // =============================

    /// <summary>
    /// Récupérer toutes les activités de l'utilisateur
    /// GET /api/UserActivity/user/{userId}
    /// </summary>
    public async Task<List<UserActivity>?> GetUserActivitiesAsync(string userId, ActivityFilter? filter = null)
    {
        try
        {
            var endpoint = $"/api/UserActivity/user/{Uri.EscapeDataString(userId)}";

            if (filter != null)
            {
                var queryParams = BuildQueryString(filter);
                if (!string.IsNullOrEmpty(queryParams))
                {
                    endpoint += $"?{queryParams}";
                }
            }

            return await _apiService.GetAsync<List<UserActivity>>(endpoint);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération activités: {ex.Message}");
            return new List<UserActivity>();
        }
    }

    /// <summary>
    /// Récupérer les activités récentes
    /// GET /api/UserActivity/recent
    /// </summary>
    public async Task<List<UserActivity>?> GetRecentActivitiesAsync(int count = 10)
    {
        try
        {
            return await _apiService.GetAsync<List<UserActivity>>(
                $"/api/UserActivity/recent?count={count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération activités récentes: {ex.Message}");
            return new List<UserActivity>();
        }
    }

    /// <summary>
    /// Récupérer une activité par ID
    /// GET /api/UserActivity/{id}
    /// </summary>
    public async Task<UserActivity?> GetActivityByIdAsync(int activityId)
    {
        try
        {
            return await _apiService.GetAsync<UserActivity>(
                $"/api/UserActivity/{activityId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération activité {activityId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Récupérer les activités par type d'action
    /// GET /api/UserActivity/action/{actionType}
    /// </summary>
    public async Task<List<UserActivity>?> GetActivitiesByActionTypeAsync(ActionType actionType)
    {
        try
        {
            var actionValue = (int)actionType;
            return await _apiService.GetAsync<List<UserActivity>>(
                $"/api/UserActivity/action/{actionValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération par type d'action: {ex.Message}");
            return new List<UserActivity>();
        }
    }

    /// <summary>
    /// Récupérer les activités par type de ressource
    /// GET /api/UserActivity/resource/{resourceType}
    /// </summary>
    public async Task<List<UserActivity>?> GetActivitiesByResourceTypeAsync(ResourceType resourceType)
    {
        try
        {
            var resourceValue = (int)resourceType;
            return await _apiService.GetAsync<List<UserActivity>>(
                $"/api/UserActivity/resource/{resourceValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération par type de ressource: {ex.Message}");
            return new List<UserActivity>();
        }
    }

    /// <summary>
    /// Récupérer les statistiques d'activité
    /// GET /api/UserActivity/stats
    /// </summary>
    public async Task<ActivityStats?> GetActivityStatsAsync()
    {
        try
        {
            return await _apiService.GetAsync<ActivityStats>(
                "/api/UserActivity/stats");
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
    /// Filtrer les activités par période
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
    /// Filtrer les activités par catégorie
    /// </summary>
    public List<UserActivity> FilterByCategory(List<UserActivity> activities, ActivityCategory category)
    {
        if (category == ActivityCategory.All)
            return activities;

        return activities.Where(a => a.Category == category).ToList();
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
            a.ActionDescription.ToLower().Contains(searchTerm) ||
            (a.ResourceName?.ToLower().Contains(searchTerm) ?? false) ||
            a.UserName.ToLower().Contains(searchTerm)
        ).ToList();
    }

    // =============================
    // HELPERS
    // =============================

    /// <summary>
    /// Construire la query string depuis un filtre
    /// </summary>
    private string BuildQueryString(ActivityFilter filter)
    {
        var queryParams = new List<string>();

        if (filter.StartDate.HasValue)
            queryParams.Add($"startDate={filter.StartDate.Value:yyyy-MM-dd}");

        if (filter.EndDate.HasValue)
            queryParams.Add($"endDate={filter.EndDate.Value:yyyy-MM-dd}");

        if (filter.ActionType.HasValue)
            queryParams.Add($"actionType={( int)filter.ActionType.Value}");

        if (filter.ResourceType.HasValue)
            queryParams.Add($"resourceType={(int)filter.ResourceType.Value}");

        if (!string.IsNullOrEmpty(filter.SearchTerm))
            queryParams.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");

        queryParams.Add($"pageNumber={filter.PageNumber}");
        queryParams.Add($"pageSize={filter.PageSize}");

        return string.Join("&", queryParams);
    }

    /// <summary>
    /// Obtenir le libellé d'un type d'action
    /// </summary>
    public static string GetActionTypeDisplayName(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.View => "Consultation",
            ActionType.Create => "Création",
            ActionType.Update => "Modification",
            ActionType.Delete => "Suppression",
            ActionType.Execute => "Exécution",
            ActionType.Login => "Connexion",
            ActionType.Logout => "Déconnexion",
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Obtenir le libellé d'un type de ressource
    /// </summary>
    public static string GetResourceTypeDisplayName(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Device => "Appareil",
            ResourceType.Zone => "Zone",
            ResourceType.User => "Utilisateur",
            ResourceType.Ticket => "Ticket",
            ResourceType.Command => "Commande",
            ResourceType.System => "Système",
            ResourceType.Security => "Sécurité",
            ResourceType.Maintenance => "Maintenance",
            ResourceType.Report => "Rapport",
            ResourceType.Settings => "Paramètres",
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Obtenir la couleur d'une catégorie
    /// </summary>
    public static Color GetCategoryColor(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.User => Color.FromArgb("#4A90E2"),        // Bleu
            ActivityCategory.Device => Color.FromArgb("#4ECDC4"),      // Turquoise
            ActivityCategory.Security => Color.FromArgb("#FF6B6B"),    // Rouge
            ActivityCategory.System => Color.FromArgb("#6C757D"),      // Gris
            ActivityCategory.Ticket => Color.FromArgb("#9575CD"),      // Violet
            ActivityCategory.Maintenance => Color.FromArgb("#FFB74D"), // Orange
            _ => Color.FromArgb("#ADB5BD")                             // Gris clair
        };
    }

    /// <summary>
    /// Obtenir le nom de la catégorie
    /// </summary>
    public static string GetCategoryDisplayName(ActivityCategory category)
    {
        return category switch
        {
            ActivityCategory.User => "UTILISATEUR",
            ActivityCategory.Device => "APPAREIL",
            ActivityCategory.Security => "SÉCURITÉ",
            ActivityCategory.System => "SYSTÈME",
            ActivityCategory.Ticket => "TICKET",
            ActivityCategory.Maintenance => "MAINTENANCE",
            _ => "GÉNÉRAL"
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
```

---

## ?? **5. INTERFACE UTILISATEUR** {#interface-utilisateur}

### **5.1 ViewModel pour l'Historique**

Créez `ViewModels/HistoryViewModel.cs` :

```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartOfficeMobile.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly UserActivityService _activityService;
    private readonly string _userId;
    private List<UserActivity> _allActivities = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _summaryText = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ObservableCollection<UserActivity> Activities { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand<string> FilterByPeriodCommand { get; }
    public ICommand<ActivityCategory> FilterByCategoryCommand { get; }
    public ICommand<UserActivity> ViewDetailsCommand { get; }
    public ICommand SearchCommand { get; }

    public HistoryViewModel(UserActivityService activityService, string userId)
    {
        _activityService = activityService;
        _userId = userId;

        RefreshCommand = new AsyncRelayCommand(LoadActivitiesAsync);
        FilterByPeriodCommand = new AsyncRelayCommand<string>(FilterByPeriodAsync);
        FilterByCategoryCommand = new AsyncRelayCommand<ActivityCategory>(FilterByCategoryAsync);
        ViewDetailsCommand = new AsyncRelayCommand<UserActivity>(ViewActivityDetailsAsync);
        SearchCommand = new RelayCommand(PerformSearch);

        // Charger au démarrage
        _ = LoadActivitiesAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        // Recherche automatique après 500ms
        Task.Delay(500).ContinueWith(_ => PerformSearch());
    }

    private async Task LoadActivitiesAsync()
    {
        IsLoading = true;

        try
        {
            var activities = await _activityService.GetUserActivitiesAsync(_userId);

            if (activities != null)
            {
                _allActivities = activities;
                DisplayActivities(_allActivities);
                UpdateSummary("today");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur",
                $"Impossible de charger l'historique: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task FilterByPeriodAsync(string? period)
    {
        if (string.IsNullOrEmpty(period)) return;

        IsLoading = true;

        try
        {
            var filtered = _activityService.FilterByPeriod(_allActivities, period);
            DisplayActivities(filtered);
            UpdateSummary(period);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur",
                $"Impossible de filtrer: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task FilterByCategoryAsync(ActivityCategory category)
    {
        IsLoading = true;

        try
        {
            var filtered = _activityService.FilterByCategory(_allActivities, category);
            DisplayActivities(filtered);
            UpdateSummary("filtered");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur",
                $"Impossible de filtrer: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void PerformSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            DisplayActivities(_allActivities);
            return;
        }

        var filtered = _activityService.Search(_allActivities, SearchText);
        DisplayActivities(filtered);
    }

    private void DisplayActivities(List<UserActivity> activities)
    {
        Activities.Clear();

        foreach (var activity in activities.OrderByDescending(a => a.Timestamp))
        {
            Activities.Add(activity);
        }
    }

    private void UpdateSummary(string period)
    {
        var count = Activities.Count;
        var periodText = period switch
        {
            "today" => "aujourd'hui",
            "week" => "cette semaine",
            "month" => "ce mois",
            _ => "au total"
        };

        SummaryText = $"{count} activités {periodText}";
    }

    private async Task ViewActivityDetailsAsync(UserActivity? activity)
    {
        if (activity == null) return;

        var details = $"Action: {UserActivityService.GetActionTypeDisplayName(activity.ActionType)}\n" +
                     $"Ressource: {UserActivityService.GetResourceTypeDisplayName(activity.ResourceType)}\n" +
                     $"Description: {activity.ActionDescription}\n" +
                     $"Date: {activity.Timestamp:dd/MM/yyyy HH:mm:ss}\n" +
                     $"Utilisateur: {activity.UserName}\n";

        if (!string.IsNullOrEmpty(activity.ResourceName))
        {
            details += $"Nom ressource: {activity.ResourceName}\n";
        }

        if (!string.IsNullOrEmpty(activity.Location))
        {
            details += $"Localisation: {activity.Location}\n";
        }

        if (!string.IsNullOrEmpty(activity.IpAddress))
        {
            details += $"IP: {activity.IpAddress}\n";
        }

        if (!activity.IsSuccessful && !string.IsNullOrEmpty(activity.ErrorMessage))
        {
            details += $"\n?? Erreur: {activity.ErrorMessage}";
        }

        await Application.Current.MainPage.DisplayAlert(
            $"Activité #{activity.Id}",
            details,
            "Fermer");
    }
}
```

---

## ?? **6. EXEMPLES DE CODE COMPLETS** {#exemples-code}

### **6.1 Page Code-Behind Minimaliste**

```csharp
namespace SmartOfficeMobile.Pages;

public class HistoryPage : ContentPage
{
    private readonly UserActivityService _activityService;
    private StackLayout _historyStack;
    private List<UserActivity> _allActivities = new();
    private string _currentFilter = "today";

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
        var headerLabel = new Label
        {
            Text = "Historique des activités",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromHex("#1A1A1A")
        };

        // Filtres de période
        var filterGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(),
                new ColumnDefinition()
            },
            ColumnSpacing = 12
        };

        var todayButton = new Button
        {
            Text = "Aujourd'hui",
            BackgroundColor = Color.FromHex("#4A90E2"),
            TextColor = Colors.White,
            FontSize = 14,
            CornerRadius = 12
        };
        todayButton.Clicked += (s, e) => FilterByPeriod("today");

        var weekButton = new Button
        {
            Text = "Cette semaine",
            BackgroundColor = Color.FromHex("#F0F4F8"),
            TextColor = Color.FromHex("#1A1A1A"),
            FontSize = 14,
            CornerRadius = 12
        };
        weekButton.Clicked += (s, e) => FilterByPeriod("week");

        Grid.SetColumn(todayButton, 0);
        Grid.SetColumn(weekButton, 1);
        filterGrid.Children.Add(todayButton);
        filterGrid.Children.Add(weekButton);

        // Liste des activités
        _historyStack = new StackLayout { Spacing = 8 };

        mainStack.Children.Add(headerLabel);
        mainStack.Children.Add(filterGrid);
        mainStack.Children.Add(_historyStack);

        scrollView.Content = mainStack;
        Content = scrollView;
    }

    private async void LoadActivities()
    {
        try
        {
            _allActivities = await _activityService.GetUserActivitiesAsync("current-user-id") ?? new();
            DisplayActivities();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger l'historique: {ex.Message}", "OK");
        }
    }

    private void FilterByPeriod(string period)
    {
        _currentFilter = period;
        DisplayActivities();
    }

    private void DisplayActivities()
    {
        _historyStack.Children.Clear();

        var filtered = _activityService.FilterByPeriod(_allActivities, _currentFilter);

        if (!filtered.Any())
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
        var summaryFrame = new Frame
        {
            BackgroundColor = Color.FromHex("#4A90E2"),
            CornerRadius = 12,
            Padding = 16,
            Margin = new Thickness(0, 0, 0, 10),
            HasShadow = true,
            Content = new Label
            {
                Text = $"{filtered.Count} activités {GetPeriodText(_currentFilter)}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White
            }
        };
        _historyStack.Children.Add(summaryFrame);

        // Activités
        foreach (var activity in filtered.OrderByDescending(a => a.Timestamp))
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
        var iconFrame = new Frame
        {
            BackgroundColor = UserActivityService.GetCategoryColor(activity.Category),
            WidthRequest = 44,
            HeightRequest = 44,
            CornerRadius = 22,
            HasShadow = false,
            Padding = 0,
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = activity.DisplayIcon,
                FontSize = 20,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        // Contenu
        var contentStack = new VerticalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(12, 0, 0, 0)
        };

        contentStack.Children.Add(new Label
        {
            Text = activity.ActionDescription,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromHex("#1A1A1A")
        });

        if (!string.IsNullOrEmpty(activity.ResourceName))
        {
            contentStack.Children.Add(new Label
            {
                Text = activity.ResourceName,
                FontSize = 12,
                TextColor = Color.FromHex("#6C757D")
            });
        }

        contentStack.Children.Add(new Label
        {
            Text = UserActivityService.GetCategoryDisplayName(activity.Category),
            FontSize = 10,
            TextColor = UserActivityService.GetCategoryColor(activity.Category),
            FontAttributes = FontAttributes.Bold
        });

        // Heure
        var timeLabel = new Label
        {
            Text = UserActivityService.GetRelativeTime(activity.Timestamp),
            FontSize = 12,
            TextColor = Color.FromHex("#ADB5BD"),
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        Grid.SetColumn(iconFrame, 0);
        Grid.SetColumn(contentStack, 1);
        Grid.SetColumn(timeLabel, 2);

        grid.Children.Add(iconFrame);
        grid.Children.Add(contentStack);
        grid.Children.Add(timeLabel);

        frame.Content = grid;

        // Interaction
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            await frame.ScaleTo(0.95, 100);
            await frame.ScaleTo(1.0, 100);

            var details = $"Action: {UserActivityService.GetActionTypeDisplayName(activity.ActionType)}\n" +
                         $"Ressource: {UserActivityService.GetResourceTypeDisplayName(activity.ResourceType)}\n" +
                         $"Description: {activity.ActionDescription}\n" +
                         $"Date: {activity.Timestamp:dd/MM/yyyy HH:mm:ss}";

            await DisplayAlert($"Activité #{activity.Id}", details, "Fermer");
        };
        frame.GestureRecognizers.Add(tapGesture);

        return frame;
    }

    private string GetPeriodText(string filter)
    {
        return filter switch
        {
            "today" => "aujourd'hui",
            "week" => "cette semaine",
            "month" => "ce mois",
            _ => "au total"
        };
    }
}
```

---

## ? **7. TESTS ET VALIDATION** {#tests-validation}

### **7.1 Checklist de Validation**

```
? Configuration
[ ] Backend démarré (dotnet run --launch-profile http)
[ ] Pare-feu configuré
[ ] Test API (/api/UserActivity/stats)

? Code MAUI
[ ] Modèles créés (UserActivity, Enums, Filters)
[ ] UserActivityService créé
[ ] HistoryViewModel créé (optionnel)
[ ] HistoryPage créée

? Fonctionnalités
[ ] Login fonctionne
[ ] Liste activités affichée
[ ] Filtrage par période fonctionne
[ ] Filtrage par catégorie fonctionne
[ ] Recherche fonctionne
[ ] Détails activité affichés
[ ] Statistiques affichées

? Tests
[ ] Test liste vide
[ ] Test filtrage today
[ ] Test filtrage week
[ ] Test recherche
[ ] Test gestion d'erreurs
```

---

## ?? **8. DÉPANNAGE** {#depannage}

### **Problème: "Activities list empty"**
**Solution:**
- Vérifier que l'utilisateur a des activités en base
- Tester avec `GET /api/UserActivity/user/{userId}` dans HTTP file
- Créer des activités de test via backend

### **Problème: "Cannot load activities"**
**Solution:**
- Vérifier IP dans `AppConfig`
- Backend démarré: `dotnet run --launch-profile http`
- Token JWT valide
- User ID correct

### **Problème: "Filter not working"**
**Solution:**
- Vérifier que les activités ont des timestamps valides
- Vérifier la logique de filtrage locale
- Tester avec différentes périodes

---

## ?? **RESSOURCES ADDITIONNELLES**

| Document | Description |
|----------|-------------|
| [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md) | Index complet mobile |
| [MAUI-TICKETS-COMPLETE-GUIDE.md](MAUI-TICKETS-COMPLETE-GUIDE.md) | Guide tickets |
| [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) | Guide périphériques |

---

## ?? **ENDPOINTS API DISPONIBLES**

### **Activités**
```
GET  /api/UserActivity/user/{userId}      - Activités de l'utilisateur
GET  /api/UserActivity/{id}               - Détails d'une activité
GET  /api/UserActivity/recent             - Activités récentes
GET  /api/UserActivity/action/{actionType} - Filtrer par type d'action
GET  /api/UserActivity/resource/{resourceType} - Filtrer par type de ressource
GET  /api/UserActivity/stats              - Statistiques
```

---

**? Vous êtes maintenant prêt à gérer l'historique depuis MAUI !** ??

**Temps total estimé:** 2-3 heures pour l'implémentation complète  
**Niveau:** Intermédiaire  
**Support:** Consultez les fichiers de documentation dans `SmartOfficeAPP/`
