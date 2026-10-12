> **Le design validé fait foi** : `docs/superpowers/specs/2026-10-11-toodoo-design.md`.
> Il remplace certains points du brief ci-dessous (nom `TooDoo`, mois = itération, pas de repli en barre, Velopack).
>
> **Git :** aucune mention de Claude dans les commits, PR, tags ou releases (pas de `Co-Authored-By`, pas de « Generated with Claude Code »).

# Contexte

Je suis développeur C# / .NET (alternant full-stack), je code tous les jours en C# au travail.
Je veux créer une petite application de to-do list pour mes PC Windows, en C# / WPF (.NET 8).
Je l'utiliserai sur DEUX PC différents.

# TON RÔLE : me guider, PAS coder à ma place

C'est un projet d'apprentissage. Règles strictes :
- Ne crée et ne modifie AUCUN fichier de code sans que je te le demande explicitement.
  Seule exception : les documents de conception et de plan produits par les skills Superpowers.
- Avance étape par étape : explique ce qu'on fait et pourquoi, puis laisse-moi écrire le code.
- Donne des extraits de code courts uniquement pour un concept nouveau pour moi
  (NotifyIcon, registre, positionnement de fenêtre, YAML GitHub Actions…) ou si je suis bloqué après avoir essayé.
- Quand je te dis « j'ai fini l'étape », relis mes fichiers, fais une revue honnête
  et dis-moi quoi tester avant de passer à la suite.
- Si je fais un choix discutable, dis-le franchement et explique l'alternative.
- À chaque étape, donne-moi l'objectif, les fichiers concernés, les notions à connaître
  et un critère de « c'est terminé ».

# Utilisation de Superpowers (obligatoire)

Utilise les skills Superpowers, en les adaptant au mode « guide » :
- superpowers:brainstorming → au début, pour clarifier avec moi le besoin et le design
  (modèle des itérations, synchronisation entre les 2 PC, UI). Pose-moi les questions une par une.
- superpowers:writing-plans → pour rédiger le plan d'implémentation, découpé en petites étapes.
- NE PAS utiliser executing-plans ni subagent-driven-development pour coder à ma place :
  tu me guides à travers le plan, et c'est moi qui écris le code.
- superpowers:test-driven-development → guide-moi pour écrire d'abord les tests (xUnit)
  sur la logique métier (ViewModels, services d'itérations, persistance).
- superpowers:requesting-code-review → pour relire mon code à la fin de chaque étape.
- superpowers:systematic-debugging → quand j'ai un bug, guide-moi dans le diagnostic
  au lieu de me donner directement la correction.
- superpowers:verification-before-completion → avant de valider une étape, vérifie
  que ça build, que les tests passent et que le critère « terminé » est rempli.

# Fonctionnalités voulues

1. Tâches : ajouter, cocher/décocher, modifier le titre, supprimer.
2. Sous-tâches : chaque tâche peut avoir des sous-tâches, cochables, modifiables et supprimables.
   Afficher la progression (ex. 2/5).
3. Itérations façon Azure DevOps :
   - Navigation par mois (◀ / ▶).
   - Chaque mois est découpé en itérations (sprints) avec nom, date de début et date de fin.
   - Un backlog pour les tâches non assignées.
   - Déplacer une tâche vers une autre itération ; reporter les tâches non terminées vers l'itération suivante.
   → Propose-moi un modèle simple (2 semaines fixes ou durée configurable) pendant le brainstorming.
4. Fenêtre PETITE, faite pour vivre dans un coin de l'écran :
   - Taille compacte (environ 320 × 500 px), sans bordure Windows classique, avec une barre de titre
     custom fine qui permet de la déplacer.
   - Ancrée par défaut en bas à droite de la zone de travail (au-dessus de la barre des tâches,
     via SystemParameters.WorkArea), avec un choix du coin dans les paramètres.
   - Gestion du multi-écran : mémoriser l'écran et la position, et revenir à une position
     valide si l'écran n'est plus branché.
   - Option « toujours au premier plan », et possibilité de replier la fenêtre en une simple barre.
   - UI dense et lisible : pas de gros boutons, actions au survol ou au clic droit.
5. Toujours disponible :
   - Lancement auto au démarrage de Windows (HKCU\...\CurrentVersion\Run), activable dans l'app.
   - Icône dans le system tray : fermer = réduire dans le tray ; « Quitter » dans le menu clic droit.
6. Persistance : JSON (System.Text.Json), sauvegarde automatique.
   Par défaut dans %AppData%\MaTodo\, avec un chemin configurable.
   → Pendant le brainstorming, pose-moi la question de la synchronisation entre mes 2 PC
     (ex. fichier dans un dossier OneDrive + rechargement via FileSystemWatcher) et explique les risques de conflit.

# CI/CD et installation sur mes 2 PC

- Repo GitHub.
- GitHub Actions (runner windows-latest) :
  - Sur chaque push/PR : restore, build, tests.
  - Sur un tag de version (v1.2.0) : dotnet publish win-x64 self-contained, packaging
    et création d'une GitHub Release.
- Installation et mises à jour : propose-moi une solution avec mise à jour automatique depuis
  les GitHub Releases (par ex. Velopack), compare-la avec un simple .exe single-file et laisse-moi choisir.
- Versionnage sémantique, numéro de version affiché dans l'app.

# Contraintes techniques

- .NET 8, WPF, MVVM propre (CommunityToolkit.Mvvm conseillé).
- Solution avec un projet app + un projet de tests xUnit.
- Pas de base de données, pas de dépendances inutiles.
- Code et noms en anglais, explications en français.

# Déroulé attendu

1. Brainstorming avec moi (skill brainstorming), puis rédaction du design.
2. Plan d'implémentation (skill writing-plans), découpé du plus simple au plus complet :
   repo + solution + CI de build → modèle de données (TDD) → liste de tâches → sous-tâches →
   persistance JSON → itérations et navigation par mois → fenêtre compacte ancrée dans un coin →
   tray et démarrage auto → release et installation → finitions.
3. Attends ma validation du plan, puis guide-moi étape par étape.
