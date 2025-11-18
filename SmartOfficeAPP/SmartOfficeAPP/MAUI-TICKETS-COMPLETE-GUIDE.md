# ?? Guide Complet - Gestion des Tickets & Incidents depuis MAUI

## ?? **OBJECTIF**

Permettre à votre application mobile MAUI de :
- ? Créer des tickets/incidents
- ? Lister les tickets de l'utilisateur
- ? Filtrer par statut (Ouvert, En cours, Résolu, Fermé)
- ? Voir les détails complets d'un ticket
- ? Consulter les statistiques
- ? Recevoir des notifications temps réel (SignalR)

---

## ?? **TABLE DES MATIÈRES**

1. [Architecture](#architecture)
2. [Configuration Initiale](#configuration)
3. [Modèles de Données](#modeles)
4. [Service Tickets](#service-tickets)
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
?  ?  - TicketsPage           ?   ?
?  ?  - CreateTicketPage      ?   ?
?  ?  - TicketDetailsPage     ?   ?
?  ????????????????????????????   ?
?             ?                   ?
?  ????????????????????????????   ?
?  ?  Services                ?   ?
?  ?  - TicketService         ?   ?
?  ?  - ApiService            ?   ?
?  ?  - SignalRService        ?   ?
?  ????????????????????????????   ?
?             ?                   ?
?  ????????????????????????????   ?
?  ?  Models                  ?   ?
?  ?  - Ticket                ?   ?
?  ?  - TicketCreateRequest   ?   ?
?  ?  - TicketStats           ?   ?
?  ????????????????????????????   ?
???????????????????????????????????
             ?
             ? HTTP REST
             ?
???????????????????????????????????
?   BACKEND API (.NET 9)          ?
?   /api/IncidentTickets          ?
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

Créez `Models/Enums/TicketEnums.cs` :

```csharp
namespace SmartOfficeMobile.Models;

/// <summary>
/// Statut du ticket
/// </summary>
public enum TicketStatus
{
    Open = 0,        // Ouvert
    InProgress = 1,  // En cours
    Resolved = 2,    // Résolu
    Closed = 3       // Fermé
}

/// <summary>
/// Type de ticket
/// </summary>
public enum TicketType
{
    TechnicalIssue = 0,      // Problème technique
    MaintenanceRequest = 1,  // Demande de maintenance
    NewFeatureRequest = 2,   // Nouvelle fonctionnalité
    AccessIssue = 3,         // Problème d'accès
    Other = 4                // Autre
}

/// <summary>
/// Priorité du ticket
/// </summary>
public enum TicketPriority
{
    Low = 0,      // Basse
    Medium = 1,   // Moyenne
    High = 2,     // Haute
    Critical = 3  // Critique
}
```

### **3.2 Modèle Ticket**

Créez `Models/Ticket.cs` :

```csharp
namespace SmartOfficeMobile.Models;

public class Ticket
{
    // =============================
    // IDENTITÉ
    // =============================
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // =============================
    // CLASSIFICATION
    // =============================
    public TicketType Type { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }

    // =============================
    // LOCALISATION
    // =============================
    public string DeviceLocation { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? PeripheralName { get; set; }

    // =============================
    // RÉSOLUTION
    // =============================
    public string? Resolution { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? ResolvedBy { get; set; }

    // =============================
    // MÉTADONNÉES
    // =============================
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? AssignedTo { get; set; }
    public List<string> Tags { get; set; } = new();
}
```

### **3.3 DTO de Création**

Créez `Models/TicketCreateRequest.cs` :

```csharp
namespace SmartOfficeMobile.Models;

public class TicketCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Type { get; set; }  // TicketType enum (0-4)
    public int Priority { get; set; }  // TicketPriority enum (0-3)
    public string DeviceLocation { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? PeripheralName { get; set; }
    public List<string> Tags { get; set; } = new();
}
```

### **3.4 Statistiques**

Créez `Models/TicketStats.cs` :

```csharp
namespace SmartOfficeMobile.Models;

public class TicketStats
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    
    public Dictionary<TicketType, int> ByType { get; set; } = new();
    public Dictionary<TicketPriority, int> ByPriority { get; set; } = new();
    
    public double AverageResolutionTimeHours { get; set; }
    public DateTime GeneratedAt { get; set; }
}
```

---

## ?? **4. SERVICE TICKETS** {#service-tickets}

### **4.1 Service Complet**

Créez `Services/TicketService.cs` :

```csharp
namespace SmartOfficeMobile.Services;

public class TicketService
{
    private readonly ApiService _apiService;

    public TicketService(ApiService apiService)
    {
        _apiService = apiService;
    }

    // =============================
    // CRÉATION
    // =============================

    /// <summary>
    /// Créer un nouveau ticket
    /// POST /api/IncidentTickets
    /// </summary>
    public async Task<Ticket?> CreateTicketAsync(TicketCreateRequest request)
    {
        try
        {
            return await _apiService.PostAsync<TicketCreateRequest, Ticket>(
                "/api/IncidentTickets",
                request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur création ticket: {ex.Message}");
            throw;
        }
    }

    // =============================
    // RÉCUPÉRATION
    // =============================

    /// <summary>
    /// Récupérer tous les tickets de l'utilisateur
    /// GET /api/IncidentTickets/user/{email}
    /// </summary>
    public async Task<List<Ticket>?> GetUserTicketsAsync(string userEmail)
    {
        try
        {
            return await _apiService.GetAsync<List<Ticket>>(
                $"/api/IncidentTickets/user/{Uri.EscapeDataString(userEmail)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération tickets: {ex.Message}");
            return new List<Ticket>();
        }
    }

    /// <summary>
    /// Récupérer un ticket par ID
    /// GET /api/IncidentTickets/{id}
    /// </summary>
    public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
    {
        try
        {
            return await _apiService.GetAsync<Ticket>(
                $"/api/IncidentTickets/{ticketId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération ticket {ticketId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Récupérer les tickets par statut
    /// GET /api/IncidentTickets/status/{status}
    /// </summary>
    public async Task<List<Ticket>?> GetTicketsByStatusAsync(TicketStatus status)
    {
        try
        {
            var statusValue = (int)status;
            return await _apiService.GetAsync<List<Ticket>>(
                $"/api/IncidentTickets/status/{statusValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération tickets par statut: {ex.Message}");
            return new List<Ticket>();
        }
    }

    /// <summary>
    /// Récupérer les tickets par type
    /// GET /api/IncidentTickets/type/{type}
    /// </summary>
    public async Task<List<Ticket>?> GetTicketsByTypeAsync(TicketType type)
    {
        try
        {
            var typeValue = (int)type;
            return await _apiService.GetAsync<List<Ticket>>(
                $"/api/IncidentTickets/type/{typeValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération tickets par type: {ex.Message}");
            return new List<Ticket>();
        }
    }

    /// <summary>
    /// Récupérer les tickets par priorité
    /// GET /api/IncidentTickets/priority/{priority}
    /// </summary>
    public async Task<List<Ticket>?> GetTicketsByPriorityAsync(TicketPriority priority)
    {
        try
        {
            var priorityValue = (int)priority;
            return await _apiService.GetAsync<List<Ticket>>(
                $"/api/IncidentTickets/priority/{priorityValue}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération tickets par priorité: {ex.Message}");
            return new List<Ticket>();
        }
    }

    // =============================
    // STATISTIQUES
    // =============================

    /// <summary>
    /// Récupérer les statistiques des tickets
    /// GET /api/IncidentTickets/stats
    /// </summary>
    public async Task<TicketStats?> GetTicketStatsAsync()
    {
        try
        {
            return await _apiService.GetAsync<TicketStats>(
                "/api/IncidentTickets/stats");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur récupération stats: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Calculer les statistiques localement (fallback)
    /// </summary>
    public Dictionary<TicketStatus, int> CalculateLocalStats(List<Ticket> tickets)
    {
        return tickets
            .GroupBy(t => t.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // =============================
    // MISE À JOUR (ADMIN)
    // =============================

    /// <summary>
    /// Mettre à jour un ticket (nécessite droits admin)
    /// PUT /api/IncidentTickets/{id}
    /// </summary>
    public async Task<bool> UpdateTicketAsync(int ticketId, Ticket ticket)
    {
        try
        {
            var response = await _apiService.PostAsync(
                $"/api/IncidentTickets/{ticketId}",
                ticket);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur mise à jour ticket: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Assigner un ticket à un technicien (nécessite droits admin)
    /// PUT /api/IncidentTickets/{id}/assign
    /// </summary>
    public async Task<bool> AssignTicketAsync(int ticketId, string technicianEmail)
    {
        try
        {
            var response = await _apiService.PostAsync(
                $"/api/IncidentTickets/{ticketId}/assign",
                new { assignedTo = technicianEmail });
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur assignation ticket: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Résoudre un ticket (nécessite droits admin)
    /// PUT /api/IncidentTickets/{id}/resolve
    /// </summary>
    public async Task<bool> ResolveTicketAsync(int ticketId, string resolution)
    {
        try
        {
            var response = await _apiService.PostAsync(
                $"/api/IncidentTickets/{ticketId}/resolve",
                new { resolution });
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur résolution ticket: {ex.Message}");
            return false;
        }
    }

    // =============================
    // HELPERS
    // =============================

    /// <summary>
    /// Obtenir le libellé d'un statut
    /// </summary>
    public static string GetStatusDisplayName(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.Open => "Ouvert",
            TicketStatus.InProgress => "En cours",
            TicketStatus.Resolved => "Résolu",
            TicketStatus.Closed => "Fermé",
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Obtenir le libellé d'un type
    /// </summary>
    public static string GetTypeDisplayName(TicketType type)
    {
        return type switch
        {
            TicketType.TechnicalIssue => "Problème technique",
            TicketType.MaintenanceRequest => "Demande de maintenance",
            TicketType.NewFeatureRequest => "Nouvelle fonctionnalité",
            TicketType.AccessIssue => "Problème d'accès",
            TicketType.Other => "Autre",
            _ => "Inconnu"
        };
    }

    /// <summary>
    /// Obtenir le libellé d'une priorité
    /// </summary>
    public static string GetPriorityDisplayName(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Low => "Basse",
            TicketPriority.Medium => "Moyenne",
            TicketPriority.High => "Haute",
            TicketPriority.Critical => "Critique",
            _ => "Inconnue"
        };
    }

    /// <summary>
    /// Obtenir la couleur d'un statut
    /// </summary>
    public static Color GetStatusColor(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.Open => Color.FromArgb("#FF6B6B"),       // Rouge
            TicketStatus.InProgress => Color.FromArgb("#FFB74D"), // Orange
            TicketStatus.Resolved => Color.FromArgb("#4ECDC4"),   // Vert
            TicketStatus.Closed => Color.FromArgb("#ADB5BD"),     // Gris
            _ => Color.FromArgb("#6C757D")
        };
    }

    /// <summary>
    /// Obtenir la couleur d'une priorité
    /// </summary>
    public static Color GetPriorityColor(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Low => Color.FromArgb("#4ECDC4"),      // Vert
            TicketPriority.Medium => Color.FromArgb("#FFB74D"),   // Orange
            TicketPriority.High => Color.FromArgb("#FF6B6B"),     // Rouge
            TicketPriority.Critical => Color.FromArgb("#E53935"), // Rouge foncé
            _ => Color.FromArgb("#6C757D")
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

### **5.1 ViewModel pour la Liste**

Créez `ViewModels/TicketsViewModel.cs` :

```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartOfficeMobile.ViewModels;

public partial class TicketsViewModel : ObservableObject
{
    private readonly TicketService _ticketService;
    private readonly string _userEmail;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statsText = string.Empty;

    public ObservableCollection<Ticket> Tickets { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand<TicketStatus?> FilterCommand { get; }
    public ICommand<Ticket> ViewDetailsCommand { get; }
    public ICommand CreateTicketCommand { get; }

    public TicketsViewModel(TicketService ticketService, string userEmail)
    {
        _ticketService = ticketService;
        _userEmail = userEmail;

        RefreshCommand = new AsyncRelayCommand(LoadTicketsAsync);
        FilterCommand = new AsyncRelayCommand<TicketStatus?>(FilterTicketsAsync);
        ViewDetailsCommand = new AsyncRelayCommand<Ticket>(ViewTicketDetailsAsync);
        CreateTicketCommand = new AsyncRelayCommand(NavigateToCreateTicketAsync);

        // Charger au démarrage
        _ = LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        IsLoading = true;

        try
        {
            var tickets = await _ticketService.GetUserTicketsAsync(_userEmail);
            
            if (tickets != null)
            {
                Tickets.Clear();
                foreach (var ticket in tickets.OrderByDescending(t => t.CreatedDate))
                {
                    Tickets.Add(ticket);
                }

                // Mettre à jour les stats
                var stats = _ticketService.CalculateLocalStats(tickets);
                StatsText = $"{tickets.Count} tickets - " +
                           $"{stats.GetValueOrDefault(TicketStatus.Open, 0)} ouverts - " +
                           $"{stats.GetValueOrDefault(TicketStatus.Resolved, 0)} résolus";
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur",
                $"Impossible de charger les tickets: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task FilterTicketsAsync(TicketStatus? status)
    {
        IsLoading = true;

        try
        {
            List<Ticket>? tickets;

            if (status.HasValue)
            {
                tickets = await _ticketService.GetTicketsByStatusAsync(status.Value);
            }
            else
            {
                tickets = await _ticketService.GetUserTicketsAsync(_userEmail);
            }

            if (tickets != null)
            {
                Tickets.Clear();
                foreach (var ticket in tickets.OrderByDescending(t => t.CreatedDate))
                {
                    Tickets.Add(ticket);
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur",
                $"Impossible de filtrer les tickets: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewTicketDetailsAsync(Ticket? ticket)
    {
        if (ticket == null) return;

        var details = $"Ticket #{ticket.Id}\n\n" +
                     $"Type: {TicketService.GetTypeDisplayName(ticket.Type)}\n" +
                     $"Priorité: {TicketService.GetPriorityDisplayName(ticket.Priority)}\n" +
                     $"Localisation: {ticket.DeviceLocation}\n" +
                     $"Créé le: {ticket.CreatedDate:dd/MM/yyyy HH:mm}\n" +
                     $"Par: {ticket.CreatedBy}\n\n" +
                     $"Description:\n{ticket.Description}";

        if (ticket.Status == TicketStatus.Resolved && !string.IsNullOrEmpty(ticket.Resolution))
        {
            details += $"\n\nRésolution:\n{ticket.Resolution}\n" +
                      $"Résolu le: {ticket.ResolvedDate:dd/MM/yyyy HH:mm}";
        }

        await Application.Current.MainPage.DisplayAlert(
            $"Ticket #{ticket.Id}",
            details,
            "Fermer");
    }

    private async Task NavigateToCreateTicketAsync()
    {
        await Shell.Current.GoToAsync("createticket");
    }
}
```

### **5.2 ViewModel pour Création**

Créez `ViewModels/CreateTicketViewModel.cs` :

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartOfficeMobile.ViewModels;

public partial class CreateTicketViewModel : ObservableObject
{
    private readonly TicketService _ticketService;
    private readonly string _userEmail;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private int _selectedTypeIndex = 0;

    [ObservableProperty]
    private int _selectedPriorityIndex = 1; // Moyenne par défaut

    [ObservableProperty]
    private string _deviceLocation = string.Empty;

    [ObservableProperty]
    private bool _isSubmitting;

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }

    public List<string> TicketTypes { get; } = new()
    {
        "Problème technique",
        "Demande de maintenance",
        "Nouvelle fonctionnalité",
        "Problème d'accès",
        "Autre"
    };

    public List<string> PriorityLevels { get; } = new()
    {
        "Basse",
        "Moyenne",
        "Haute",
        "Critique"
    };

    public CreateTicketViewModel(TicketService ticketService, string userEmail)
    {
        _ticketService = ticketService;
        _userEmail = userEmail;

        SubmitCommand = new AsyncRelayCommand(SubmitTicketAsync);
        CancelCommand = new AsyncRelayCommand(CancelAsync);
    }

    private async Task SubmitTicketAsync()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Title))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Titre manquant",
                "Veuillez saisir un titre pour votre ticket",
                "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Description manquante",
                "Veuillez décrire le problème en détail",
                "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(DeviceLocation))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Localisation manquante",
                "Veuillez indiquer la localisation",
                "OK");
            return;
        }

        IsSubmitting = true;

        try
        {
            var request = new TicketCreateRequest
            {
                Title = Title.Trim(),
                Description = Description.Trim(),
                Type = SelectedTypeIndex,
                Priority = SelectedPriorityIndex,
                DeviceLocation = DeviceLocation.Trim()
            };

            var createdTicket = await _ticketService.CreateTicketAsync(request);

            if (createdTicket != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Succès",
                    $"Votre ticket #{createdTicket.Id} a été créé avec succès!\n\n" +
                    $"Type: {TicketService.GetTypeDisplayName(createdTicket.Type)}\n" +
                    $"Priorité: {TicketService.GetPriorityDisplayName(createdTicket.Priority)}",
                    "Parfait !");

                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Erreur",
                    "Impossible de créer le ticket. Veuillez réessayer.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur",
                $"Erreur inattendue: {ex.Message}",
                "OK");
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private async Task CancelAsync()
    {
        var result = await Application.Current.MainPage.DisplayAlert(
            "Confirmation",
            "Êtes-vous sûr de vouloir annuler ?\nToutes les informations saisies seront perdues.",
            "Oui, annuler",
            "Non, continuer");

        if (result)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
```

---

## ?? **6. EXEMPLES DE CODE COMPLETS** {#exemples-code}

### **6.1 Page XAML - Liste des Tickets**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:SmartOfficeMobile.ViewModels"
             x:Class="SmartOfficeMobile.Pages.TicketsPage"
             x:DataType="vm:TicketsViewModel"
             Title="Tickets & Incidents"
             BackgroundColor="#F5F5F7">

    <Grid Padding="20" RowSpacing="24">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Header -->
        <Grid Grid.Row="0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <Label Grid.Column="0"
                   Text="Mes Tickets"
                   FontSize="20"
                   FontAttributes="Bold"
                   TextColor="#1A1A1A"
                   VerticalOptions="Center"/>

            <Button Grid.Column="1"
                    Text="+ Nouveau"
                    Command="{Binding CreateTicketCommand}"
                    BackgroundColor="#4A90E2"
                    TextColor="White"
                    FontSize="14"
                    CornerRadius="12"
                    Padding="16,8"
                    FontAttributes="Bold"
                    WidthRequest="120"/>
        </Grid>

        <!-- Statistiques -->
        <Label Grid.Row="1"
               Text="{Binding StatsText}"
               FontSize="14"
               TextColor="#6C757D"/>

        <!-- Filtres -->
        <Grid Grid.Row="2" ColumnSpacing="8">
            <Grid.ColumnDefinitions>
                <ColumnDefinition/>
                <ColumnDefinition/>
                <ColumnDefinition/>
            </Grid.ColumnDefinitions>

            <Button Grid.Column="0"
                    Text="Tous"
                    Command="{Binding FilterCommand}"
                    CommandParameter="{x:Null}"
                    BackgroundColor="#F0F4F8"
                    TextColor="#1A1A1A"
                    FontSize="12"
                    CornerRadius="8"
                    Padding="8,6"/>

            <Button Grid.Column="1"
                    Text="Ouverts"
                    Command="{Binding FilterCommand}"
                    CommandParameter="0"
                    BackgroundColor="#F0F4F8"
                    TextColor="#1A1A1A"
                    FontSize="12"
                    CornerRadius="8"
                    Padding="8,6"/>

            <Button Grid.Column="2"
                    Text="Résolus"
                    Command="{Binding FilterCommand}"
                    CommandParameter="2"
                    BackgroundColor="#F0F4F8"
                    TextColor="#1A1A1A"
                    FontSize="12"
                    CornerRadius="8"
                    Padding="8,6"/>
        </Grid>

        <!-- Liste des tickets -->
        <CollectionView Grid.Row="3"
                        ItemsSource="{Binding Tickets}"
                        SelectionMode="Single"
                        SelectionChangedCommand="{Binding ViewDetailsCommand}"
                        ItemSpacing="8">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame BackgroundColor="White"
                           CornerRadius="12"
                           Padding="16"
                           HasShadow="True"
                           BorderColor="#F0F0F0"
                           Margin="0,4">
                        
                        <Grid RowSpacing="8">
                            <Grid.RowDefinitions>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                                <RowDefinition Height="Auto"/>
                            </Grid.RowDefinitions>
                            <Grid.ColumnDefinitions>
                                <ColumnDefinition Width="*"/>
                                <ColumnDefinition Width="Auto"/>
                            </Grid.ColumnDefinitions>

                            <!-- Titre et statut -->
                            <Label Grid.Row="0" Grid.Column="0"
                                   Text="{Binding Title}"
                                   FontSize="16"
                                   FontAttributes="Bold"
                                   TextColor="#1A1A1A"/>

                            <Frame Grid.Row="0" Grid.Column="1"
                                   BackgroundColor="{Binding Status, Converter={StaticResource StatusToColorConverter}}"
                                   CornerRadius="12"
                                   Padding="8,4"
                                   HasShadow="False">
                                <Label Text="{Binding Status, Converter={StaticResource StatusToStringConverter}}"
                                       FontSize="10"
                                       TextColor="White"
                                       FontAttributes="Bold"/>
                            </Frame>

                            <!-- Description -->
                            <Label Grid.Row="1" Grid.ColumnSpan="2"
                                   Text="{Binding Description}"
                                   FontSize="12"
                                   TextColor="#6C757D"
                                   MaxLines="2"
                                   LineBreakMode="TailTruncation"/>

                            <!-- Footer -->
                            <HorizontalStackLayout Grid.Row="2" Grid.ColumnSpan="2"
                                                  Spacing="16">
                                <Label Text="{Binding Type, Converter={StaticResource TypeToStringConverter}}"
                                       FontSize="12"
                                       TextColor="#ADB5BD"/>
                                <Label Text="{Binding DeviceLocation}"
                                       FontSize="12"
                                       TextColor="#ADB5BD"/>
                                <Label Text="{Binding CreatedDate, Converter={StaticResource RelativeTimeConverter}}"
                                       FontSize="12"
                                       TextColor="#ADB5BD"
                                       HorizontalOptions="EndAndExpand"/>
                            </HorizontalStackLayout>
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>

            <CollectionView.EmptyView>
                <Frame BackgroundColor="#F8F9FA"
                       CornerRadius="12"
                       Padding="20"
                       Margin="0,20"
                       HasShadow="False"
                       BorderColor="#E5E7EB">
                    <VerticalStackLayout>
                        <Label Text="??"
                               FontSize="48"
                               HorizontalOptions="Center"
                               Margin="0,0,0,10"/>
                        <Label Text="Aucun ticket trouvé"
                               FontSize="16"
                               FontAttributes="Bold"
                               TextColor="#1A1A1A"
                               HorizontalOptions="Center"/>
                        <Label Text="Créez votre premier ticket !"
                               FontSize="14"
                               TextColor="#6C757D"
                               HorizontalOptions="Center"/>
                    </VerticalStackLayout>
                </Frame>
            </CollectionView.EmptyView>
        </CollectionView>

        <!-- Indicateur de chargement -->
        <ActivityIndicator Grid.Row="3"
                          IsRunning="{Binding IsLoading}"
                          IsVisible="{Binding IsLoading}"
                          Color="#4A90E2"
                          HorizontalOptions="Center"
                          VerticalOptions="Center"/>
    </Grid>
</ContentPage>
```

### **6.2 Page XAML - Création de Ticket**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:SmartOfficeMobile.ViewModels"
             x:Class="SmartOfficeMobile.Pages.CreateTicketPage"
             x:DataType="vm:CreateTicketViewModel"
             Title="Nouveau Ticket"
             BackgroundColor="#F8F9FA">

    <ScrollView>
        <VerticalStackLayout Padding="20" Spacing="20">

            <!-- Header -->
            <Button Text="< Retour"
                    Command="{Binding CancelCommand}"
                    FontSize="16"
                    TextColor="#4A90E2"
                    BackgroundColor="Transparent"
                    HorizontalOptions="Start"
                    Padding="0,8"/>

            <Label Text="Nouveau Ticket"
                   FontSize="28"
                   FontAttributes="Bold"
                   TextColor="#1A1A1A"/>

            <Label Text="Signalez un problème ou faites une demande"
                   FontSize="16"
                   TextColor="#666666"/>

            <!-- Formulaire -->
            <Frame BackgroundColor="White"
                   CornerRadius="16"
                   HasShadow="True"
                   Padding="16">
                <VerticalStackLayout Spacing="12">
                    <Label Text="Titre du problème"
                           FontSize="16"
                           FontAttributes="Bold"/>
                    <Entry Text="{Binding Title}"
                           Placeholder="Ex: Climatisation ne fonctionne pas"
                           FontSize="14"/>
                </VerticalStackLayout>
            </Frame>

            <Frame BackgroundColor="White"
                   CornerRadius="16"
                   HasShadow="True"
                   Padding="16">
                <VerticalStackLayout Spacing="12">
                    <Label Text="Type de ticket"
                           FontSize="16"
                           FontAttributes="Bold"/>
                    <Picker ItemsSource="{Binding TicketTypes}"
                            SelectedIndex="{Binding SelectedTypeIndex}"
                            FontSize="14"/>
                </VerticalStackLayout>
            </Frame>

            <Frame BackgroundColor="White"
                   CornerRadius="16"
                   HasShadow="True"
                   Padding="16">
                <VerticalStackLayout Spacing="12">
                    <Label Text="Priorité"
                           FontSize="16"
                           FontAttributes="Bold"/>
                    <Picker ItemsSource="{Binding PriorityLevels}"
                            SelectedIndex="{Binding SelectedPriorityIndex}"
                            FontSize="14"/>
                </VerticalStackLayout>
            </Frame>

            <Frame BackgroundColor="White"
                   CornerRadius="16"
                   HasShadow="True"
                   Padding="16">
                <VerticalStackLayout Spacing="12">
                    <Label Text="Localisation/Équipement"
                           FontSize="16"
                           FontAttributes="Bold"/>
                    <Entry Text="{Binding DeviceLocation}"
                           Placeholder="Ex: Bureau principal, Salle de réunion..."
                           FontSize="14"/>
                </VerticalStackLayout>
            </Frame>

            <Frame BackgroundColor="White"
                   CornerRadius="16"
                   HasShadow="True"
                   Padding="16">
                <VerticalStackLayout Spacing="12">
                    <Label Text="Description détaillée"
                           FontSize="16"
                           FontAttributes="Bold"/>
                    <Editor Text="{Binding Description}"
                            Placeholder="Décrivez le problème en détail..."
                            HeightRequest="120"
                            FontSize="14"/>
                </VerticalStackLayout>
            </Frame>

            <!-- Conseils -->
            <Frame BackgroundColor="#E3F2FD"
                   CornerRadius="12"
                   Padding="16"
                   HasShadow="False">
                <HorizontalStackLayout Spacing="8">
                    <Label Text="??"
                           FontSize="20"
                           VerticalOptions="Start"/>
                    <Label Text="Conseil: Plus votre description est détaillée, plus vite nous pourrons résoudre votre problème!"
                           FontSize="12"
                           TextColor="#1976D2"
                           VerticalOptions="Center"/>
                </HorizontalStackLayout>
            </Frame>

            <!-- Boutons -->
            <Button Text="Créer le ticket"
                    Command="{Binding SubmitCommand}"
                    IsEnabled="{Binding IsSubmitting, Converter={StaticResource InverseBoolConverter}}"
                    FontSize="16"
                    FontAttributes="Bold"
                    TextColor="White"
                    BackgroundColor="#4A90E2"
                    CornerRadius="16"
                    Padding="24,16"
                    HeightRequest="56"/>

            <Button Text="Annuler"
                    Command="{Binding CancelCommand}"
                    FontSize="14"
                    TextColor="#666666"
                    BackgroundColor="Transparent"
                    CornerRadius="16"
                    Padding="24,12"
                    HeightRequest="48"/>

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

---

## ? **7. TESTS ET VALIDATION** {#tests-validation}

### **7.1 Tests Manuels**

```csharp
public class TicketTestsPage : ContentPage
{
    private readonly TicketService _ticketService;

    public TicketTestsPage(TicketService ticketService)
    {
        _ticketService = ticketService;
        CreateTestUI();
    }

    private void CreateTestUI()
    {
        var stack = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 16
        };

        stack.Children.Add(CreateTestButton("Test 1: Créer un ticket", TestCreateTicket));
        stack.Children.Add(CreateTestButton("Test 2: Lister les tickets", TestGetTickets));
        stack.Children.Add(CreateTestButton("Test 3: Filtrer par statut", TestFilterByStatus));
        stack.Children.Add(CreateTestButton("Test 4: Stats", TestGetStats));

        Content = new ScrollView { Content = stack };
    }

    private Button CreateTestButton(string text, Func<Task> testAction)
    {
        var button = new Button
        {
            Text = text,
            BackgroundColor = Color.FromHex("#4A90E2"),
            TextColor = Colors.White
        };
        button.Clicked += async (s, e) => await testAction();
        return button;
    }

    private async Task TestCreateTicket()
    {
        try
        {
            var request = new TicketCreateRequest
            {
                Title = "Test Ticket MAUI",
                Description = "Ceci est un ticket de test créé depuis MAUI",
                Type = 0, // TechnicalIssue
                Priority = 1, // Medium
                DeviceLocation = "Test Location"
            };

            var ticket = await _ticketService.CreateTicketAsync(request);

            await DisplayAlert("Succès", 
                $"Ticket créé: #{ticket?.Id}\nTitre: {ticket?.Title}", 
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async Task TestGetTickets()
    {
        try
        {
            var tickets = await _ticketService.GetUserTicketsAsync("utilisateur@smartoffice.tg");
            
            await DisplayAlert("Succès", 
                $"{tickets?.Count ?? 0} tickets récupérés", 
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async Task TestFilterByStatus()
    {
        try
        {
            var tickets = await _ticketService.GetTicketsByStatusAsync(TicketStatus.Open);
            
            await DisplayAlert("Succès", 
                $"{tickets?.Count ?? 0} tickets ouverts", 
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }

    private async Task TestGetStats()
    {
        try
        {
            var stats = await _ticketService.GetTicketStatsAsync();
            
            await DisplayAlert("Succès", 
                $"Total: {stats?.TotalTickets}\nOuverts: {stats?.OpenTickets}", 
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", ex.Message, "OK");
        }
    }
}
```

### **7.2 Checklist de Validation**

```
? Configuration
[ ] IP Backend correcte dans AppConfig
[ ] Pare-feu configuré
[ ] Backend démarré
[ ] Test connexion (http://IP:5290/health/live)

? Authentification
[ ] Login fonctionne
[ ] Token JWT stocké
[ ] Token ajouté aux headers

? Création de Tickets
[ ] Formulaire de création affiché
[ ] Validation des champs fonctionne
[ ] Ticket créé avec succès (POST)
[ ] Redirection vers liste après création

? Liste des Tickets
[ ] Tickets affichés
[ ] Filtrage par statut fonctionne
[ ] Statistiques affichées
[ ] Temps relatif correct

? Détails
[ ] Clic sur ticket affiche détails
[ ] Toutes les informations présentes
[ ] Résolution affichée si résolu

? Gestion d'Erreurs
[ ] Erreur réseau gérée
[ ] Messages d'erreur clairs
[ ] Pas de crash sur erreur
```

---

## ?? **8. DÉPANNAGE** {#depannage}

### **Problème: "Cannot create ticket" (401)**
**Solution:**
- Vérifier que le token JWT est valide
- Vérifier que l'email utilisateur est correct
- Relancer le login si nécessaire

### **Problème: "Tickets list empty"**
**Solution:**
- Vérifier que l'utilisateur a des tickets en base
- Tester avec `GET /api/IncidentTickets/user/{email}` dans HTTP file
- Créer un ticket de test via backend

### **Problème: "Type/Priority/Status not matching"**
**Solution:**
- Vérifier que les enums correspondent entre mobile et backend
- Utiliser les valeurs numériques (0, 1, 2, 3)
- Ne pas utiliser les strings ("Open", "InProgress", etc.)

### **Problème: "Cannot connect to backend"**
**Solution:**
- Vérifier IP dans `AppConfig`
- Backend démarré: `dotnet run --launch-profile http`
- Pare-feu configuré: `.\configure-firewall.ps1`
- Test navigateur: `http://IP:5290/api/IncidentTickets/stats`

---

## ?? **RESSOURCES ADDITIONNELLES**

| Document | Description |
|----------|-------------|
| [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md) | Index complet mobile |
| [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) | Guide périphériques |
| [Tests/37-Incidents-Complete.http](Tests/37-Incidents-Complete.http) | Tests HTTP backend |
| [FRONTEND-INCIDENTS-README.md](FRONTEND-INCIDENTS-README.md) | Documentation frontend |

---

## ?? **ENDPOINTS API DISPONIBLES**

### **Tickets**
```
POST   /api/IncidentTickets              - Créer un ticket
GET    /api/IncidentTickets/{id}         - Récupérer un ticket
GET    /api/IncidentTickets/user/{email} - Tickets de l'utilisateur
GET    /api/IncidentTickets/status/{status} - Filtrer par statut
GET    /api/IncidentTickets/type/{type}  - Filtrer par type
GET    /api/IncidentTickets/priority/{priority} - Filtrer par priorité
GET    /api/IncidentTickets/stats        - Statistiques
PUT    /api/IncidentTickets/{id}         - Mettre à jour (Admin)
PUT    /api/IncidentTickets/{id}/assign  - Assigner (Admin)
PUT    /api/IncidentTickets/{id}/resolve - Résoudre (Admin)
```

---

**? Vous êtes maintenant prêt à gérer les tickets depuis MAUI !** ??

**Temps total estimé:** 2-3 heures pour l'implémentation complète  
**Niveau:** Intermédiaire  
**Support:** Consultez les fichiers de documentation dans `SmartOfficeAPP/`
