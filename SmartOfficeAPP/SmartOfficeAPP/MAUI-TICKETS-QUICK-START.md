# ? Démarrage Rapide - Tickets & Incidents MAUI

## ?? En 5 Minutes

### **1. Modèles (2 min)**

#### **Ticket.cs**
```csharp
namespace SmartOfficeMobile.Models;

public enum TicketStatus { Open = 0, InProgress = 1, Resolved = 2, Closed = 3 }
public enum TicketType { TechnicalIssue = 0, MaintenanceRequest = 1, NewFeatureRequest = 2, AccessIssue = 3, Other = 4 }
public enum TicketPriority { Low = 0, Medium = 1, High = 2, Critical = 3 }

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public string DeviceLocation { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class TicketCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Type { get; set; }
    public int Priority { get; set; }
    public string DeviceLocation { get; set; } = string.Empty;
}
```

---

### **2. Service (2 min)**

#### **TicketService.cs**
```csharp
namespace SmartOfficeMobile.Services;

public class TicketService
{
    private readonly ApiService _api;

    public TicketService(ApiService api) => _api = api;

    // Créer un ticket
    public async Task<Ticket?> CreateTicketAsync(TicketCreateRequest request) =>
        await _api.PostAsync<TicketCreateRequest, Ticket>("/api/IncidentTickets", request);

    // Lister mes tickets
    public async Task<List<Ticket>?> GetUserTicketsAsync(string email) =>
        await _api.GetAsync<List<Ticket>>($"/api/IncidentTickets/user/{Uri.EscapeDataString(email)}");

    // Filtrer par statut
    public async Task<List<Ticket>?> GetTicketsByStatusAsync(TicketStatus status) =>
        await _api.GetAsync<List<Ticket>>($"/api/IncidentTickets/status/{(int)status}");

    // Helpers
    public static string GetStatusText(TicketStatus status) => status switch
    {
        TicketStatus.Open => "Ouvert",
        TicketStatus.InProgress => "En cours",
        TicketStatus.Resolved => "Résolu",
        TicketStatus.Closed => "Fermé",
        _ => "Inconnu"
    };

    public static Color GetStatusColor(TicketStatus status) => status switch
    {
        TicketStatus.Open => Color.FromArgb("#FF6B6B"),
        TicketStatus.InProgress => Color.FromArgb("#FFB74D"),
        TicketStatus.Resolved => Color.FromArgb("#4ECDC4"),
        TicketStatus.Closed => Color.FromArgb("#ADB5BD"),
        _ => Color.FromArgb("#6C757D")
    };
}
```

---

### **3. Utilisation (1 min)**

#### **Créer un ticket**
```csharp
var ticketService = new TicketService(apiService);

var request = new TicketCreateRequest
{
    Title = "Climatisation ne fonctionne pas",
    Description = "La clim du bureau principal ne démarre plus depuis ce matin",
    Type = 0, // TechnicalIssue
    Priority = 2, // High
    DeviceLocation = "Bureau principal"
};

var ticket = await ticketService.CreateTicketAsync(request);

if (ticket != null)
{
    await DisplayAlert("Succès", $"Ticket #{ticket.Id} créé", "OK");
}
```

#### **Lister les tickets**
```csharp
var tickets = await ticketService.GetUserTicketsAsync("user@smartoffice.com");

foreach (var ticket in tickets)
{
    Console.WriteLine($"#{ticket.Id} - {ticket.Title} [{ticket.Status}]");
}
```

#### **Filtrer par statut**
```csharp
// Tickets ouverts seulement
var openTickets = await ticketService.GetTicketsByStatusAsync(TicketStatus.Open);
```

---

## ?? **Interface Utilisateur Minimale**

### **Page de Liste (Code-Behind)**

```csharp
public class TicketsPage : ContentPage
{
    private readonly TicketService _ticketService;
    private StackLayout _ticketsStack;

    public TicketsPage(TicketService ticketService)
    {
        _ticketService = ticketService;
        Title = "Tickets";
        
        var scrollView = new ScrollView();
        _ticketsStack = new StackLayout { Padding = 20, Spacing = 12 };
        scrollView.Content = _ticketsStack;
        Content = scrollView;

        LoadTickets();
    }

    private async void LoadTickets()
    {
        var tickets = await _ticketService.GetUserTicketsAsync("user@smartoffice.com");
        
        _ticketsStack.Children.Clear();
        
        foreach (var ticket in tickets ?? new List<Ticket>())
        {
            _ticketsStack.Children.Add(CreateTicketCard(ticket));
        }
    }

    private Frame CreateTicketCard(Ticket ticket)
    {
        return new Frame
        {
            BackgroundColor = Colors.White,
            CornerRadius = 12,
            Padding = 16,
            HasShadow = true,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = ticket.Title,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label
                    {
                        Text = ticket.Description,
                        FontSize = 12,
                        TextColor = Color.FromArgb("#666666"),
                        MaxLines = 2
                    },
                    new HorizontalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            new Label
                            {
                                Text = TicketService.GetStatusText(ticket.Status),
                                FontSize = 10,
                                TextColor = TicketService.GetStatusColor(ticket.Status),
                                FontAttributes = FontAttributes.Bold
                            },
                            new Label
                            {
                                Text = ticket.DeviceLocation,
                                FontSize = 10,
                                TextColor = Color.FromArgb("#999999")
                            }
                        }
                    }
                }
            }
        };
    }
}
```

