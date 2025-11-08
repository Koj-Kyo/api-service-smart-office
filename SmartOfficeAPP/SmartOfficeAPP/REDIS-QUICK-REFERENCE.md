# Redis Configuration - Quick Reference

## ? Current Status
Your SmartOffice API is now configured to work **WITHOUT Redis** by default.

## ?? What Was Fixed

### Problem
```
? RedisConnectionException: Could not connect to localhost:6389
? InvalidOperationException: Unable to resolve IDistributedCache
```

### Solution
```csharp
? Added: builder.Services.AddDistributedMemoryCache();
? Disabled Redis in appsettings.json (commented out)
? Application uses in-memory distributed cache as fallback
```

## ?? Quick Start

### Run the Application (No Setup Required)
```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
dotnet run
```

**Expected Log:**
```
warn: Redis non configuré, utilisation du cache mémoire distribué.
```

## ?? Configuration Files

### `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    // Redis disabled for development - using in-memory cache
    // "Redis": "localhost:6379"
  },
  // Redis disabled for development - using in-memory cache
  // "Redis": {
  //   "ConnectionString": "localhost:6379",
  //   "InstanceName": "SmartOfficeAPI"
  // }
}
```

### `Program.cs` (lines 27-88)
```csharp
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    // Use Redis
    builder.Services.AddStackExchangeRedisCache(options => { ... });
}
else
{
    // ? Use in-memory distributed cache (NEW)
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddMemoryCache();
}
```

## ?? Enable Redis Later

### 1. Install Redis
**Using Docker (Recommended):**
```powershell
docker run -d -p 6379:6379 --name redis redis:latest
```

**Using Chocolatey:**
```powershell
choco install redis-64
redis-server
```

### 2. Update Configuration
**Uncomment in `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "SmartOfficeAPI"
  }
}
```

### 3. Restart Application
```powershell
dotnet run
```

**Expected Log:**
```
info: Redis configuré avec OutputCache pour : localhost:6379
```

## ?? Test Caching

### Test Endpoint with Cache
```http
### Get Telemetry Data (cached for 30 seconds)
GET https://localhost:5001/api/telemetries/history
    ?startDate=2025-01-05
    &endDate=2025-01-06
Authorization: Bearer {{token}}
```

**Behavior:**
- **First call:** Queries database (slower)
- **Subsequent calls (within 30s):** Returns from cache (faster)

### Bypass Cache
```http
### Force fresh data
GET https://localhost:5001/api/telemetries/history
    ?startDate=2025-01-05
    &endDate=2025-01-06
    &fresh=true
Authorization: Bearer {{token}}
```

## ?? Cache Policies

| Endpoint | Policy | Duration | Tags |
|----------|--------|----------|------|
| General API | `DefaultPolicy` | 3 min | `api-data` |
| Device Data | `DeviceData` | 1 min | `device-data` |
| Telemetry | `TelemetryData` | 30 sec | `telemetry-data` |

## ?? Verify Cache is Working

### Check Application Logs
```
info: Microsoft.AspNetCore.OutputCaching.OutputCacheMiddleware[0]
      Serving from cache
```

### Check Response Headers
```http
HTTP/1.1 200 OK
age: 15
x-cache: HIT
```

## ?? Understanding the Fix

### Before (Broken)
```
Application ? ? Redis (not available)
Services ? ? IDistributedCache (not registered)
Result ? ?? Application crash
```

### After (Fixed)
```
Application ? ? In-Memory Cache (always available)
Services ? ? IDistributedCache (registered)
Result ? ? Application works
```

## ??? Architecture

```
???????????????????????????????????????????????
?         SmartOffice API           ?
???????????????????????????????????????????????
?  Services (CommandeService, etc.)?
?   ? inject?
?  IDistributedCache (interface)     ?
?    ? implements        ?
?  ??????????????????????????????????????    ?
?  ? Redis Available ? Redis Unavailable ?    ?
?  ??????????????????????????????????????    ?
?  ? Redis Cache   ? Memory Cache     ?    ?
?? (Production)    ? (Development)    ?    ?
?  ??????????????????????????????????????    ?
???????????????????????????????????????????????
```

## ?? Important Notes

### In-Memory Cache Limitations
- ? Not shared between multiple instances
- ? Lost when application restarts
- ? Perfect for local development
- ? Zero configuration required

### When to Use Redis
- ? Production environments
- ? Multiple API instances (load balanced)
- ? Need persistent cache across restarts
- ? Distributed systems

## ??? Troubleshooting

### Issue: Application won't start
**Check:**
```powershell
# Verify configuration
cat appsettings.json | findstr Redis

# Should show commented lines
```

### Issue: Cache not working
**Check logs:**
```
# Look for this warning
warn: Redis non configuré, utilisation du cache mémoire distribué
```

### Issue: Want to use Redis but getting errors
**Verify Redis is running:**
```powershell
# Test Redis connection
docker exec -it redis redis-cli ping
# Should return: PONG
```

## ?? Related Files
- `SmartOfficeAPP/Program.cs` - Cache configuration
- `SmartOfficeAPP/appsettings.json` - Connection strings
- `SmartOfficeAPP/REDIS-FIX-SUMMARY.md` - Detailed fix documentation

---

**Status:** ? Working  
**Cache Mode:** In-Memory (Development)  
**Redis Required:** ? No (Optional for production)
