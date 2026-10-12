# TooDoo — Design

Date : 2026-10-11
Statut : validé en brainstorming, en attente de relecture de la spec

## 1. Objectif

TooDoo est une petite to-do list WPF (.NET 8) à usage personnel, pensée pour vivre dans un coin de l'écran,
utilisée sur **deux PC Windows allumés en même temps** et synchronisée via OneDrive.
C'est un projet d'apprentissage : l'utilisateur écrit le code, Claude le guide (voir `CLAUDE.md`).

### Critères de succès
- Ajouter / cocher / renommer / supprimer tâches et sous-tâches en quelques clics, dans une fenêtre ~320 × 500.
- Organiser les tâches par mois, avec un backlog, et ne jamais perdre une tâche non terminée (report automatique).
- Une modification faite sur un PC apparaît sur l'autre sans action manuelle, sans perte silencieuse de données.
- L'app démarre avec Windows, vit dans le tray, et se met à jour toute seule depuis les GitHub Releases.

### Hors périmètre (YAGNI)
- Sprints / itérations à l'intérieur d'un mois.
- Repli de la fenêtre en barre.
- Fusion fine tâche par tâche lors des conflits de synchro (porte laissée ouverte via `Id` + `UpdatedAt`).
- Réordonnancement par glisser-déposer, annulation (undo), signature de code.

## 2. Décisions de conception

| Sujet | Décision |
|---|---|
| Itérations | **Le mois est l'itération.** Une tâche appartient à un mois ou au backlog. |
| Report | **Automatique** : toute tâche non terminée d'un mois passé est **déplacée** dans le mois courant (opération idempotente). |
| Backlog | Section **repliable en bas** de la fenêtre (max ~40 % de la hauteur, scroll propre). |
| Sous-tâches | **Repliées par défaut**, chevron pour déplier ; état déplié non persisté. |
| Synchro | Fichier JSON dans OneDrive, rechargement par `FileSystemWatcher`, « dernier écrivain gagne » avec 4 garde-fous (§5). |
| Fenêtre | Comportement d'app normale (passe en arrière-plan), option « toujours au premier plan » désactivée par défaut. |
| Barre des tâches | Bouton visible + Alt+Tab quand ouverte ; fermer (×) = cacher dans le tray. |
| Installation | **Velopack** (installeur + mises à jour delta depuis GitHub Releases). |
| Repo | GitHub **public** `https://github.com/EnzoCarbo/TooDoo` (pas de secret embarqué pour les mises à jour). |
| Nom | `TooDoo` partout : projets, `%AppData%\TooDoo\`, `packId`, clé `Run`, tray. |
| Suppression | Sans confirmation, **sauf** une tâche ayant des sous-tâches. |

## 3. Architecture

Solution `TooDoo.sln` avec deux projets :

- `src/TooDoo.App` — WPF, `net8.0-windows`, `UseWPF` + `UseWindowsForms` (pour `NotifyIcon` et `Screen`).
- `tests/TooDoo.Tests` — xUnit, `net8.0-windows`, référence `TooDoo.App`.

Dépendances NuGet : `CommunityToolkit.Mvvm`, `Velopack` (app) ; xUnit (tests). Rien d'autre.

Organisation de `TooDoo.App` :
```
Models/        TodoData, TodoTask, SubTask, YearMonth, AppSettings
Services/      TodoStore, FileWatcherService, CarryOverService, WindowPlacementService,
               AutoStartService, SettingsStore, UpdateService, SingleInstanceService
Abstractions/  IClock, IDialogService, IAutoStartService, ISettingsStore, IScreenProvider, ...
Platform/      implémentations dépendant de WPF/WinForms : TrayIcon, DialogService, ScreenProvider
ViewModels/    MainViewModel, TaskListViewModel, TaskItemViewModel, SubTaskItemViewModel, SettingsViewModel
Views/         MainWindow, contrôles de liste, panneau Paramètres
Program.cs     Main personnalisé (VelopackApp.Build().Run() en premier)
```

`<AssemblyName>TooDoo</AssemblyName>` : l'exécutable produit s'appelle `TooDoo.exe`.

**Règle :** aucun type WPF/WinForms dans `Models/`, `Services/` ni `ViewModels/` ; seuls `Platform/` et `Views/` y ont droit.
Les dépendances système (horloge, fichiers, registre, dialogues, écrans) passent par des interfaces pour être simulées en test.

## 4. Modèle de données

```
TodoData   { int SchemaVersion = 1; List<TodoTask> Tasks }
TodoTask   { Guid Id; string Title; bool IsDone; YearMonth? Month;   // null = backlog
             DateTime CreatedAt; DateTime UpdatedAt; List<SubTask> SubTasks }
