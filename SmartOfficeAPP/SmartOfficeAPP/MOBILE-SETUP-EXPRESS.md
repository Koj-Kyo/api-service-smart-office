# ? CONFIGURATION EXPRESS - 3 COMMANDES

## ?? Ce Qu'il Faut Faire (5 minutes chrono)

---

## 1?? Trouvez Votre IP (30 secondes)

**Ouvrez PowerShell (pas besoin d'admin)** et tapez :

```powershell
ipconfig | findstr "IPv4"
```

**Résultat exemple :**
```
Adresse IPv4. . . . . . . . . . . . . : 192.168.1.100
```

?? **Notez cette adresse : `192.168.1.100`** (exemple)

---

## 2?? Autorisez les Ports (1 minute)

**?? IMPORTANT : Ouvrez un NOUVEAU PowerShell EN TANT QU'ADMINISTRATEUR**

**Comment ?**
- Menu Démarrer ? Tapez "PowerShell"
- **Clic droit** ? "Exécuter en tant qu'administrateur"

**Copier-coller ces 2 lignes :**

```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
.\configure-firewall.ps1
```

**Résultat attendu :**
```
? Règle HTTP créée avec succès
? Règle HTTPS créée avec succès
```

---

## 3?? Démarrez le Backend (30 secondes)

**Dans un terminal NORMAL (pas admin)** :

```bash
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
dotnet run --launch-profile http
```

**Attendez de voir :**
```
Now listening on: http://0.0.0.0:5290
Application started.
```

? **Backend prêt !** Laissez ce terminal ouvert.

---

## 4?? Testez depuis le Téléphone (1 minute)

1. **Connectez votre téléphone au MÊME WiFi que votre PC**
2. **Ouvrez le navigateur mobile**
3. **Allez sur :** `http://192.168.1.100:5290/health/live` (remplacez par votre IP)
4. **Résultat attendu :** `Healthy`

? Si vous voyez "Healthy", ça marche !

---

## 5?? Configurez Votre App Mobile (2 minutes)

**Dans votre projet MAUI**, trouvez le fichier qui contient l'URL de l'API.

**Cherchez une ligne comme :**
```csharp
private const string BaseUrl = "http://localhost:5290";
```

**Remplacez par :**
```csharp
private const string BaseUrl = "http://192.168.1.100:5290"; // VOTRE IP
```

---

## ?? Où Mettre l'IP dans le Code MAUI ?

### Fichiers à Chercher :

1. `Services/ApiService.cs`
2. `Helpers/Constants.cs`
3. `Config/AppSettings.cs`

### Exemple Complet :

Copiez ce fichier dans votre projet MAUI : `Services/ApiService.cs`

```csharp
using System.Net.Http;
using System.Net.Http.Json;

namespace VotreApp.Services
{
    public class ApiService
    {
        // ?????? METTEZ VOTRE IP ICI ??????
        private const string BaseUrl = "http://192.168.1.100:5290";
        
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient 
            { 
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health/live");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var request = new { email, password };
            var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", request);
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
    }
}
```

---

## ?? Récap Ultra-Rapide

| Étape | Où | Quoi |
|-------|----|----|
| 1 | PowerShell normal | `ipconfig` ? Notez l'IP |
| 2 | PowerShell **ADMIN** | `.\configure-firewall.ps1` |
| 3 | Terminal normal | `dotnet run --launch-profile http` |
| 4 | Navigateur mobile | Test : `http://VOTRE_IP:5290/health/live` |
| 5 | Code MAUI | Remplacez `localhost` par `VOTRE_IP` |

---

## ? Test Final

**Depuis votre app mobile, testez :**

```csharp
var apiService = new ApiService();
var isConnected = await apiService.TestConnectionAsync();

if (isConnected)
    Console.WriteLine("? Connexion OK !");
else
    Console.WriteLine("? Erreur de connexion");
```

---

## ?? Problème ?

### "Connection refused"
? Le backend est-il démarré ? (`dotnet run`)

### "Timeout"
? Téléphone et PC sur le même WiFi ?

### "Firewall error"
? Script exécuté en **administrateur** ?

---

## ?? Besoin d'Aide Détaillée ?

Consultez : `MOBILE-SETUP-VISUAL-GUIDE.md`

---

**Total : 5 minutes** ??  
**Prêt à développer ! ??**
