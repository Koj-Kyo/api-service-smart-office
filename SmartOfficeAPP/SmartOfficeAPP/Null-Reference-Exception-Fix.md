# ?? Guide de Diagnostic - Exception Null Reference

## Corrections appliquées dans IoTHubService

### ? Validations ajoutées :

1. **Validation des paramètres d'entrée**
   - `deviceId` ne peut plus être null ou vide
   - `methodName` ne peut plus être null ou vide
   - `message` ne peut plus être null ou vide
   - `desiredJson` ne peut plus être null ou vide
   - `deviceIds` collection ne peut plus être null

2. **Validation des retours Azure**
   - Vérification que `result` n'est pas null après appel IoT Hub
   - Vérification que `twin` n'est pas null après GetTwinAsync
   - Vérification que `device` n'est pas null après GetDeviceAsync
   - Vérification que `existingDevice` n'est pas null

3. **Protection des propriétés nullables**
   - `twin.Properties?.Reported?.ToJson() ?? "{}"`
   - `twin.Properties?.Desired?.ToJson() ?? "{}"`
   - `twin?.Version ?? 0`

4. **Gestion d'exceptions spécifiques**
   - `ArgumentNullException` capturée séparément
   - Logs spécifiques pour chaque type d'erreur

## ?? Comment identifier votre exception précise

### 1. Activez les points d'arrêt
```csharp
// Dans IoTHubService.cs, ajoutez des points d'arrêt sur :
// - Ligne où l'exception se produit
// - Début de chaque méthode publique
// - Lignes avec appels await vers Azure
```

### 2. Vérifiez les logs
Regardez dans la fenêtre Output de Visual Studio pour voir :
```
[ERROR] DeviceId ne peut pas être null ou vide
[ERROR] Argument null lors de l'invocation de la méthode...
[ERROR] Twin null reçu pour le device...
```

### 3. Testez avec des appels spécifiques

**Test 1 : Méthode directe**
```http
POST {{baseUrl}}/api/iothub/device/test-device/method/getStatus
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "payload": "{}"
}
```

**Test 2 : Device Twin**
```http
GET {{baseUrl}}/api/iothub/device/test-device/twin
Authorization: Bearer {{token}}
```

**Test 3 : Création device**
```http
POST {{baseUrl}}/api/iothub/device/test-device
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "enabled": true
}
```

### 4. Vérifiez la configuration
```bash
# Vérifiez que votre chaîne IoT Hub est bien configurée
dotnet user-secrets list | findstr IotHub
```

## ?? Points de vérification si exception persiste

### A. Dans le constructeur
```csharp
// Vérifiez que ces objets ne sont pas null :
// - _serviceClient
// - _registryManager
// - _logger
// - _signalRService
```

### B. Dans les appels de service
```csharp
// Vérifiez les paramètres reçus depuis les contrôleurs :
// - CommandeService.SendCommandAsync
// - IoTHubController actions
// - DeviceController actions
```

### C. Configuration Azure
```csharp
// Vérifiez que la chaîne de connexion est valide :
// HostName=votre-hub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=...
```

## ?? Actions selon le type d'exception

### Si NullReferenceException sur _serviceClient
- ? Problème d'initialisation dans le constructeur
- ? Vérifiez la chaîne de connexion IoT Hub

### Si NullReferenceException sur result.Status
- ? Azure IoT Hub retourne null
- ? Maintenant géré avec vérification `if (result == null)`

### Si NullReferenceException sur twin.Properties
- ? Device Twin malformé
- ? Maintenant géré avec `twin.Properties?.Reported?.ToJson()`

### Si ArgumentNullException
- ? Paramètre null passé depuis contrôleur
- ? Maintenant validé en début de méthode

## ?? Prochaines étapes

1. **Redémarrez l'application** pour appliquer les corrections
2. **Reproduisez l'exception** avec les mêmes étapes
3. **Regardez les nouveaux logs** pour identifier la cause précise
4. **Testez avec les fichiers HTTP** fournis

Si l'exception persiste, vous devriez maintenant voir des logs spécifiques comme :
```
[ERROR] DeviceId ne peut pas être null ou vide
[ERROR] Twin null reçu pour le device test-device
[ERROR] Argument null lors de l'invocation de la méthode getStatus sur test-device
```

Ces logs vous indiqueront exactement où et pourquoi l'exception se produit.

---

**?? Astuce :** Utilisez Hot Reload (Ctrl+Alt+F5) pour appliquer les corrections sans redémarrer complètement l'application.