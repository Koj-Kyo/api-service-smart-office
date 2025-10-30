namespace SmartOfficeAPP.DTOs.Response;

/// <summary>
/// Enveloppe de réponse unifiée avec métadonnées de cache/source
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Données de la réponse
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Métadonnées sur la source et la fraîcheur des données
    /// </summary>
    public ApiResponseMeta Meta { get; set; } = new();

    /// <summary>
    /// Crée une réponse depuis le cache
    /// </summary>
    public static ApiResponse<T> FromCache(T data, DateTime lastUpdated, bool isStale = false)
    {
        return new ApiResponse<T>
        {
Data = data,
   Meta = new ApiResponseMeta
            {
     Source = "cache",
     LastUpdatedUtc = lastUpdated,
              Stale = isStale,
                CachedAt = DateTime.UtcNow
    }
        };
    }

    /// <summary>
    /// Crée une réponse depuis la base de données
    /// </summary>
    public static ApiResponse<T> FromDatabase(T data, DateTime? lastUpdated = null)
    {
        return new ApiResponse<T>
        {
 Data = data,
            Meta = new ApiResponseMeta
       {
         Source = "database",
     LastUpdatedUtc = lastUpdated ?? DateTime.UtcNow,
  Stale = false
          }
   };
    }

    /// <summary>
    /// Crée une réponse depuis le cloud (IoT Hub)
    /// </summary>
    public static ApiResponse<T> FromCloud(T data)
    {
     return new ApiResponse<T>
        {
            Data = data,
     Meta = new ApiResponseMeta
         {
      Source = "cloud",
    LastUpdatedUtc = DateTime.UtcNow,
    Stale = false,
      FreshFromCloud = true
            }
        };
 }
}

/// <summary>
/// Métadonnées de la réponse API
/// </summary>
public class ApiResponseMeta
{
    /// <summary>
    /// Source des données: "cache", "database", "cloud"
    /// </summary>
    public string Source { get; set; } = "database";

    /// <summary>
    /// Date de dernière mise à jour des données
 /// </summary>
    public DateTime LastUpdatedUtc { get; set; }

    /// <summary>
    /// Indique si les données sont obsolètes (stale-while-revalidate)
  /// </summary>
    public bool Stale { get; set; }

    /// <summary>
    /// Date de mise en cache (si applicable)
    /// </summary>
  public DateTime? CachedAt { get; set; }

    /// <summary>
    /// Indique si les données viennent directement du cloud
    /// </summary>
    public bool FreshFromCloud { get; set; }

/// <summary>
    /// Durée de validité du cache (en secondes)
    /// </summary>
    public int? CacheTtlSeconds { get; set; }
}