SubTask    { Guid Id; string Title; bool IsDone }
YearMonth  record struct { int Year; int Month; Next(); Previous(); comparable }
```

- `YearMonth` est sérialisé en `"2026-10"` via un `JsonConverter` dédié.
- Ordre d'affichage = ordre de la liste ; une nouvelle tâche est ajoutée en fin.
- Tâche cochée : reste en place, barrée et grisée.
- Progression (`1/3`) : propriété calculée dans le ViewModel, non stockée.
- `UpdatedAt` est mis à jour à chaque modification de la tâche ou de ses sous-tâches.

### Report automatique — `CarryOverService.Apply(TodoData, YearMonth current)`
Toute tâche avec `IsDone == false` et `Month < current` reçoit `Month = current` (sous-tâches incluses, même partiellement faites).
Déclenché au chargement, à la réactivation de la fenêtre et par un minuteur horaire si le mois a changé.
L'opération est un déplacement : l'appliquer deux fois (ou sur les deux PC) donne le même résultat.

### Paramètres locaux — `%AppData%\TooDoo\settings.json` (jamais synchronisé)
Chemin du dossier de données, coin d'ancrage, position / taille / écran mémorisés, toujours au premier plan, backlog déplié.
Le démarrage auto n'y est **pas** stocké : le registre est la source de vérité.

## 5. Persistance et synchronisation

Fichier de données : `<dossier configuré>\todo.json` (par défaut `%AppData%\TooDoo\`, en pratique un dossier OneDrive).

### `TodoStore`
Toute modification passe par `store.Update(Action<TodoData> mutation)` :
1. Calcul du hash SHA-256 du fichier ; s'il diffère du dernier hash connu → rechargement préalable.
2. Application de la mutation sur les données à jour.
3. Écriture atomique : `todo.json.tmp` puis `File.Move(tmp, path, overwrite: true)`.
4. Mémorisation du nouveau hash.
5. Événement `DataChanged`.

Sauvegarde immédiate à chaque action utilisateur (pas de debounce d'écriture).
Au démarrage, copie de `todo.json` vers `todo.json.bak`.

### `FileWatcherService`
- Surveille le dossier, debounce ~500 ms.
- Changement de `todo.json` dont le hash diffère du dernier connu → rechargement + `DataChanged`
  (nos propres écritures sont ignorées car leur hash est connu).
- Apparition d'un fichier `todo-*.json` (copie de conflit OneDrive) → bandeau « Conflit de synchronisation détecté ».

### Garde-fous (récapitulatif)
1. Rechargement automatique sur changement externe.
2. Vérification du hash avant chaque écriture, rechargement puis ré-application de la mutation.
3. Écriture atomique.
4. Détection et signalement des copies de conflit OneDrive.

Risque résiduel accepté : modification sur un PC dans les secondes qui suivent une sortie de veille,
avant que OneDrive ait synchronisé → copie de conflit OneDrive, signalée par le garde-fou 4.

### Gestion d'erreurs
- JSON illisible au démarrage → renommé en `todo.corrupt-<horodatage>.json`, démarrage sur liste vide, bandeau.
- JSON illisible lors d'un rechargement → nouvel essai différé, l'état courant est conservé.
- `IOException` à l'écriture → 3 essais avec délai croissant, puis bandeau « Sauvegarde impossible ».
- Changement de dossier de données : si le dossier cible contient un `todo.json`, il est chargé ;
  sinon les données courantes y sont déplacées.

## 6. ViewModels

- `MainViewModel` : `CurrentMonth`, `PreviousMonthCommand`, `NextMonthCommand`, `GoToCurrentMonthCommand`,
  `MonthList`, `Backlog`, `IsBacklogExpanded`, `Banner`, `AppVersion`, `UpdateAvailable`, `ShowSettings`.
- `TaskListViewModel` (mois ou backlog) : `Items`, `NewTaskTitle`, `AddTaskCommand`.
- `TaskItemViewModel` : `Title`, `IsDone`, `IsEditing`, `IsExpanded`, `Progress`, `SubTasks`,
  commandes `Rename`, `Delete`, `MoveToBacklog`, `MoveToPreviousMonth`, `MoveToNextMonth`, `MoveToCurrentMonth`, `AddSubTask`.
- `SubTaskItemViewModel` : `Title`, `IsDone`, `Rename`, `Delete`.
- `SettingsViewModel` : coin, toujours au premier plan, démarrage auto, dossier de données, version.

Sur `DataChanged`, les listes sont reconstruites depuis le modèle ; `IsExpanded` est préservé par `Id`.
La suppression d'une tâche ayant des sous-tâches demande confirmation via `IDialogService.Confirm`.

## 7. Fenêtre et intégration Windows

### Fenêtre
- `WindowStyle="None"` + `WindowChrome` (`CaptionHeight` ~28) : barre de titre custom fine, déplaçable,
  bords de redimensionnement natifs. Pas de `AllowsTransparency`.
- Taille par défaut 320 × 500, redimensionnable avec taille minimale, taille mémorisée.
- Barre de titre : nom, bouton Paramètres, bouton × (cacher dans le tray).
- En-tête : `◀  Octobre 2026  ▶` ; clic sur le nom du mois = retour au mois courant.
- Bandeau d'information (conflit, erreur de sauvegarde, mise à jour prête) sous l'en-tête.

### Positionnement — `WindowPlacementService` (pur, testé)
Entrées : position/taille mémorisées + nom d'écran, liste des écrans (nom + zone de travail), coin préféré.
Sortie : le `Rect` à appliquer.
- Position mémorisée suffisamment visible sur un écran existant → on la garde.
- Sinon → ancrage au coin préféré de la zone de travail de l'écran principal (défaut : bas droite).
Écrans obtenus via `System.Windows.Forms.Screen.AllScreens`, conversion pixels physiques ↔ DIP selon le DPI (Per-Monitor V2).
Action « Réancrer dans le coin » dans le menu.

### Interactions
- Ajout : ligne `+ Ajouter une tâche…`, Entrée (idem pour sous-tâches).
- Cocher : case à cocher.
- Renommer : double-clic → édition en place ; Entrée valide, Échap annule, titre vide = annulation.
- Survol : icônes `+` (sous-tâche) et 🗑.
- Clic droit : Renommer, Ajouter une sous-tâche, Déplacer vers ▸ (Backlog, Mois précédent, Mois suivant, Mois courant), Supprimer.

### Tray et cycle de vie
- `NotifyIcon` : clic gauche = afficher et activer ; menu clic droit : Afficher, Paramètres, Quitter.
- × et fermeture → fenêtre cachée (`Closing` annulé) ; seul « Quitter » termine l'app.
- **Instance unique** : `Mutex` nommé ; une seconde instance signale la première via `EventWaitHandle` pour qu'elle s'affiche, puis se termine.

### Démarrage auto — `AutoStartService`
Valeur `TooDoo` dans `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, pointant vers le chemin d'exe stable fourni par Velopack
(à confirmer à l'étape release). Supprimée par le hook de désinstallation Velopack.

### Paramètres
Panneau affiché à la place de la liste dans la même fenêtre : coin d'ancrage (4 choix), toujours au premier plan,
démarrer avec Windows, dossier des données (`OpenFolderDialog`), version affichée.

### Mises à jour — `UpdateService`
Velopack `UpdateManager` + `GithubSource("https://github.com/EnzoCarbo/TooDoo")` (repo public). Vérification au démarrage puis toutes les 4 h ;
téléchargement en arrière-plan ; bandeau « Mise à jour vX.Y.Z prête — redémarrer ». Jamais de redémarrage forcé.

## 8. CI/CD, release et versionnage

### Structure du repo
```
.github/workflows/ci.yml, release.yml
docs/superpowers/specs/, docs/superpowers/plans/
src/TooDoo.App/
tests/TooDoo.Tests/
TooDoo.sln, .gitignore, README.md, CLAUDE.md
```

Flux de travail : une branche par étape du plan, PR vers `main`, CI verte + revue avant merge.

### `ci.yml` — push et pull_request
`windows-latest` → `actions/setup-dotnet` 8.x → `dotnet restore` → `dotnet build -c Release --no-restore` → `dotnet test --no-build -c Release`.

### `release.yml` — tag `v*.*.*`
1. Build + tests.
2. Version extraite du tag (`v1.2.0` → `1.2.0`).
3. `dotnet publish src/TooDoo.App -c Release -r win-x64 --self-contained -p:Version=<version> -o publish`.
4. Installation de `vpk`, `vpk download github` (release précédente, pour les deltas).
5. `vpk pack --packId TooDoo --packVersion <version> --packDir publish --mainExe TooDoo.exe`.
6. `vpk upload github --publish --tag v<version>` avec `GITHUB_TOKEN` ; `permissions: contents: write`.

### Versionnage
- SemVer ; le tag git est la seule source de vérité ; build local = `0.0.0-dev`.
- Version affichée lue depuis `AssemblyInformationalVersionAttribute`, avec
  `IncludeSourceRevisionInInformationalVersion=false` pour éviter le suffixe `+sha`.
- Exe non signé : avertissement SmartScreen au premier lancement du Setup sur chaque PC (accepté).

## 9. Stratégie de test (TDD, xUnit)

| Cible | Type |
|---|---|
| `YearMonth` (Next/Previous/comparaison/JSON) | unitaire |
| Opérations sur `TodoData` (ajout, renommage, suppression, déplacement, sous-tâches, `UpdatedAt`) | unitaire |
| `CarryOverService` (y compris idempotence, tâches faites non déplacées, backlog intact) | unitaire avec `IClock` simulé |
| `WindowPlacementService` (écran débranché, 4 coins, multi-écran) | unitaire |
| `TodoStore` (aller-retour JSON, écriture atomique, modification externe entre chargement et sauvegarde, JSON corrompu) | intégration sur dossier temporaire |
| ViewModels (navigation de mois, progression, confirmation de suppression, reconstruction sur `DataChanged`) | unitaire avec faux store / horloge / dialogues |

Non testé automatiquement (vérification manuelle) : rendu XAML, tray, registre réel, Velopack, `FileSystemWatcher` réel avec OneDrive.
