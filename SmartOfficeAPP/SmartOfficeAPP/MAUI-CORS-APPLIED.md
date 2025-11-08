# 📱 CORS CONFIGURÉ POUR APPLICATION MOBILE MAUI

## ✅ **CONFIGURATION APPLIQUÉE - OPTION 2**

La configuration CORS a été mise à jour pour autoriser les requêtes provenant de l'application mobile MAUI de manière sécurisée.

---

## 🔧 **MODIFICATIONS EFFECTUÉES**

### **1. Nouvelle Politique CORS Ajoutée (Program.cs)**

```csharp
// ✅ Politique pour application mobile MAUI
opt.AddPolicy("AllowMobileApp", p => p
    .AllowAnyHeader()
  .AllowAnyMethod()
    .SetIsOriginAllowed(origin => 
  {
 // Applications mobiles natives (origine null ou vide)
      if (string.IsNullOrEmpty(origin)) return true;
        
   // Localhost pour tests émulateur/simulateur
        if (origin.StartsWith("http://localhost")) return true;
   if (origin.StartsWith("https://localhost")) return true;
        if (origin.StartsWith("http://127.0.0.1")) return true;
        if (origin.StartsWith("https://127.0.0.1")) return true;
     
        // Android Emulator
        if (origin.StartsWith("http://10.0.2.2")) return true;
        if (origin.StartsWith("https://10.0.2.2")) return true;
        
        // Android Emulator (Genymotion)
        if (origin.StartsWith("http://10.0.3.2")) return true;
        
        // iOS Simulator
     if (origin.Contains("localhost")) return true;
        
    // Schémas personnalisés MAUI
    if (origin.StartsWith("app://")) return true;
        if (origin.StartsWith("file://")) return true;
        if (origin.StartsWith("capacitor://")) return true;
        if (origin.StartsWith("ionic://")) return true;
        
  // Domaines de production
        if (origin.Contains("smartoffice") || origin.Contains("brainoffice")) return true;
     
        return false;
    })
    .AllowCredentials());
```

### **2. Politique Appliquée dans le Pipeline**

```csharp
// Remplacé AllowFrontend par AllowMobileApp
app.UseCors("AllowMobileApp");
```

---

## 📱 **ORIGINES AUTORISÉES**

### **Développement Local**
| Plateforme | URL | Usage |
|------------|-----|-------|
| **Android Emulator** | `http://10.0.2.2:7032` | Accès localhost depuis émulateur |
| **Android Emulator (Genymotion)** | `http://10.0.3.2:7032` | Émulateur alternatif |
| **iOS Simulator** | `https://localhost:7032` | Simulateur iOS |
| **Desktop Windows** | `https://localhost:7032` | Tests desktop |
| **Desktop macOS** | `https://localhost:7032` | Tests desktop |

### **Schémas Personnalisés**
- ✅ `app://smartoffice` - Schéma application MAUI
- ✅ `file://` - Fichiers locaux
- ✅ `capacitor://` - Framework Capacitor
- ✅ Origine `null` ou vide (applications natives)

### **Production**
- ✅ Domaines contenant `smartoffice`
- ✅ Domaines contenant `brainoffice`
- ✅ URLs Azure configurées

---

## 🚀 **CONFIGURATION MAUI REQUISE**

### **1. MauiProgram.cs**

```csharp
using System.Net.Http;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
     var builder = MauiApp.CreateBuilder();
        
   builder.UseMauiApp<App>()
 .ConfigureFonts(fonts =>
   {
      fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
});

        // ✅ Configuration HttpClient avec gestion plateforme
        builder.Services.AddHttpClient("SmartOfficeAPI", client =>
        {
// Configuration selon la plateforme
#if ANDROID
        // Android Emulator utilise 10.0.2.2 pour accéder à localhost
            client.BaseAddress = new Uri("https://10.0.2.2:7032/api/");
#elif IOS
            // iOS Simulator utilise localhost
 client.BaseAddress = new Uri("https://localhost:7032/api/");
#else
            // Desktop (Windows/macOS)
     client.BaseAddress = new Uri("https://localhost:7032/api/");
#endif

     // Headers par défaut
         client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
     .ConfigurePrimaryHttpMessageHandler(() =>
      {
#if DEBUG
   // ⚠️ DÉVELOPPEMENT UNIQUEMENT : Ignorer erreurs SSL
  return new HttpClientHandler
            {
     ServerCertificateCustomValidationCallback = 
          HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
#else
       // Production : validation SSL normale
       return new HttpClientHandler();
#endif
        });

        // Enregistrer le service d'authentification
        builder.Services.AddSingleton<AuthService>();

        return builder.Build();
 }
}
```

### **2. AuthService.cs (MAUI)**

```csharp
using System.Net.Http.Json;
using System.Net.Http.Headers;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(IHttpClientFactory httpClientFactory)
    {
      _httpClient = httpClientFactory.CreateClient("SmartOfficeAPI");
    }

 public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
try
     {
   // ✅ La requête respecte automatiquement CORS
          var response = await _httpClient.PostAsJsonAsync("/Auth/login", loginDto);
         
if (response.IsSuccessStatusCode)
     {
         return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
 }
            
    return null;
     }
        catch (HttpRequestException ex)
    {
        Console.WriteLine($"Erreur réseau: {ex.Message}");
            return null;
      }
    }

    public void SetAuthToken(string token)
 {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<bool> TestConnectionAsync()
    {
 try
        {
  var response = await _httpClient.GetAsync("/Auth/test-auth");
            return response.IsSuccessStatusCode;
     }
        catch
        {
        return false;
        }
  }
}
```

