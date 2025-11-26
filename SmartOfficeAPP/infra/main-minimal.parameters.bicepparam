# ?? Paramètres de déploiement minimaux - SmartOffice API
# Utilisé par le workflow GitHub Actions

# Nom de l'application et environnement
param appName='smartoffice'
param environment='prod'
param location='eastus'

# ?? Secrets (à passer depuis GitHub Secrets)
# Ces valeurs DOIVENT être fournies via GitHub Actions secrets
param jwtSecret=''
param iotHubConnectionString=''
param iotHubEventHubConnectionString=''
param storageConnectionString=''
param postgresqlConnectionString=''
