# Tests API Smart Office Application

Ce dossier contient une collection complète de tests HTTP pour toutes les APIs de l'application Smart Office.

## Structure des fichiers de test

### 01-Authentication.http
Tests complets pour l'authentification et l'autorisation :
- Enregistrement d'utilisateurs
- Connexion utilisateur/admin
- Tests d'autorisation
- Gestion des tokens JWT
- Tests d'erreurs d'authentification

### 02-Zones.http
Tests pour la gestion des zones (Admin uniquement) :
- CRUD complet des zones
- Tests d'autorisation
- Gestion des erreurs

### 03-Devices.http
Tests pour la gestion des périphériques :
- CRUD des périphériques logiques
- Gestion des desired properties IoT
- Association zones-périphériques
- Tests d'autorisation

### 04-Telemetry.http
Tests pour la télémétrie :
- Ajout de données de télémétrie
- Récupération par périphérique
- Données historiques
- Suggestions IA
- Tests avec différents types d'événements

### 05-Commands-Notifications.http
Tests pour les commandes et notifications :
- Envoi de commandes aux périphériques
- Gestion des notifications utilisateur
- Différents types de commandes
- Tests d'autorisation

### 06-AccessRights-Incidents.http
Tests pour les droits d'accès et tickets d'incident :
- CRUD des droits d'accès (Admin)
- Consultation des droits utilisateur
- Gestion complète des tickets d'incident
- Workflow d'assignation et de résolution

### 07-User-Management.http
Tests pour la gestion des utilisateurs :
- Gestion des profils utilisateur
- Administration des utilisateurs (Admin)
- Gestion des rôles
- Association utilisateurs-rôles
- Statistiques

### 08-Energy-Analytics.http
Tests pour l'analyse énergétique :
- Statistiques de consommation
- Analyses horaires et par tendances
- Détection d'anomalies
- Rapports énergétiques
- Calculs de coûts
- Comparaisons temporelles

### 09-IoT-Hub.http
Tests pour l'intégration Azure IoT Hub :
- Méthodes directes sur périphériques
- Gestion des device twins
- Messages cloud-to-device
- Gestion des périphériques IoT
- Commandes intelligentes prédéfinies

### 10-Integration-Tests.http
Tests d'intégration et scenarios complets :
- Workflows complets utilisateur
- Tests de charge
- Tests de sécurité
- Tests de performance
- Gestion d'erreurs

## Comment utiliser ces tests

### Prérequis
1. Application Smart Office démarrée sur `https://localhost:7242`
2. Extension REST Client pour VS Code ou outil similaire
3. Tokens JWT valides pour admin et utilisateur

### Configuration
1. Modifier les variables dans chaque fichier :
   ```
   @baseUrl = https://localhost:7242
   @adminToken = votre_token_admin_ici
   @userToken = votre_token_utilisateur_ici
   ```

2. Obtenir les tokens :
   - Utiliser d'abord `01-Authentication.http` pour s'enregistrer et se connecter
   - Copier les tokens retournés dans les variables

### Ordre d'exécution recommandé
1. **01-Authentication.http** - Obtenir les tokens
2. **02-Zones.http** - Créer des zones de test
3. **03-Devices.http** - Créer des périphériques
4. **06-AccessRights-Incidents.http** - Configurer les droits
5. **04-Telemetry.http** - Ajouter des données
6. **05-Commands-Notifications.http** - Tester les commandes
7. **07-User-Management.http** - Gestion utilisateurs
8. **08-Energy-Analytics.http** - Analyses énergétiques
9. **09-IoT-Hub.http** - Tests IoT
10. **10-Integration-Tests.http** - Tests intégrés

## Types de tests inclus

### Tests fonctionnels
- ? CRUD complet pour toutes les entités
- ? Workflows métier complets
- ? Validation des données

### Tests de sécurité
- ? Tests d'autorisation par rôle
- ? Validation des tokens JWT
- ? Tests d'accès non autorisé
- ? Protection contre l'injection SQL
- ? Protection XSS

### Tests d'erreur
- ? Données invalides
- ? Ressources inexistantes
- ? Formats incorrects
- ? Payloads malformés

### Tests de performance
- ? Requêtes avec gros volumes
- ? Requêtes concurrentes
- ? Tests de charge

## Variables dynamiques disponibles

Les fichiers utilisent des variables dynamiques REST Client :
- `{{$timestamp}}` - Timestamp actuel
- `{{$datetime iso8601}}` - Date ISO 8601
- `{{$randomInt}}` - Entier aléatoire
- `{{$guid}}` - GUID aléatoire
- `{{$randomString}}` - Chaîne aléatoire

## Scénarios de test avancés

### Test de workflow complet
Le fichier `10-Integration-Tests.http` contient un scénario complet :
1. Création d'utilisateur
2. Création de zone et périphérique
3. Attribution de droits
4. Envoi de commandes
5. Ajout de télémétrie
6. Création de ticket
7. Analyses énergétiques

### Tests IoT Hub
Tests spécialisés pour Azure IoT Hub :
- Méthodes directes
- Device twins
- Messages cloud-to-device
- Gestion des périphériques

## Monitoring et debugging

Pour surveiller les tests :
1. Vérifier les logs de l'application
2. Utiliser les outils de développement du navigateur
3. Consulter Azure IoT Hub si connecté
4. Vérifier la base de données PostgreSQL

## Automatisation

Ces tests peuvent être automatisés avec :
- Newman (runner Postman en CLI)
- Scripts PowerShell/Bash
- Pipelines CI/CD
- Tests d'intégration .NET

## Support

Pour tout problème avec les tests :
1. Vérifier que l'application est démarrée
2. Valider les tokens JWT
3. Contrôler les permissions des rôles
4. Consulter les logs d'application