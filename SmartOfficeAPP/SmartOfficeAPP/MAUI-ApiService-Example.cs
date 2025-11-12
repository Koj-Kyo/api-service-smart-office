// ============================================
// EXEMPLE - Configuration API pour MAUI
// ============================================
// 
// Créez ce fichier dans votre projet MAUI :
// Services/ApiService.cs
//

using System.Net.Http;
using System.Net.Http.Json;

namespace VotreAppMobile.Services
{
    public class ApiService
    {
        // ?? IMPORTANT : Remplacez 192.168.1.100 par l'adresse IP de VOTRE PC
        // Trouvez votre IP avec : ipconfig (dans PowerShell)
        private const string BaseUrl = "http://192.168.1.100:5290";
        
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            // Configuration pour accepter tous les certificats SSL (DEV UNIQUEMENT)
            var handler = new HttpClientHandler
            {
                #if DEBUG
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                #endif
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        // Test de connexion simple
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health/live");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur connexion: {ex.Message}");
                return false;
            }
        }

        // Login
        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new { email, password };
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<LoginResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur login: {ex.Message}");
                throw;
            }
        }
    }

    // Classe pour la réponse de login
    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? UserId { get; set; }
    }
}
