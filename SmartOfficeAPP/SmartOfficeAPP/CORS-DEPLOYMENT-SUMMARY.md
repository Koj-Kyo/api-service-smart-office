# ? Résumé : Configuration CORS pour Static Web App

## ?? Changements appliqués

### 1?? Modification de `Program.cs`

**Ligne modifiée :** Configuration CORS `AllowFrontend`

**Avant :**
```csharp
opt.AddPolicy("AllowFrontend", p => p
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("https://localhost:7152", "http://localhost:5249", "http://localhost:3000", "https://localhost:3000")
    .AllowCredentials());
```

**Après :**
```csharp
opt.AddPolicy("AllowFrontend", p => p
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins(
        "https://localhost:7152", 
        "http://localhost:5249", 
        "http://localhost:3000", 
        "https://localhost:3000",
        "https://mango-bush-03e4a6703.3.azurestaticapps.net" // ? Ajouté
    )
    .AllowCredentials());
```

### 2?? Ajout wildcard dans `AllowMobileApp`

**Ligne ajoutée :**
```csharp
// ? Azure Static Web Apps
if (origin.Contains("azurestaticapps.net")) return true;
```

---

## ?? Prochaines étapes

### Étape 1 : Déployer sur Azure (Choisissez une méthode)

#### Option A : Script PowerShell (Le plus rapide - 2 minutes)

```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
.\deploy-cors-update.ps1
```

Le script va automatiquement :
- ? Build le projet
- ? Publier en Release
- ? Créer le package zip
- ? Se connecter à Azure
- ? Déployer sur App Service
- ? Redémarrer l'application
- ? Nettoyer les fichiers temporaires

#### Option B : Visual Studio (Interface graphique)

1. **Visual Studio** ? Clic droit sur `SmartOfficeAPP`
2. **"Publish..."**
3. Sélectionnez votre profil Azure
4. Cliquez sur **"Publish"**

#### Option C : Azure CLI manuel

```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP

# Build et publish
dotnet publish -c Release -o ./publish

# Créer le zip
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

# Déployer
az webapp deployment source config-zip `
  --resource-group smartoffice-free-rg `
  --name smartoffice-api-free `
  --src ./deploy.zip

# Redémarrer
az webapp restart `
  --resource-group smartoffice-free-rg `
  --name smartoffice-api-free
```

### Étape 2 : Attendre le redémarrage (1-2 minutes)

L'App Service redémarre automatiquement après le déploiement.

### Étape 3 : Tester depuis le frontend

Ouvrez votre Static Web App :
```
https://mango-bush-03e4a6703.3.azurestaticapps.net
```

**Console navigateur (F12) :**

```javascript
// Test health check
fetch('https://smartoffice-api-free-euhzgfb4fzd0ckbh.westeurope-01.azurewebsites.net/health/ready')
  .then(res => res.text())
  .then(data => console.log('? Backend accessible:', data))
  .catch(err => console.error('? CORS Error:', err));
```

**Résultat attendu :**
```
? Backend accessible: Healthy
```

---

## ?? Si ça ne fonctionne pas

### Problème 1 : Erreur CORS persiste

**Vérifications :**
1. Le backend a-t-il bien été redéployé ?
   ```powershell
   az webapp show --name smartoffice-api-free --resource-group smartoffice-free-rg --query state
   ```
   Résultat attendu : `"Running"`

2. Videz le cache du navigateur (Ctrl + Shift + Delete)

3. Attendez 2-3 minutes supplémentaires

### Problème 2 : "Application Error" dans le backend

**Solution :**
Vérifiez les logs :
```powershell
az webapp log tail --name smartoffice-api-free --resource-group smartoffice-free-rg
```

Ou dans Azure Portal :
- App Service ? **Monitoring** ? **Log stream**

### Problème 3 : Déploiement échoue

**Solution :**
Vérifiez que vous êtes connecté à Azure :
```powershell
az account show
```

Si pas connecté :
```powershell
az login
```

---

## ?? Checklist finale

Avant de valider la configuration :

- [ ] ? Code modifié dans `Program.cs`
- [ ] ? Build réussi en local
- [ ] ? Backend déployé sur Azure
- [ ] ? Application redémarrée (status = Running)
- [ ] ? Attente 2 minutes après redémarrage
- [ ] ? Test health check depuis frontend (F12 console)
- [ ] ? Pas d'erreur CORS dans la console
- [ ] ? Login API fonctionne depuis le frontend

---

## ?? Résultat

Votre frontend Static Web App peut maintenant communiquer avec le backend API !

**Architecture complète :**

```
????????????????????????????????????????????????????????
?  Frontend                                            ?
?  https://mango-bush-03e4a6703.3.azurestaticapps.net ?
?  (Azure Static Web App)                             ?
????????????????????????????????????????????????????????
                 ?
                 ? ? CORS autorisé
                 ? ? AllowCredentials: true
                 ?
                 ?
????????????????????????????????????????????????????????
?  Backend API                                         ?
?  https://smartoffice-api-free-euhzgfb4fzd0ckbh      ?
?  .westeurope-01.azurewebsites.net                   ?
?  (Azure App Service F1 Free)                        ?
????????????????????????????????????????????????????????
         ?               ?               ?
         ?               ?               ?
   ???????????    ????????????    ????????????
   ? IoT Hub ?    ? Storage  ?    ? Railway  ?
   ?         ?    ? Account  ?    ? PostgreSQL?
   ???????????    ????????????    ????????????
```

**Coût total : 0 USD** (tout est dans le plan gratuit) ??

Bonne chance pour votre soutenance ! ??
