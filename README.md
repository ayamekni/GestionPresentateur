# GestionPresentateur

Bienvenue au **Cirque Fantastique**!  
GestionPresentateur est une application web avancée de gestion de spectacles de cirque, avec une interface moderne et une expérience utilisateur (UI/UX) soignée. Elle permet la gestion complète des rôles, des présentateurs et des spectacles, tout en offrant une billetterie et un système de réservation en ligne. L’application est dotée d’une authentification robuste basée sur les rôles, dont un rôle administrateur doté de pouvoirs étendus.

<img width="1919" height="948" alt="gestionPresentateur_homePage" src="https://github.com/user-attachments/assets/6b3b8b04-4f9d-4634-94e9-0c35e27a780e" />


## ✨ Fonctionnalités principales

- **Authentification avancée basée sur les rôles**
  - Prise en charge des rôles multiples : Administrateur, Présentateur, Utilisateur
  - Sécurité accrue avec gestion fine des accès selon le rôle

- **Gestion administrative (Rôle Admin)**
  - L’admin peut :
    - Gérer les comptes utilisateurs et présentateurs
    - Créer, modifier, supprimer les rôles
    - Créer, planifier, modifier et supprimer les spectacles
    - Gérer et valider les réservations
    - Visualiser l’historique des spectacles et des réservations
    - Assigner des rôles et des présentateurs à chaque spectacle

- **Gestion des spectacles**
  - Création et édition des spectacles avec affectation de présentateurs et rôles
  - Visualisation claire des spectacles à venir et passés (dates expirées visibles)
  - Liste dynamique des prochains spectacles et des spectacles passés

- **Réservation et billetterie**
  - Réservation de places en ligne pour les spectacles
  - Génération automatique de tickets électroniques
  - Visualisation de l’historique des réservations utilisateur

- **Interface UI/UX professionnelle**
  - Design responsive, agréable et intuitif
  - Tableaux interactifs pour la gestion et la consultation des spectacles
  - Expérience utilisateur optimisée pour faciliter la navigation et l’action

---

## 🚀 À propos

GestionPresentateur vise à simplifier la gestion quotidienne des événements de cirque, tout en offrant aux visiteurs une plateforme performante pour réserver leurs spectacles préférés.  
Le rôle administrateur bénéficie d’un tableau de bord complet pour superviser tous les aspects du cirque : comptes, rôles, présentateurs, spectacles, réservations et accès.

---


## 🛠️ Technologies

- ASP.NET MVC (C#)
- Entity Framework
- SQL Server
- Bootstrap (UI)
- Authentification ASP.NET Identity

---

## ⚙️ Installation

1. Clonez le dépôt :
   ```sh
   git clone https://github.com/ayamekni/GestionPresentateur.git
   ```
2. Ouvrez le projet dans **Visual Studio**.
3. Exécutez les migrations Entity Framework si besoin (`Update-Database` via la Console du Gestionnaire de Package).
4. Configurez la chaîne de connexion à la base de données dans `appsettings.json`.
5. Lancez le projet.

---

## 👤 Rôles et accès

- **Admin**
  - Gère tous les utilisateurs, rôles, présentateurs, spectacles et réservations
  - Accès complet au back-office
- **Présentateur**
  - Consulte ses propres spectacles assignés
- **Utilisateur**
  - Réserve des places, obtient des tickets, consulte l’historique de ses réservations

---

## 📅 Gestion des Spectacles

- Liste des prochains spectacles et gestion des réservations
- Affichage automatique des spectacles passés (avec dates expirées)

---

## 💡 Points forts

- Interface moderne, responsive et agréable à utiliser
- Authentification et gestion des accès très flexible et puissante
- Outils complets de gestion pour l’admin
- Circuit de réservation et de billetterie fluide pour les utilisateurs

---

> Pour toute question, suggestion ou contribution, ouvrez une issue ou une pull request !
