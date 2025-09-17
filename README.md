
# Application web de vente Ecommerce

![.NET](https://img.shields.io/badge/.NET-9.0-blue?logo=dotnet)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)


Ce projet est une application web de vente en ligne développée en ASP.NET Core MVC. Elle permet la gestion des produits, des utilisateurs, des commandes, du panier, ainsi que l'intégration de paiements (Stripe) et la gestion des vendeurs.

## Fonctionnalités principales
- Authentification et gestion des utilisateurs (inscription, connexion, profil)
- Gestion des produits (ajout, modification, suppression, recherche, catégories)
- Gestion du panier d'achat
- Passage de commandes et génération de factures
- Suivi de l'historique des commandes
- Tableau de bord pour les vendeurs
- Intégration du paiement en ligne avec Stripe

## Structure du projet
- `Controllers/` : Contrôleurs MVC pour la gestion des routes et de la logique métier
- `Models/` : Modèles de données (produits, utilisateurs, commandes, etc.)
- `Views/` : Vues Razor pour l'affichage côté client
- `Services/` : Services métiers (paiement, initialisation de la base, etc.)
- `Data/` : Contexte de base de données Entity Framework
- `Migrations/` : Fichiers de migration de la base de données
- `wwwroot/` : Fichiers statiques (CSS, JS, images)

## Prérequis
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- Un éditeur comme [Visual Studio](https://visualstudio.microsoft.com/fr/) ou [Visual Studio Code](https://code.visualstudio.com/)
- Accès à Internet pour restaurer les dépendances NuGet

## Installation

1. **Cloner le dépôt**
   ```powershell
   git clone <url-du-repo>
   cd "Application web de vente Ecommerce"
   ```

2. **Restaurer les dépendances NuGet**
   ```powershell
   dotnet restore
   ```

3. **Appliquer les migrations et créer la base de données**
   ```powershell
   dotnet ef database update
   ```
   > Si la commande `dotnet ef` n'est pas reconnue, installez l'outil Entity Framework CLI :
   > ```powershell
   > dotnet tool install --global dotnet-ef
   > ```

4. **Configurer les fichiers de configuration**
   - Modifiez `appsettings.json` et `appsettings.Development.json` pour adapter la chaîne de connexion à votre base de données et les clés Stripe.

5. **Lancer l'application**
   ```powershell
   dotnet run
   ```
   L'application sera accessible à l'adresse indiquée dans la console (par défaut http://localhost:5000).

## Installation des librairies nécessaires

Les principales librairies utilisées sont automatiquement restaurées avec `dotnet restore`. Voici les plus importantes :
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Stripe.net

Si besoin, vous pouvez les installer manuellement :
```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Stripe.net
```


## Déploiement
Pour déployer l'application sur un serveur, adaptez la configuration de la base de données et des clés API dans les fichiers `appsettings.*.json`.

## Contribution

Les contributions sont les bienvenues ! Merci de soumettre vos issues et pull requests pour améliorer le projet.




