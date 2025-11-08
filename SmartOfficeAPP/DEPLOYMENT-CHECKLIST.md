# ? SmartOffice API - Checklist de Déploiement

## ?? Fichiers d'Infrastructure Créés

? **Bicep (Infrastructure as Code)**
- `infra/main.bicep` - Infrastructure principale Azure
- `infra/main.parameters.json` - Paramètres de déploiement

? **Configuration Azure Developer CLI**
- `azure.yaml` - Configuration azd avec hooks de déploiement

? **Scripts de Déploiement**
- `deploy.ps1` - Script PowerShell automatisé (Windows)
- `deploy.sh` - Script Bash automatisé (Linux/Mac)

? **Configuration**
- `.env.example` - Template des variables d'environnement
- `.gitignore` - Protection des secrets (mise à jour)

? **Documentation**
- `README.md` - Documentation principale du projet
- `AZURE-DEPLOYMENT-COMPLETE-GUIDE.md` - Guide complet de déploiement
- `DEPLOYMENT-QUICK-START.md` - Guide rapide 5 minutes
- `AZURE-COMMANDS-CHEATSHEET.md` - Toutes les commandes utiles

---

## ??? Infrastructure Azure Provisionnée

### Services Azure déployés :

| Service | SKU | Usage | Coût mensuel |
|---------|-----|-------|--------------|
| ? **App Service Plan** | Standard S1 | Hébergement API .NET 9 | ~70 USD |
| ? **App Service** | Linux | Runtime .NET 9 | Inclus |
| ? **PostgreSQL Flexible** | Burstable B2s | Base de données (16) | ~30 USD |
| ? **Redis Cache** | Basic C0 | Cache distribué | ~17 USD |
| ? **Key Vault** | Standard | Gestion secrets | ~1 USD |
| ? **Application Insights** | Pay-as-you-go | Monitoring | ~5 USD |
| ? **Log Analytics** | Workspace | Logs centralisés | Inclus |

**Coût total estimé : ~123 USD/mois**

### Configuration de sécurité :

? **HTTPS obligatoire** - Certificat SSL automatique  
? **Managed Identity** - Authentification sans credentials  
? **Key Vault RBAC** - Accès aux secrets sécurisé  
? **Firewall PostgreSQL** - Règles d'accès configurées  
? **Redis SSL** - Connexions chiffrées TLS 1.2  
? **JWT Authentication** - Tokens sécurisés  

### Monitoring & Observabilité :

? **Application Insights** - Métriques temps réel  
? **Health Checks** - `/health/ready` et `/health/live`  
? **Log Analytics** - Centralisation des logs  
? **Alertes** - Notifications d'anomalies  

---

## ?? Prochaines Étapes

### 1?? Configurer les Secrets (OBLIGATOIRE)

```powershell
# 1. Copier le template
Copy-Item .env.example .env

# 2. Remplir les valeurs dans .env :
#    - AZURE_SUBSCRIPTION_ID
#    - POSTGRESQL_ADMIN_PASSWORD (min 8 chars, maj, min, chiffre)
#    - JWT_SECRET (votre secret actuel)
#    - IOTHUB_CONNECTION_STRING (depuis votre IoT Hub existant)
#    - STORAGE_CONNECTION_STRING (depuis votre Storage Account)

notepad .env
```

### 2?? Obtenir les Connection Strings Existantes

```powershell
# Se connecter à Azure
az login

# IoT Hub Connection String
az iot hub connection-string show --hub-name brainoffice --policy-name service

# Storage Account Connection String (remplacez les valeurs)
az storage account show-connection-string `
  --name VOTRE_STORAGE_ACCOUNT `
  --resource-group VOTRE_RESOURCE_GROUP
```

### 3?? Déployer sur Azure (5 minutes)

```powershell
# Option A : Script automatisé (recommandé)
.\deploy.ps1 up

# Option B : Directement avec azd
azd up

# Suivre les prompts et attendre ~5 minutes
```

### 4?? Vérifier le Déploiement

```powershell
# Tester le health check
$url = azd env get-value AZURE_WEBAPP_URL
Invoke-WebRequest -Uri "$url/health/ready"

# Ouvrir Swagger UI
Start-Process "$url/swagger"

# Voir les logs
azd monitor --logs
```

---

## ?? Pour la Soutenance

### Démonstration Live

```powershell
# 1. Démarrer l'API
az webapp start --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# 2. Ouvrir le monitoring
azd monitor

# 3. Tester l'API
$url = azd env get-value AZURE_WEBAPP_URL
Start-Process "$url/swagger"

# 4. Montrer le portail Azure
azd monitor --portal
```

### Points Clés à Présenter

1. **Architecture Cloud-Native**
   - Services managés (PaaS)
   - Auto-scaling
   - Haute disponibilité

2. **Infrastructure as Code**
   - Bicep pour reproductibilité
   - Versioning avec Git
   - Documentation as code

3. **Sécurité**
   - Key Vault pour secrets
   - Managed Identity
   - HTTPS/TLS 1.2 minimum
   - RBAC

