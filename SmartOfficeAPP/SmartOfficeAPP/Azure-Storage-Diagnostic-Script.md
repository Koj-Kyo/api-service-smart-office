# ?? Script de Diagnostic Azure Event Hub & Blob Storage

## 1. Vérification IoT Hub et Consumer Groups

```bash
# Variables à adapter
RESOURCE_GROUP="votre-resource-group"
IOT_HUB_NAME="brainoffice"
STORAGE_ACCOUNT="smartofficecheckpoints"

# Lister les consumer groups existants
echo "=== Consumer Groups IoT Hub ==="
az iot hub consumer-group list \
  --hub-name $IOT_HUB_NAME \
  --resource-group $RESOURCE_GROUP

# Créer le consumer group "logs" si manquant
echo "=== Création Consumer Group 'logs' ==="
az iot hub consumer-group create \
  --name logs \
  --hub-name $IOT_HUB_NAME \
  --resource-group $RESOURCE_GROUP
```

## 2. Vérification Compte de Stockage

```bash
# Vérifier l'existence du compte de stockage
echo "=== Vérification Compte de Stockage ==="
az storage account show \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP

# Lister les conteneurs existants
echo "=== Conteneurs Existants ==="
az storage container list \
  --account-name $STORAGE_ACCOUNT \
  --auth-mode login

# Créer les conteneurs manquants
echo "=== Création Conteneurs ==="
az storage container create \
  --name "iothub-checkpoints" \
  --account-name $STORAGE_ACCOUNT \
  --auth-mode login

az storage container create \
  --name "iothub-logs-checkpoints" \
  --account-name $STORAGE_ACCOUNT \
  --auth-mode login
```

## 3. Vérification Permissions

```bash
# Vérifier les permissions du compte de stockage
echo "=== Permissions Compte de Stockage ==="
az role assignment list \
  --scope "/subscriptions/$(az account show --query id -o tsv)/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.Storage/storageAccounts/$STORAGE_ACCOUNT"
```

## 4. Alternative: Création via Azure Portal

Si les commandes CLI échouent :

1. **Azure Portal** ? **Comptes de stockage** ? **smartofficecheckpoints**
2. **Conteneurs** ? **+ Conteneur**
3. Créer :
   - Nom: `iothub-checkpoints`
   - Niveau d'accès public: **Privé**
4. Répéter pour `iothub-logs-checkpoints`

## 5. Création Consumer Group via Portal

1. **Azure Portal** ? **IoT Hub** ? **brainoffice**
2. **Built-in endpoints** ? **Events**
3. **Consumer groups** ? **+ Add consumer group**
4. Nom: `logs`
5. **Save**

## ?? Actions Immédiates

Exécutez d'abord ces commandes pour identifier le problème :

```bash
# 1. Vérifier si le compte de stockage existe
az storage account show --name smartofficecheckpoints --query "name"

# 2. Vérifier les conteneurs
az storage container list --account-name smartofficecheckpoints --auth-mode login

# 3. Vérifier les consumer groups
az iot hub consumer-group list --hub-name brainoffice
```