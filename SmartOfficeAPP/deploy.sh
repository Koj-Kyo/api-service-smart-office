#!/bin/bash

# =========================================
# ?? SmartOffice API - Script de déploiement automatisé
# Facilite le déploiement sur Azure avec azd
# =========================================

set -e

# Couleurs
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

# Fonctions d'affichage
success() { echo -e "${GREEN}? $1${NC}"; }
info() { echo -e "${CYAN}??  $1${NC}"; }
warning() { echo -e "${YELLOW}??  $1${NC}"; }
error() { echo -e "${RED}? $1${NC}"; exit 1; }
step() { echo -e "\n${MAGENTA}?? $1${NC}"; }

# Banner
echo -e "${CYAN}"
cat << "EOF"
?????????????????????????????????????????????????????????????
?                                                           ?
?   ?? SmartOffice API - Déploiement Azure                 ?
?   Infrastructure as Code avec Bicep + azd                ?
?                                                           ?
?????????????????????????????????????????????????????????????
EOF
echo -e "${NC}"

# Paramètres
ACTION=${1:-up}
FORCE=${2:-false}

# Vérifier les prérequis
step "Vérification des prérequis..."

# Azure CLI
if command -v az &> /dev/null; then
    AZ_VERSION=$(az version --query '"azure-cli"' -o tsv)
    success "Azure CLI installé : $AZ_VERSION"
else
    error "Azure CLI non trouvé. Installer depuis : https://aka.ms/installazurecli"
fi

# Azure Developer CLI
if command -v azd &> /dev/null; then
    AZD_VERSION=$(azd version)
    success "Azure Developer CLI installé : $AZD_VERSION"
else
    error "Azure Developer CLI (azd) non trouvé. Installer depuis : https://aka.ms/install-azd"
fi

# .NET SDK
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    success ".NET SDK installé : $DOTNET_VERSION"
else
    error ".NET SDK non trouvé. Installer depuis : https://dotnet.microsoft.com/download"
fi

# Vérifier l'authentification Azure
step "Vérification de l'authentification Azure..."
if az account show &> /dev/null; then
    ACCOUNT_NAME=$(az account show --query user.name -o tsv)
    SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
    SUBSCRIPTION_ID=$(az account show --query id -o tsv)
    success "Connecté à Azure : $ACCOUNT_NAME"
    info "Abonnement actif : $SUBSCRIPTION_NAME ($SUBSCRIPTION_ID)"
else
    warning "Non connecté à Azure. Connexion en cours..."
    az login
    ACCOUNT_NAME=$(az account show --query user.name -o tsv)
    success "Connecté à Azure : $ACCOUNT_NAME"
fi

# Charger les variables d'environnement depuis .env si disponible
if [ -f ".env" ]; then
    step "Chargement des variables d'environnement depuis .env..."
    export $(grep -v '^#' .env | xargs)
    success "Variables d'environnement chargées depuis .env"
else
    warning "Fichier .env non trouvé. Copiez .env.example vers .env et remplissez les valeurs."
    
    if [ "$ACTION" == "up" ] || [ "$ACTION" == "provision" ]; then
        read -p "Voulez-vous créer .env maintenant ? (o/n) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Oo]$ ]]; then
            cp .env.example .env
            success "Fichier .env créé. Éditez-le avec vos valeurs puis relancez le script."
            ${EDITOR:-nano} .env
            exit 0
        fi
    fi
fi

# Exécuter l'action demandée
step "Exécution de l'action : $ACTION"

