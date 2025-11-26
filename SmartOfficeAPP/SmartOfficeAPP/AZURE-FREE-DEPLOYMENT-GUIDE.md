# 🚀 Déploiement Backend SmartOffice sur Azure (Budget 20 USD)

## 📋 Vue d'ensemble

Ce guide vous permet de déployer votre backend sur Azure avec **ZÉRO coût supplémentaire** en utilisant :
- ✅ **Azure App Service F1 (Free)** - 0 USD/mois
- ✅ **Railway PostgreSQL** - 0 USD (500h gratuit/mois)
- ✅ **IoT Hub + Storage existants** - Déjà créés

**Budget total consommé : 0 USD** 🎉

---

## 🎯 Architecture de déploiement

```
┌────────────────────────────────────────────────────────┐
│                    AZURE CLOUD (FREE)                  │
├────────────────────────────────────────────────────────┤
│                                                        │
│  ┌─────────────────────────────────────────┐          │
│  │  App Service Plan F1 (FREE)             │          │
│  │  - Shared compute                       │          │
│  │  - 60 min/day compute time             │          │
│  │  - 1 GB RAM                             │          │
│  │  - 1 GB storage                         │          │
│  │  - FREE FOREVER                         │          │
│  └─────────────────────────────────────────┘          │
│         │                                              │
│         ▼                                              │
│  ┌─────────────────────────────────────────┐          │
│  │  SmartOffice Web API                    │          │
│  │  - .NET 9                               │          │
│  │  - HTTPS uniquement                     │          │
│  │  - Custom domain (optionnel)           │          │
│  └─────────────────────────────────────────┘          │
│                                                        │
└────────────────────────────────────────────────────────┘
         │                     │                │
         ▼                     ▼                ▼
┌──────────────┐    ┌──────────────┐   ┌──────────────┐
│  IoT Hub     │    │  Storage     │   │  Railway     │
│  (Existant)  │    │  (Existant)  │   │  PostgreSQL  │
│  ~25 USD     │    │  ~0.50 USD   │   │  FREE        │
└──────────────┘    └──────────────┘   └──────────────┘
```

---

## 📝 PARTIE 1 : Configuration Railway PostgreSQL

### Étape 1.1 : Créer un compte Railway

1. Allez sur **https://railway.app**
2. Cliquez sur **"Start a New Project"**
3. Connectez-vous avec **GitHub**
4. Confirmez votre email

### Étape 1.2 : Créer une base PostgreSQL

1. Cliquez sur **"+ New Project"**
2. Sélectionnez **"Provision PostgreSQL"**
3. Railway crée automatiquement une instance PostgreSQL

**Résultat :**
```
┌───────────────────────────────────────────────────────┐
│  PostgreSQL                                           │
├───────────────────────────────────────────────────────┤
│  Status: Running                                      │
│  Region: us-west1                                     │
│  Version: 16                                          │
│  Plan: Free (500h/month)                             │
└───────────────────────────────────────────────────────┘
```

### Étape 1.3 : Récupérer la connection string

1. Cliquez sur votre base PostgreSQL
2. Onglet **"Connect"**
3. Copiez la **"Postgres Connection URL"**

**Format :**
```
postgresql://postgres:PASSWORD@containers-us-west-XXX.railway.app:PORT/railway
```

### Étape 1.4 : Convertir au format .NET

Transformez la connection string Railway en format .NET :

**Railway :**
```
postgresql://postgres:PASSWORD@containers-us-west-XXX.railway.app:6543/railway
```

**Format .NET :**
```
Host=shortline.proxy.rlwy.net;Port=33437;Database=railway;Username=postgres;Password=vWbMuNcXvKhkPTIAZapCFPiLuexzRKgj;SSL Mode=Require
```

**⚠️ Notez cette connection string, vous en aurez besoin !**

---

## 📝 PARTIE 2 : Créer l'App Service sur Azure Portal

### Étape 2.1 : Se connecter à Azure Portal