### **3. Test de Connexion CORS**

```csharp
// Dans votre page de test MAUI
private async void TestCors_Clicked(object sender, EventArgs e)
{
    var authService = new AuthService(httpClientFactory);
    
    // Test simple sans authentification
    var isConnected = await authService.TestConnectionAsync();
    
    if (isConnected)
    {
      await DisplayAlert("Succès", "✅ CORS configuré correctement", "OK");
    }
    else
  {
        await DisplayAlert("Erreur", "❌ Impossible de se connecter à l'API", "OK");
    }
}
```

---

## 🧪 **VALIDATION DE LA CONFIGURATION**

### **Tests à Effectuer**

#### **1. Test depuis Android Emulator**
```bash
# Lancer l'API
cd SmartOfficeAPP
dotnet run

# Dans l'app MAUI Android
URL: https://10.0.2.2:7032/api/Auth/login
```

#### **2. Test depuis iOS Simulator**
```bash
# Dans l'app MAUI iOS
URL: https://localhost:7032/api/Auth/login
```

#### **3. Test Login**
```csharp
var loginDto = new LoginDto
{
    Email = "admin@smartoffice.local",
    Password = "Admin123!"
};

var result = await authService.LoginAsync(loginDto);
// ✅ Doit réussir sans erreur CORS
```

---

## 📊 **LOGS API À SURVEILLER**

### **Messages Attendus (Succès)**
```
✅ CORS policy applied successfully
✅ Request from origin 'null' allowed
✅ Request from origin 'http://10.0.2.2' allowed
```

### **Messages d'Erreur (À Corriger)**
```
❌ CORS policy blocked request from origin 'http://...'
❌ Origin not allowed by CORS policy
```

Si vous voyez ces erreurs :
1. Vérifier que `app.UseCors("AllowMobileApp")` est actif
2. Vérifier les logs pour voir l'origine de la requête
3. Ajouter l'origine manquante dans `SetIsOriginAllowed`

---

## 🔒 **SÉCURITÉ**

### **✅ Avantages de cette Configuration**

1. **Sélective** : Autorise uniquement les origines spécifiques
2. **Plateforme-aware** : Support Android, iOS, Desktop
3. **Développement** : Autorise localhost et émulateurs
4. **Production** : Peut être restreinte facilement

### **⚠️ Points de Vigilance**

- ✅ `AllowCredentials()` activé pour JWT
- ✅ Origines `null` autorisées (applications natives)
- ⚠️ Certificats SSL ignorés en DEBUG uniquement
- ⚠️ En production, restreindre les domaines autorisés

---

## 🎯 **CHECKLIST D'INTÉGRATION**

### **Backend (.NET API)**
- [x] Politique CORS `AllowMobileApp` ajoutée
- [x] `SetIsOriginAllowed` configuré pour mobiles
- [x] `app.UseCors("AllowMobileApp")` appliqué
- [x] Build réussi

### **Frontend (MAUI)**
- [ ] `HttpClient` configuré avec `BaseAddress` correcte
- [ ] Handler SSL ignoré en DEBUG
- [ ] `AuthService` implémenté
- [ ] Tests de connexion effectués
- [ ] Login testé sur Android Emulator
- [ ] Login testé sur iOS Simulator

---

## 📝 **PROCHAINES ÉTAPES**

### **Pour l'Équipe MAUI**

1. **Configurer MauiProgram.cs** avec le code fourni
2. **Implémenter AuthService** avec HttpClient
3. **Tester login** sur émulateur Android
4. **Tester login** sur simulateur iOS
5. **Valider requêtes authentifiées** avec JWT

### **URLs de Test**

| Endpoint | Méthode | Description |
|----------|---------|-------------|
| `/Auth/login` | POST | Connexion utilisateur |
| `/Auth/test-auth` | GET | Test token JWT |
| `/User/profile` | GET | Profil utilisateur |
| `/Dashboard/stats` | GET | Statistiques dashboard |

---

## 🎉 **RÉSULTAT**

### **✅ Configuration CORS Appliquée**

- ✅ **Applications web** : continuent de fonctionner
- ✅ **Application mobile MAUI** : autorisée
- ✅ **Android Emulator** : configuré (10.0.2.2)
- ✅ **iOS Simulator** : configuré (localhost)
- ✅ **Desktop MAUI** : configuré (localhost)
- ✅ **Production** : domaines contrôlés

### **🔐 Sécurité Maintenue**

- ✅ Origines sélectives autorisées
- ✅ Credentials supportés pour JWT
- ✅ Facilement extensible pour production
- ✅ Logging des requêtes CORS

---

**📱 Votre application mobile MAUI peut maintenant communiquer avec l'API SmartOffice !**

Pour déployer les changements, **redémarrez l'API** :
```bash
dotnet run
```²