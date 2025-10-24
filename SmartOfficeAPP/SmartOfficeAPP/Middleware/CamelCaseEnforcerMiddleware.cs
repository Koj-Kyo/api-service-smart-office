using System.Text;
using System.Text.Json;

namespace SmartOfficeAPP.Middleware;

/// <summary>
/// Middleware qui FORCE la conversion en camelCase pour TOUTES les réponses JSON
/// À utiliser en dernier recours si la configuration normale ne fonctionne pas
/// </summary>
public class CamelCaseEnforcerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CamelCaseEnforcerMiddleware> _logger;

    public CamelCaseEnforcerMiddleware(RequestDelegate next, ILogger<CamelCaseEnforcerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Capturer la réponse originale
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Si la réponse est JSON, convertir en camelCase
        if (context.Response.ContentType?.Contains("application/json") == true)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();

            if (!string.IsNullOrEmpty(responseText))
            {
                try
                {
                    // Désérialiser avec options PascalCase
                    var jsonDoc = JsonSerializer.Deserialize<JsonElement>(responseText);

                    // Re-sérialiser avec camelCase forcé
                    var camelCaseOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = false
                    };

                    var camelCaseJson = JsonSerializer.Serialize(jsonDoc, camelCaseOptions);

                    _logger.LogDebug("Converted response to camelCase: {Path}", context.Request.Path);

                    // Écrire la réponse modifiée
                    var bytes = Encoding.UTF8.GetBytes(camelCaseJson);
                    context.Response.ContentLength = bytes.Length;
                    context.Response.Body = originalBodyStream;
                    await context.Response.Body.WriteAsync(bytes);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to convert response to camelCase");
                }
            }
        }

        // Si pas JSON ou erreur, copier la réponse originale
        responseBody.Seek(0, SeekOrigin.Begin);
        context.Response.Body = originalBodyStream;
        await responseBody.CopyToAsync(originalBodyStream);
    }
}