1. Allez sur **https://portal.azure.com**
2. Connectez-vous avec votre compte Azure

### Étape 2.2 : Créer un Resource Group

1. Dans la barre de recherche, tapez : **"Resource groups"**
2. Cliquez sur **"+ Create"**
3. Remplissez :

```
Subscription     : [Votre abonnement]
Resource group   : smartoffice-free-rg
Region          : West Europe (ou East US)
```

4. Cliquez sur **"Review + create"**
5. Cliquez sur **"Create"**

### Étape 2.3 : Créer un App Service Plan (FREE)

1. Dans la barre de recherche, tapez : **"App Service plans"**
2. Cliquez sur **"+ Create"**
3. **Onglet Basics :**

```
┌───────────────────────────────────────────────────────┐
│  BASICS                                               │
├───────────────────────────────────────────────────────┤
│  Subscription        : [Votre abonnement]            │
│  Resource group      : smartoffice-free-rg           │
│  Name                : smartoffice-free-plan         │
│  Operating System    : Linux                          │
│  Region             : West Europe                     │
│                                                       │
│  Pricing tier        : F1 (Free)  ← IMPORTANT       │
│                       - 1 GB RAM                      │
│                       - 1 GB storage                  │
│                       - 60 min/day compute           │
│                       - FREE FOREVER                  │
└───────────────────────────────────────────────────────┘
```

4. Cliquez sur **"Pricing tier"**
5. Sélectionnez **"F1"** (dans la section **Dev / Test**)
6. Cliquez sur **"Apply"**
7. Cliquez sur **"Review + create"**
8. Cliquez sur **"Create"**

⏱️ **Durée : ~1 minute**

### Étape 2.4 : Créer l'App Service

1. Dans la barre de recherche, tapez : **"App Services"**
2. Cliquez sur **"+ Create"**
3. **Onglet Basics :**

```
┌───────────────────────────────────────────────────────┐
│  BASICS                                               │
├───────────────────────────────────────────────────────┤
│  Subscription        : [Votre abonnement]            │
│  Resource group      : smartoffice-free-rg           │
│  Name                : smartoffice-api-free          │
│                       (doit être unique globalement)  │
│  Publish             : Code                           │
│  Runtime stack       : .NET 9 (STS)                  │
│  Operating System    : Linux                          │
│  Region             : West Europe                     │
│                                                       │
│  Pricing plan        : smartoffice-free-plan (F1)    │
│                       FREE - 0 USD/month              │
└───────────────────────────────────────────────────────┘
```

4. Cliquez sur **"Next: Deployment >"**
5. **Déploiement continu :** Désactivé (on déploiera manuellement)
6. Cliquez sur **"Next: Networking >"**
7. **Public access :** Enabled (par défaut)
8. Cliquez sur **"Review + create"**
9. Cliquez sur **"Create"**

⏱️ **Durée : ~2-3 minutes**

### Étape 2.5 : Vérifier la création

Une fois le déploiement terminé :

1. Cliquez sur **"Go to resource"**
2. Vous verrez votre App Service :

```
┌───────────────────────────────────────────────────────┐
│  smartoffice-api-free                                 │
├───────────────────────────────────────────────────────┤
│  Status: Running                                      │
│  URL: https://smartoffice-api-free.azurewebsites.net │
│  Pricing tier: F1 (FREE)                             │
└───────────────────────────────────────────────────────┘
```

3. **Notez l'URL** de votre application

---

## 📝 PARTIE 3 : Configuration des Variables d'Environnement

### Étape 3.1 : Accéder aux Configuration Settings

1. Dans votre App Service (**smartoffice-api-free**)
2. Menu de gauche : **"Settings"** → **"Environment variables"**
3. Vous verrez la section **"App settings"**

### Étape 3.2 : Ajouter les variables d'environnement

Cliquez sur **"+ Add"** pour chaque variable :

#### 1️⃣ PostgreSQL (Railway)

