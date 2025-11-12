# ?? Configuration Rapide - Où Mettre l'Adresse IP ?

## ?? Checklist Visuelle

### ? Étape 1 : Trouvez Votre IP (FAIT)

Vous avez exécuté `ipconfig` et trouvé votre IP.

**Exemple :** `192.168.1.100`

?? **Notez cette adresse, vous allez l'utiliser 2 fois :**
1. Pour tester dans le navigateur
2. Pour configurer votre app mobile

---

## ?? Étape 2 : Autoriser les Ports (À FAIRE MAINTENANT)

### Option Facile : Script Automatique

1. **Clic droit** sur le menu Démarrer ? **Windows PowerShell (Admin)**

2. **Copier-coller ces 2 lignes** :
   ```powershell
   cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
   .\configure-firewall.ps1
   ```

3. **Appuyez sur Entrée**

4. **Résultat attendu :**
   ```
   ? Règle HTTP créée avec succès
   ? Règle HTTPS créée avec succès
   ?? Votre adresse IP locale:
      ? 192.168.1.100 (Wi-Fi)
   ```

### Alternative : Commandes Manuelles

Si le script ne fonctionne pas, copier-coller ces commandes (toujours dans PowerShell Admin) :

```powershell
# Port 5290 (HTTP)
New-NetFirewallRule -DisplayName "SmartOffice API HTTP" -Direction Inbound -LocalPort 5290 -Protocol TCP -Action Allow

# Port 7144 (HTTPS)
New-NetFirewallRule -DisplayName "SmartOffice API HTTPS" -Direction Inbound -LocalPort 7144 -Protocol TCP -Action Allow
```

---

## ?? Étape 3 : Démarrer le Backend

**Dans un terminal normal (pas besoin d'admin)** :

```bash
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
dotnet run --launch-profile http
```

**Attendez de voir :**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:5290
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

? **Le backend est prêt !**

---

## ?? Étape 4 : Mettre l'IP dans l'App Mobile

### Où exactement ?

Dans votre projet MAUI, cherchez le fichier qui configure l'URL de l'API.

**Fichiers possibles :**
- `Services/ApiService.cs`
- `Helpers/Constants.cs`
- `Config/AppSettings.cs`
- `ViewModels/BaseViewModel.cs`

### Cherchez une ligne comme :

```csharp
private const string BaseUrl = "http://localhost:5290";
// OU
private const string ApiUrl = "https://localhost:7144";
// OU
public static string BackendUrl = "http://10.0.2.2:5290";
```

### Remplacez par :

```csharp
private const string BaseUrl = "http://192.168.1.100:5290"; // ?? VOTRE IP ICI
```

**?? Important :**
- Remplacez `192.168.1.100` par VOTRE adresse IP
- Gardez le port `:5290`
- Utilisez `http://` (pas `https://` pour simplifier en dev)

---

## ?? Étape 5 : Tester

### Test 1 : Depuis le Navigateur de Votre Téléphone

1. **Connectez votre téléphone au même WiFi que votre PC**
2. **Ouvrez le navigateur mobile** (Chrome, Safari, etc.)
3. **Allez sur :** `http://192.168.1.100:5290/health/live` (votre IP)
4. **Résultat attendu :** `Healthy`

? Si vous voyez "Healthy", la connexion fonctionne !

### Test 2 : Depuis Votre App Mobile

1. **Déployez l'app** sur votre téléphone
2. **Testez la connexion** (bouton "Test Connection" ou équivalent)
3. **Résultat attendu :** "Connexion réussie"

---

## ?? Récapitulatif des Adresses IP

| Utilisation | Adresse | Où ? |
|-------------|---------|------|
| **Émulateur Android** | `http://10.0.2.2:5290` | ApiService.cs |
| **Simulateur iOS** | `http://localhost:5290` | ApiService.cs |
| **Téléphone Physique** | `http://192.168.1.100:5290` | ApiService.cs |

**Pour un téléphone physique, utilisez TOUJOURS l'IP de votre PC !**

---

## ?? Exemples de Code

### Exemple 1 : ApiService Simple

```csharp
public class ApiService
{
    // ?? METTEZ VOTRE IP ICI
    private const string BaseUrl = "http://192.168.1.100:5290";
    
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
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
}
```

### Exemple 2 : Configuration avec Settings

```csharp
public static class AppSettings
{
    // ?? METTEZ VOTRE IP ICI
    #if DEBUG
    public const string ApiBaseUrl = "http://192.168.1.100:5290";
    #else
    public const string ApiBaseUrl = "https://api.production.com";
    #endif
}

// Utilisation :
_httpClient.BaseAddress = new Uri(AppSettings.ApiBaseUrl);
```

### Exemple 3 : Test dans une Page MAUI

```csharp
public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;

    public MainPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    private async void OnTestConnectionClicked(object sender, EventArgs e)
    {
        var isConnected = await _apiService.TestConnectionAsync();
        
        if (isConnected)
        {
            await DisplayAlert("? Succès", 
                "Connexion au serveur réussie !", 
                "OK");
        }
        else
        {
            await DisplayAlert("? Erreur", 
                "Impossible de se connecter. Vérifiez l'IP et le WiFi.", 
                "OK");
        }
    }
}
```

---

## ?? Problèmes Courants

### Erreur : "Connection refused"

**Causes possibles :**
1. Le backend n'est pas démarré
2. L'adresse IP est incorrecte
3. Le pare-feu bloque les ports

**Solution :**
```bash
# Vérifiez que le backend est démarré
dotnet run --launch-profile http

# Vérifiez votre IP
ipconfig | findstr "IPv4"

# Vérifiez les règles de pare-feu
Get-NetFirewallRule -DisplayName "SmartOffice*"
```

### Erreur : "Timeout"

**Causes possibles :**
1. Téléphone et PC ne sont pas sur le même WiFi
2. Le pare-feu bloque la connexion

**Solution :**
1. Connectez le téléphone au **même WiFi** que le PC
2. Exécutez `.\configure-firewall.ps1` en admin

### Erreur : "SSL/TLS error"

**Cause :** Certificat SSL non valide en développement

**Solution :**
```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = 
        (message, cert, chain, errors) => true
};
```

---

## ? Checklist Finale

- [ ] IP trouvée avec `ipconfig`
- [ ] Pare-feu configuré (script ou commandes manuelles)
- [ ] Backend démarré avec `dotnet run --launch-profile http`
- [ ] Health check OK depuis navigateur PC : `http://192.168.1.100:5290/health/live`
- [ ] Health check OK depuis navigateur mobile : `http://192.168.1.100:5290/health/live`
- [ ] IP configurée dans `ApiService.cs` (ou équivalent)
- [ ] App déployée sur le téléphone
- [ ] Test de connexion réussi depuis l'app

---

## ?? Vous êtes Prêt !

Une fois toutes ces étapes validées, vous pouvez commencer à développer votre application mobile !

**Temps total : ~5 minutes** ??

---

## ?? Besoin d'Aide ?

Si vous avez un problème, vérifiez :
1. Les logs du backend (dans le terminal)
2. Que téléphone et PC sont sur le même WiFi
3. Que l'IP dans le code est la bonne
4. Que le pare-feu autorise les ports 5290 et 7144

**Bon développement ! ??**