### **Page de Création (Code-Behind)**

```csharp
public class CreateTicketPage : ContentPage
{
    private readonly TicketService _ticketService;
    private Entry _titleEntry;
    private Editor _descriptionEditor;
    private Picker _typePicker;
    private Picker _priorityPicker;
    private Entry _locationEntry;

    public CreateTicketPage(TicketService ticketService)
    {
        _ticketService = ticketService;
        Title = "Nouveau Ticket";
        
        var stack = new VerticalStackLayout { Padding = 20, Spacing = 16 };

        _titleEntry = new Entry { Placeholder = "Titre du problème" };
        _descriptionEditor = new Editor { Placeholder = "Description", HeightRequest = 100 };
        
        _typePicker = new Picker { Title = "Type" };
        _typePicker.Items.Add("Problème technique");
        _typePicker.Items.Add("Maintenance");
        _typePicker.Items.Add("Nouvelle fonctionnalité");
        _typePicker.Items.Add("Problème d'accès");
        _typePicker.Items.Add("Autre");
        _typePicker.SelectedIndex = 0;

        _priorityPicker = new Picker { Title = "Priorité" };
        _priorityPicker.Items.Add("Basse");
        _priorityPicker.Items.Add("Moyenne");
        _priorityPicker.Items.Add("Haute");
        _priorityPicker.Items.Add("Critique");
        _priorityPicker.SelectedIndex = 1;

        _locationEntry = new Entry { Placeholder = "Localisation" };

        var submitButton = new Button
        {
            Text = "Créer le ticket",
            BackgroundColor = Color.FromHex("#4A90E2"),
            TextColor = Colors.White
        };
        submitButton.Clicked += OnSubmitClicked;

        stack.Children.Add(_titleEntry);
        stack.Children.Add(_descriptionEditor);
        stack.Children.Add(_typePicker);
        stack.Children.Add(_priorityPicker);
        stack.Children.Add(_locationEntry);
        stack.Children.Add(submitButton);

        Content = new ScrollView { Content = stack };
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_titleEntry.Text))
        {
            await DisplayAlert("Erreur", "Titre requis", "OK");
            return;
        }

        var request = new TicketCreateRequest
        {
            Title = _titleEntry.Text.Trim(),
            Description = _descriptionEditor.Text?.Trim() ?? "",
            Type = _typePicker.SelectedIndex,
            Priority = _priorityPicker.SelectedIndex,
            DeviceLocation = _locationEntry.Text?.Trim() ?? "Non spécifié"
        };

        var ticket = await _ticketService.CreateTicketAsync(request);

        if (ticket != null)
        {
            await DisplayAlert("Succès", $"Ticket #{ticket.Id} créé", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
```

---

## ?? **Endpoints API**

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| **POST** | `/api/IncidentTickets` | Créer un ticket |
| **GET** | `/api/IncidentTickets/user/{email}` | Mes tickets |
| **GET** | `/api/IncidentTickets/{id}` | Détails d'un ticket |
| **GET** | `/api/IncidentTickets/status/{status}` | Filtrer par statut (0-3) |
| **GET** | `/api/IncidentTickets/type/{type}` | Filtrer par type (0-4) |
| **GET** | `/api/IncidentTickets/priority/{priority}` | Filtrer par priorité (0-3) |
| **GET** | `/api/IncidentTickets/stats` | Statistiques |

---

## ?? **Valeurs des Enums**

### **TicketStatus**
- `0` = Open (Ouvert)
- `1` = InProgress (En cours)
- `2` = Resolved (Résolu)
- `3` = Closed (Fermé)

### **TicketType**
- `0` = TechnicalIssue (Problème technique)
- `1` = MaintenanceRequest (Maintenance)
- `2` = NewFeatureRequest (Nouvelle fonctionnalité)
- `3` = AccessIssue (Problème d'accès)
- `4` = Other (Autre)

### **TicketPriority**
- `0` = Low (Basse)
- `1` = Medium (Moyenne)
- `2` = High (Haute)
- `3` = Critical (Critique)

---

## ? Checklist Rapide

- [ ] Modèles créés (`Ticket`, `TicketCreateRequest`)
- [ ] `TicketService` créé
- [ ] Page liste créée
- [ ] Page création créée
- [ ] Test création ticket OK
- [ ] Test liste tickets OK
- [ ] Test filtre statut OK

---

## ?? Dépannage Express

| Problème | Solution |
|----------|----------|
| "Unauthorized" | Token expiré, relancer login |
| "Tickets list empty" | Vérifier email utilisateur |
| "Cannot create ticket" | Vérifier tous les champs requis (Title, Description) |
| "Wrong enum value" | Utiliser valeurs numériques (0, 1, 2) pas les strings |

---

## ?? Documentation Complète

Pour plus de détails :
- [MAUI-TICKETS-COMPLETE-GUIDE.md](MAUI-TICKETS-COMPLETE-GUIDE.md) ? Guide complet avec MVVM, XAML, tests
- [Tests/37-Incidents-Complete.http](Tests/37-Incidents-Complete.http) ? Tests HTTP
- [FRONTEND-INCIDENTS-README.md](FRONTEND-INCIDENTS-README.md) ? Documentation frontend

---

**?? C'est tout ! Vous pouvez maintenant gérer les tickets depuis MAUI !**

**Temps total:** 5 minutes ??