```
Name  : ConnectionStrings__DefaultConnection
Value : Host=containers-us-west-XXX.railway.app;Port=6543;Database=railway;Username=postgres;Password=VOTRE_PASSWORD;SSL Mode=Require
```

#### 2️⃣ JWT Settings

```
Name  : JwtSettings__Secret
Value : ab01f7afaa995ff77b2947ffe9328f5632ae16aefcd0b2357ad7ad34266aeba01b9461872b7642fb35854a7a0be8aedc057ce762f48b852998c5c51edc7591c1
```

```
Name  : JwtSettings__Issuer
Value : SmartOfficeAPI
```

```
Name  : JwtSettings__Audience
Value : SmartOfficeClient
```

#### 3️⃣ IoT Hub

```
Name  : IotHubConnectionString
Value : HostName=brainoffice.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=VOTRE_CLE
```

```
Name  : IotHubEventHub__ConnectionString
Value : Endpoint=sb://iothub-ns-brainoffic-xxxxx.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=VOTRE_CLE;EntityPath=brainoffice
```

```
Name  : IotHubEventHub__Name
Value : brainoffice
```

```
Name  : IotHubEventHub__Enabled
Value : true
```

```
Name  : IotHubEventHub__LogsEnabled
Value : true
```

#### 4️⃣ Storage Account

```
Name  : IotHubEventHub__BlobConnectionString
Value : DefaultEndpointsProtocol=https;AccountName=smartofficecheckpoints;AccountKey=VOTRE_CLE;EndpointSuffix=core.windows.net
```

```
Name  : IotHubEventHub__BlobContainer
Value : iothub-checkpoints
```

```
Name  : IotHubEventHub__BlobContainerLogs
Value : iothub-logs-checkpoints
```

#### 5️⃣ Device Sync

```
Name  : DeviceSync__IntervalMinutes
Value : 5
```

```
Name  : DeviceSync__Enabled
Value : true
```

```
Name  : DeviceSync__MaxParallelDevices
Value : 5
```

#### 6️⃣ ASP.NET Core

```
Name  : ASPNETCORE_ENVIRONMENT
Value : Production
```

```
Name  : ASPNETCORE_URLS
Value : http://+:8080
```

#### 7️⃣ AutoMapper (optionnel)

```
Name  : AutoMapper__LicenseKey
Value : eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ...
```

### Étape 3.3 : Sauvegarder

1. Cliquez sur **"Apply"** en bas
2. Cliquez sur **"Confirm"** pour redémarrer l'app

⚠️ **L'application va redémarrer automatiquement** (~30 secondes)

---

## 📝 PARTIE 4 : Déploiement du Code

### Méthode 1 : Via Visual Studio (Recommandé)

#### Étape 4.1 : Préparer le projet

```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP

# Nettoyer et build
dotnet clean
dotnet build --configuration Release
```

#### Étape 4.2 : Publier depuis Visual Studio

1. **Ouvrez Visual Studio**
2. Cliquez-droit sur le projet **SmartOfficeAPP**
3. Sélectionnez **"Publish..."**
4. Cliquez sur **"Azure"**
5. Cliquez sur **"Next"**
6. Sélectionnez **"Azure App Service (Linux)"**
7. Cliquez sur **"Next"**
8. **Connectez-vous à Azure** si nécessaire
9. Sélectionnez votre **subscription**
10. Sélectionnez **smartoffice-api-free**
11. Cliquez sur **"Finish"**
12. Cliquez sur **"Publish"**

⏱️ **Durée : ~5-10 minutes**

**Logs attendus :**
```
Building...
Publishing...
Uploading files...
Deployment successful
URL: https://smartoffice-api-free.azurewebsites.net
```

### Méthode 2 : Via Azure CLI (Alternative)

```powershell
# Se connecter à Azure
az login

# Publier l'application
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP

# Créer le package de déploiement
dotnet publish -c Release -o ./publish

# Créer un fichier zip
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

# Déployer sur Azure
az webapp deployment source config-zip `
  --resource-group smartoffice-free-rg `
  --name smartoffice-api-free `
  --src ./deploy.zip