case $ACTION in
    init)
        info "Initialisation de l'environnement azd..."
        azd init
        success "Environnement azd initialisé"
        ;;
    
    provision)
        info "Provisionnement de l'infrastructure Azure uniquement..."
        warning "Cela créera toutes les ressources Azure sans déployer le code"
        
        if [ "$FORCE" != "true" ]; then
            read -p "Continuer ? (o/n) " -n 1 -r
            echo
            if [[ ! $REPLY =~ ^[Oo]$ ]]; then
                warning "Opération annulée"
                exit 0
            fi
        fi
        
        # Preview des changements
        info "Aperçu des ressources qui seront créées..."
        azd provision --preview
        
        read -p $'\nConfirmer le provisionnement ? (o/n) ' -n 1 -r
        echo
        if [[ $REPLY =~ ^[Oo]$ ]]; then
            azd provision
            success "Infrastructure provisionnée avec succès"
        fi
        ;;
    
    deploy)
        info "Déploiement du code uniquement (sans modifier l'infrastructure)..."
        azd deploy
        success "Code déployé avec succès"
        ;;
    
    up)
        info "Provisionnement de l'infrastructure + déploiement du code..."
        warning "Cela créera toutes les ressources Azure ET déploiera le code"
        
        if [ "$FORCE" != "true" ]; then
            read -p "Continuer ? (o/n) " -n 1 -r
            echo
            if [[ ! $REPLY =~ ^[Oo]$ ]]; then
                warning "Opération annulée"
                exit 0
            fi
        fi
        
        # Déploiement complet
        azd up
        
        # Récupérer les informations de déploiement
        step "Récupération des informations de déploiement..."
        
        WEBAPP_URL=$(azd env get-value AZURE_WEBAPP_URL 2>/dev/null || echo "")
        RESOURCE_GROUP=$(azd env get-value AZURE_RESOURCE_GROUP 2>/dev/null || echo "")
        
        echo -e "\n${GREEN}?????????????????????????????????????????????????????????????${NC}"
        echo -e "${GREEN}?          ?? DÉPLOIEMENT TERMINÉ AVEC SUCCÈS               ?${NC}"
        echo -e "${GREEN}?????????????????????????????????????????????????????????????${NC}\n"
        
        if [ ! -z "$WEBAPP_URL" ]; then
            echo -e "${CYAN}?? Informations de déploiement :${NC}"
            echo "???????????????????????????????????????????????????"
            echo -e "?? URL de l'API       : ${GREEN}$WEBAPP_URL${NC}"
            echo -e "?? Swagger UI         : ${GREEN}$WEBAPP_URL/swagger${NC}"
            echo -e "??  Health Check      : ${GREEN}$WEBAPP_URL/health/ready${NC}"
        fi
        
        if [ ! -z "$RESOURCE_GROUP" ]; then
            echo -e "\n?? Resource Group     : ${YELLOW}$RESOURCE_GROUP${NC}"
            PORTAL_URL="https://portal.azure.com/#@/resource/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP"
            echo -e "?? Portail Azure      : ${CYAN}$PORTAL_URL${NC}"
        fi
        
        echo -e "\n???????????????????????????????????????????????????"
        echo -e "\n${CYAN}?? Commandes utiles :${NC}"
        echo "   ./deploy.sh monitor         # Ouvrir Application Insights"
        echo "   ./deploy.sh logs            # Voir les logs en temps réel"
        echo "   ./deploy.sh status          # Voir l'état du déploiement"
        echo "   ./deploy.sh deploy          # Redéployer uniquement le code"
        echo "   ./deploy.sh down            # Supprimer l'infrastructure"
        echo ""
        
        # Tester le health check
        if [ ! -z "$WEBAPP_URL" ]; then
            step "Test du health check..."
            if curl -s -o /dev/null -w "%{http_code}" "$WEBAPP_URL/health/ready" | grep -q "200"; then
                success "Health check OK : API opérationnelle ?"
            else
                warning "Health check échoué. L'API démarre peut-être encore..."
                info "Attendez 1-2 minutes puis testez : $WEBAPP_URL/health/ready"
            fi
        fi
        
        success "Déploiement terminé avec succès"
        ;;
    
    down)
        warning "ATTENTION : Cela supprimera TOUTE l'infrastructure Azure !"
        warning "Toutes les ressources (App Service, PostgreSQL, Redis, Key Vault) seront supprimées."
        
        if [ "$FORCE" != "true" ]; then
            read -p "Êtes-vous ABSOLUMENT sûr ? Tapez 'SUPPRIMER' pour confirmer : " -r
            echo
            if [ "$REPLY" != "SUPPRIMER" ]; then
                warning "Opération annulée (heureusement !)"
                exit 0
            fi
        fi
        
        info "Suppression de l'infrastructure..."
        azd down --force --purge
        success "Infrastructure supprimée"
        ;;
    
    monitor)
        info "Ouverture d'Application Insights..."
        azd monitor
        ;;
    
    logs)
        info "Affichage des logs en temps réel..."
        azd monitor --logs
        ;;
    
    status)
        step "État de l'environnement azd..."
        azd env list
        
        echo -e "\n${CYAN}?? Valeurs d'environnement :${NC}"
        azd env get-values
        
        WEBAPP_URL=$(azd env get-value AZURE_WEBAPP_URL 2>/dev/null || echo "")
        if [ ! -z "$WEBAPP_URL" ]; then
            step "Test de connectivité..."
            HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "$WEBAPP_URL/health/ready" || echo "000")
            if [ "$HTTP_CODE" == "200" ]; then
                success "API accessible : $WEBAPP_URL"
                success "Status : $HTTP_CODE OK"
            else
                warning "API non accessible (HTTP $HTTP_CODE)"
            fi
        fi
        ;;
    
    *)
        error "Action inconnue : $ACTION"
        echo "Actions disponibles : init, provision, deploy, up, down, monitor, logs, status"
        exit 1
        ;;
esac

echo -e "\n${GREEN}? Opération terminée${NC}"