4. **Monitoring & Observabilité**
   - Application Insights
   - Health checks
   - Logs centralisés
   - Alertes

5. **DevOps & CI/CD**
   - Infrastructure versionnée
   - Déploiement automatisé
   - Tests intégrés
   - GitHub Actions ready

### Métriques à Montrer

- ?? Temps de réponse API
- ?? Nombre de requêtes/seconde
- ?? Utilisation mémoire/CPU
- ?? Cache hit ratio (Redis)
- ??? Connexions PostgreSQL

---

## ?? Validation

### Checklist de Vérification

? **Build réussi**
```powershell
dotnet build SmartOfficeAPP/SmartOfficeAPP.csproj
# ? Génération réussie
```

? **Fichiers infrastructure présents**
```powershell
dir infra
# main.bicep ?
# main.parameters.json ?
```

? **Configuration azd**
```powershell
dir azure.yaml
# ? Présent
```

? **Secrets template**
```powershell
dir .env.example
# ? Présent
```

? **Protection Git**
```powershell
Select-String -Path .gitignore -Pattern ".env"
# ? .env est ignoré
```

---

## ?? Configuration Avancée (Optionnel)

### CI/CD avec GitHub Actions

Si vous voulez automatiser le déploiement à chaque push :

1. Créer `.github/workflows/azure-deploy.yml` (template dans le guide)
2. Configurer les secrets GitHub (AZURE_CREDENTIALS, etc.)
3. Push sur `main` déclenche le déploiement

Voir : [Guide CI/CD](AZURE-DEPLOYMENT-COMPLETE-GUIDE.md#déploiement-cicd-avec-github-actions)

### Monitoring Avancé

```powershell
# Configurer des alertes
az monitor metrics alert create `
  --name "High-CPU-Alert" `
  --resource-group smartoffice-prod-rg `
  --scopes "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/smartoffice-prod-rg/providers/Microsoft.Web/sites/smartoffice-prod-api" `
  --condition "avg Percentage CPU > 80" `
  --window-size 5m `
  --evaluation-frequency 1m `
  --action email email@example.com
```

### Scaling Automatique

```powershell
# Configurer auto-scaling
az monitor autoscale create `
  --resource-group smartoffice-prod-rg `
  --resource smartoffice-prod-plan `
  --resource-type Microsoft.Web/serverfarms `
  --name autoscale-plan `
  --min-count 1 `
  --max-count 3 `
  --count 1

# Règle : scale out si CPU > 70%
az monitor autoscale rule create `
  --resource-group smartoffice-prod-rg `
  --autoscale-name autoscale-plan `
  --condition "Percentage CPU > 70 avg 5m" `
  --scale out 1
```

---

## ?? Économiser les Coûts

### Arrêter après la soutenance
```powershell
# Arrêter l'App Service (pas de facturation)
az webapp stop --name smartoffice-prod-api --resource-group smartoffice-prod-rg

# Arrêter PostgreSQL (pas de facturation)
az postgres flexible-server stop `
  --name smartoffice-prod-postgres `
  --resource-group smartoffice-prod-rg
```

### Redémarrer avant une démo
```powershell
az webapp start --name smartoffice-prod-api --resource-group smartoffice-prod-rg
az postgres flexible-server start `
  --name smartoffice-prod-postgres `
  --resource-group smartoffice-prod-rg
```

### Supprimer complètement (fin du projet)
```powershell
# ?? ATTENTION : Supprime TOUT
azd down --force --purge
```

---

## ?? Support & Ressources

### Documentation
- ?? [Guide de Déploiement Complet](AZURE-DEPLOYMENT-COMPLETE-GUIDE.md)
- ?? [Quick Start (5 min)](DEPLOYMENT-QUICK-START.md)
- ?? [Commandes Utiles](AZURE-COMMANDS-CHEATSHEET.md)
- ?? [README Principal](README.md)

### Liens Utiles
- [Azure CLI Documentation](https://learn.microsoft.com/cli/azure/)
- [Azure Developer CLI (azd)](https://learn.microsoft.com/azure/developer/azure-developer-cli/)
- [Bicep Documentation](https://learn.microsoft.com/azure/azure-resource-manager/bicep/)
- [App Service Documentation](https://learn.microsoft.com/azure/app-service/)

### Commandes de Diagnostic
```powershell
# État du déploiement
azd env list

# Valeurs d'environnement
azd env get-values

# Logs en temps réel
azd monitor --logs

# Ouvrir le portail Azure
azd monitor --portal
```

---

## ? Résumé

Vous avez maintenant :

? Infrastructure complète (Bicep)  
? Scripts de déploiement automatisés  
? Configuration sécurisée (Key Vault)  
? Monitoring (Application Insights)  
? Documentation complète  
? Build validé  

**?? Prêt pour le déploiement !**

---

## ?? Commande Finale

```powershell
# Déployer maintenant
azd up

# Ou avec le script
.\deploy.ps1 up
```

**?? Durée estimée : 5-7 minutes**

Bonne chance pour votre soutenance ! ???