# Redémarrer l'app
az webapp restart `
  --resource-group smartoffice-free-rg `
  --name smartoffice-api-free
```

### Méthode 3 : Via GitHub Actions (Automatisé)

Si vous voulez un déploiement automatique à chaque push, consultez le fichier `.github/workflows/deploy-azure-minimal.yml` que j'ai créé précédemment.

---

## 📝 PARTIE 5 : Exécuter les migrations EF Core

### Étape 5.1 : Préparer la connection string Railway

Récupérez votre connection string Railway :

```
Host=containers-us-west-XXX.railway.app;Port=6543;Database=railway;Username=postgres;Password=VOTRE_PASSWORD;SSL Mode=Require
```

### Étape 5.2 : Exécuter les migrations

```powershell
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP

# Définir la connection string temporairement
$env:ConnectionStrings__DefaultConnection="Host=containers-us-west-XXX.railway.app;Port=6543;Database=railway;Username=postgres;Password=VOTRE_PASSWORD;SSL Mode=Require"

# Exécuter les migrations
dotnet ef database update

# Vérifier les migrations
dotnet ef migrations list
```

**Résultat attendu :**
```
Applying migration '20250918004708_initial'.
Applying migration '20251012020550_initialCreate'.
Applying migration '20251012024609_AddIncidentTicketNewFields'.
Applying migration '20251012181128_AddIndex_TicketsIncidents_Statut'.
Done.
```

✅ **Base de données Railway créée et migrée !**

---

## 📝 PARTIE 6 : Vérification du Déploiement

### Étape 6.1 : Tester le Health Check

Ouvrez votre navigateur :

```
https://smartoffice-api-free.azurewebsites.net/health/ready
```

**Résultat attendu :**
```
Healthy
```

### Étape 6.2 : Tester Swagger

```
https://smartoffice-api-free.azurewebsites.net/swagger
```

Vous devriez voir l'interface Swagger avec tous vos endpoints.

### Étape 6.3 : Tester l'authentification

```bash
curl -X POST https://smartoffice-api-free.azurewebsites.net/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "adminer@smartoffice.com",
    "password": "Admin1234!"
  }'
```

**Résultat attendu :**
```json
{
  "token": "eyJhbGc...",
  "email": "adminer@smartoffice.com",
  "userName": "admin"
}
```

### Étape 6.4 : Vérifier les logs

1. Dans Azure Portal, allez dans votre **App Service**
2. Menu : **"Monitoring"** → **"Log stream"**
3. Vous verrez les logs en temps réel :

```
info: SmartOfficeAPP.Services.ServicesImpl.IotHubService[0]
      ✅ Service IoT Hub initialisé avec succès
      
info: SmartOfficeAPP.Services.Background.TelemetrieIngestionWorker[0]
      ✅ Démarrage de l'ingestion télémétrie Event Hub...
```

---

## 📝 PARTIE 7 : Configuration de l'Application Mobile

### Mettre à jour l'URL dans l'app MAUI

Dans votre application mobile, mettez à jour l'URL de l'API :

```csharp
// Dans ApiService.cs ou MauiProgram.cs
private const string BaseUrl = "https://smartoffice-api-free.azurewebsites.net";
```

---

## 💰 Récapitulatif des Coûts

| Ressource | Plan | Coût mensuel | Inclus |
|-----------|------|--------------|--------|
| **App Service** | F1 (Free) | **0 USD** | 60 min/jour, 1GB RAM |
| **Railway PostgreSQL** | Free | **0 USD** | 500h/mois, 1GB storage |
| **IoT Hub** | Standard | **~25 USD** | Déjà créé |
| **Storage Account** | Standard | **~0.50 USD** | Déjà créé |
| **TOTAL NOUVEAU** | | **0 USD** | ✅ |
| **TOTAL GLOBAL** | | **~25.50 USD** | IoT Hub + Storage |

### Budget utilisé sur vos 20 USD

```
20 USD disponible
- 0 USD (App Service Free)
- 0 USD (Railway PostgreSQL)
────────────────────────────
= 20 USD restant
```

✅ **Vous n'avez RIEN consommé de vos 20 USD !**

---

## ⚠️ Limitations du Plan Free (F1)

### Ce qui est limité

| Limitation | Valeur | Impact |
|------------|--------|--------|
| **Compute time** | 60 min/jour | OK pour démo/soutenance |
| **Always On** | Désactivé | Cold start (~30s) |
| **Custom domain** | Non supporté | URL Azure par défaut |
| **Scale out** | 1 instance fixe | Pas de scaling |
| **Backup** | Non disponible | Pas de backup automatique |

### Ce qui fonctionne parfaitement

- ✅ HTTPS automatique (certificate SSL gratuit)
- ✅ Toutes les fonctionnalités de l'API
- ✅ IoT Hub, télémétrie, commandes
- ✅ SignalR en temps réel
- ✅ Connexion mobile MAUI
- ✅ Swagger UI
- ✅ Logs et monitoring

---

## 🔧 Maintenance et Optimisation

### Activer les logs d'application

1. App Service → **"Monitoring"** → **"App Service logs"**
2. Activez :
   - **Application Logging (Filesystem)** : On
   - **Level** : Information
   - **Retention (Days)** : 3
3. Cliquez sur **"Save"**

### Redémarrer l'application

```powershell
az webapp restart \
  --name smartoffice-api-free \
  --resource-group smartoffice-free-rg
```

Ou dans le portail :
- App Service → **"Overview"** → **"Restart"**

### Arrêter l'application (économiser compute time)

Si vous ne l'utilisez pas :

```powershell
az webapp stop \
  --name smartoffice-api-free \
  --resource-group smartoffice-free-rg
```

### Redémarrer

```powershell
az webapp start \
  --name smartoffice-api-free \
  --resource-group smartoffice-free-rg
```

---

## 🚨 Dépannage

### Erreur : "Application Error"

**Cause** : Variables d'environnement manquantes ou erreur de connexion BDD

**Solution** :
1. Vérifiez les **Environment variables** dans l'App Service
2. Testez la connection string PostgreSQL Railway
3. Vérifiez les logs : **Log stream**

### Erreur : "Database connection failed"

**Cause** : Connection string Railway incorrecte

**Solution** :
1. Allez sur Railway → Votre projet PostgreSQL → **Connect**
2. Re-copiez la connection string
3. Convertissez au format .NET
4. Mettez à jour dans Azure App Service

### Erreur : "Cold start timeout"

**Cause** : Le plan F1 a un cold start de ~30 secondes

**Solution** :
- C'est normal sur le plan gratuit
- Attendez 30 secondes après la première requête
- L'application sera ensuite réactive

---

## 📊 Checklist finale

Avant de valider le déploiement :

- [ ] ✅ App Service créé (F1 Free)
- [ ] ✅ PostgreSQL Railway créé et migré
- [ ] ✅ Variables d'environnement configurées (23 variables)
- [ ] ✅ Code déployé sur Azure
- [ ] ✅ Health check `/health/ready` retourne "Healthy"
- [ ] ✅ Swagger UI accessible
- [ ] ✅ Login API fonctionne
- [ ] ✅ IoT Hub connecté (logs backend)
- [ ] ✅ Télémétrie reçue (Event Hub)
- [ ] ✅ Application mobile connectée
- [ ] ✅ Budget consommé : 0 USD (sur 20 USD disponible)

---

## 📚 Ressources

- **Azure App Service Free Tier** : https://azure.microsoft.com/pricing/details/app-service/
- **Railway PostgreSQL** : https://railway.app/
- **Azure Portal** : https://portal.azure.com

---

**Déploiement terminé sans consommer de budget ! 🎉**

Votre API est maintenant accessible sur :
```
https://smartoffice-api-free.azurewebsites.net
```

Bonne chance pour votre soutenance ! 🚀
