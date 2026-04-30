# Application TaskManager — Documentation Technique

## Projet Final DevSecOps — Bachelor Cybersécurité 2025

|                   |                                                   |
| ----------------- | ------------------------------------------------- |
| Application       | TaskManager (Gestionnaire de tâches)              |
| Architecture      | Full-stack : Backend .NET 8 + Frontend Angular 21 |
| Base de données   | MariaDB 11.4                                      |
| Conteneurisation  | Docker + Docker Compose                           |
| Date de rédaction | Avril 2026                                        |

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Architecture et fonctionnement](#2-architecture-et-fonctionnement)
3. [Prérequis et dépendances](#3-prérequis-et-dépendances)
4. [Installation et déploiement](#4-installation-et-déploiement)
5. [API REST — Endpoints et fonctionnalités](#5-api-rest--endpoints-et-fonctionnalités)
6. [Frontend Angular — Composants et services](#6-frontend-angular--composants-et-services)
7. [Base de données — Schéma et migrations](#7-base-de-données--schéma-et-migrations)
8. [Sécurité — OWASP Top 10](#8-sécurité--owasp-top-10)
9. [Tests — Backend et Frontend](#9-tests--backend-et-frontend)
10. [Docker — Hardening et sécurité](#10-docker--hardening-et-sécurité)
11. [Configuration et variables d'environnement](#11-configuration-et-variables-denvironnement)
12. [Ce qui a été fait](#12-ce-qui-a-été-fait)

---

## 1. Vue d'ensemble

L'application **TaskManager** est une solution full-stack de gestion des utilisateurs et des tâches, développée dans le cadre du projet final DevSecOps. Elle implémente les meilleures pratiques de sécurité OWASP et de conteneurisation sécurisée.

### Fonctionnalités principales

- **Gestion des utilisateurs** : CRUD complet avec authentification basique
- **Gestion des tâches** : Création, modification, suppression (soft delete)
- **Interface web moderne** : Angular 21 avec signaux réactifs
- **API REST sécurisée** : .NET 8 avec Entity Framework Core
- **Base de données relationnelle** : MariaDB avec migrations EF Core

### Technologies utilisées

| Composant        | Technologie                    | Version |
| ---------------- | ------------------------------ | ------- |
| Backend          | .NET 8 (ASP.NET Core)          | 8.0     |
| Frontend         | Angular                        | 21.2.0  |
| Base de données  | MariaDB                        | 11.4    |
| Tests Backend    | xUnit + Moq + FluentAssertions | -       |
| Tests Frontend   | Vitest                         | 4.0.8   |
| Conteneurisation | Docker + Docker Compose        | -       |

---

## 2. Architecture et fonctionnement

### Architecture en couches

```
┌─────────────────┐    HTTP/HTTPS    ┌─────────────────┐
│   Frontend      │◄────────────────►│     Backend     │
│   Angular 21    │                  │   .NET 8 API    │
│   (Port 8080)   │                  │   (Port 5000)   │
└─────────────────┘                  └─────────────────┘
         │                                   │
         └───────────────────────────────────┼─────────────────┐
                                             ▼                 │
                                   ┌─────────────────┐         │
                                   │    MariaDB      │◄────────┘
                                   │   (Port 3306)   │
                                   └─────────────────┘
```

### Flux de données

1. **Frontend** : Interface utilisateur en Angular avec services HTTP
2. **Backend** : API REST qui traite les requêtes et applique la logique métier
3. **Base de données** : Stockage persistant avec Entity Framework Core
4. **Sécurité** : Headers de sécurité, rate limiting, gestion d'erreurs

### Communication inter-conteneurs

- **Frontend → Backend** : Via proxy nginx (`/api/*` → `http://back:5000`)
- **Backend → Base de données** : Connexion directe MariaDB (`Server=db;Port=3306`)

---

## 3. Prérequis et dépendances

### Environnement de développement

- **Docker** >= 24.0
- **Docker Compose** >= 2.20
- **Git** pour le versioning
- **Navigateur moderne** (Chrome/Firefox) pour le frontend

### Dépendances backend (.NET)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.4" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Cors" Version="8.0.4" />
<PackageReference Include="Microsoft.Extensions.RateLimiting" Version="8.0.0" />
```

### Dépendances frontend (Angular)

```json
{
  "@angular/core": "^21.2.0",
  "@angular/common": "^21.2.0",
  "@angular/router": "^21.2.0",
  "@angular/forms": "^21.2.0",
  "rxjs": "~7.8.0"
}
```

### Dépendances de test

```xml
<!-- Backend -->
<PackageReference Include="xunit" Version="2.6.6" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

```json
// Frontend
{
  "@angular/build": "^21.2.7",
  "vitest": "^4.0.8",
  "jsdom": "^28.0.0"
}
```

---

## 4. Installation et déploiement

### Démarrage rapide (Docker)

```bash
# Cloner le repository
git clone <repository-url>
cd taskmanager-devsecops

# Copier le fichier d'environnement
cp docker/.env.example docker/.env

# Éditer les variables sensibles dans docker/.env
# DB_PASSWORD, DB_ROOT_PASSWORD, CORS_ALLOWED_ORIGINS

# Lancer les services
cd docker
docker compose up --build
```

### Accès aux services

- **Frontend** : http://localhost:8080
- **Backend API** : http://localhost:5000 (accessible uniquement en dev)
- **Base de données** : localhost:3306 (non exposé publiquement)

### Vérification du déploiement

```bash
# Vérifier les conteneurs
docker ps

# Vérifier les logs
docker logs taskmanager-devsecops-back-1
docker logs taskmanager-devsecops-front-1

# Tester l'API
curl http://localhost:5000/health
curl http://localhost:8080/
```

---

## 5. API REST — Endpoints et fonctionnalités

### Base URL

- **Développement** : `http://localhost:5000/api`
- **Production** : `/api` (via proxy nginx)

### Endpoints utilisateurs (`/api/users`)

| Méthode | Endpoint          | Description                         | Code de retour |
| ------- | ----------------- | ----------------------------------- | -------------- |
| GET     | `/api/users`      | Liste tous les utilisateurs actifs  | 200            |
| GET     | `/api/users/{id}` | Récupère un utilisateur par ID      | 200/404        |
| POST    | `/api/users`      | Crée un nouvel utilisateur          | 201/409        |
| PUT     | `/api/users/{id}` | Met à jour un utilisateur           | 204/404        |
| DELETE  | `/api/users/{id}` | Supprime logiquement un utilisateur | 204/404        |

### Modèles de données

#### CreateUserRequest

```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "isAdmin": false
}
```

#### UpdateUserRequest

```json
{
  "username": "string",
  "email": "string",
  "password": "string", // optionnel
  "isAdmin": false,
  "status": "ACTIVE|DELETED"
}
```

#### UserResponse

```json
{
  "id": "uuid",
  "username": "string",
  "email": "string",
  "isAdmin": false,
  "status": "ACTIVE|DELETED",
  "creationDate": "2026-04-30T10:00:00Z"
}
```

### Gestion d'erreurs

- **400 Bad Request** : Données invalides
- **404 Not Found** : Ressource inexistante
- **409 Conflict** : Email déjà utilisé
- **429 Too Many Requests** : Rate limiting dépassé
- **500 Internal Server Error** : Erreur serveur (sans stack trace en prod)

---

## 6. Frontend Angular — Composants et services

### Structure des composants

```
src/app/
├── app.config.ts          # Configuration Angular (providers, routes)
├── app.routes.ts          # Définition des routes
├── core/
│   ├── models/            # Interfaces TypeScript
│   │   ├── user.model.ts
│   │   └── subject.model.ts
│   └── services/          # Services HTTP
│       └── user.service.ts
├── views/                 # Composants de vue
│   ├── home/
│   ├── users/
│   │   ├── user-list/
│   │   └── user-form/
│   └── layout/
│       └── navbar/
└── environments/          # Configuration par environnement
    ├── environment.ts
    └── environment.prod.ts
```

### Services principaux

#### UserService

Service réactif utilisant les signaux Angular pour la gestion d'état :

```typescript
@Injectable({ providedIn: "root" })
export class UserService {
  // État réactif
  users = signal<User[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  // État dérivé
  adminCount = computed(() => this.users().filter((u) => u.isAdmin).length);

  // Méthodes CRUD
  loadAll(): void {
    /* ... */
  }
  create(request: CreateUserRequest): Observable<User> {
    /* ... */
  }
  update(id: string, request: UpdateUserRequest): Observable<void> {
    /* ... */
  }
  remove(id: string): Observable<void> {
    /* ... */
  }
}
```

### Composants

#### UserListComponent

- Affiche la liste des utilisateurs dans un tableau
- Gestion des états loading/error
- Boutons d'action (modifier, supprimer)
- Compteur d'administrateurs

#### UserFormComponent

- Formulaire réactif pour création/édition
- Validation côté client
- Gestion du mode création vs édition
- Navigation après soumission

### Routage

```typescript
export const routes: Routes = [
  { path: "", component: HomeComponent },
  { path: "users", component: UserListComponent },
  { path: "users/new", component: UserFormComponent },
  { path: "users/:id/edit", component: UserFormComponent },
  { path: "**", redirectTo: "" },
];
```

---

## 7. Base de données — Schéma et migrations

### Schéma principal

#### Table `CmUsers`

| Colonne      | Type         | Contrainte       | Description                 |
| ------------ | ------------ | ---------------- | --------------------------- |
| Id           | CHAR(36)     | PRIMARY KEY      | UUID de l'utilisateur       |
| Username     | VARCHAR(100) | NOT NULL         | Nom d'utilisateur           |
| Email        | VARCHAR(255) | UNIQUE, NOT NULL | Email (unique)              |
| PasswordHash | VARCHAR(255) | NOT NULL         | Hash BCrypt du mot de passe |
| IsAdmin      | TINYINT(1)   | NOT NULL         | Flag administrateur         |
| Status       | VARCHAR(100) | NOT NULL         | Statut (ACTIVE/DELETED)     |
| CreationDate | DATETIME     | NOT NULL         | Date de création            |

#### Table `CmSubjects` (structure similaire)

### Migrations Entity Framework

```csharp
// InitialCreate.cs
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "CmUsers",
        columns: table => new
        {
            Id = table.Column<string>(type: "varchar(36)", nullable: false),
            Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
            Email = table.Column<string>(type: "varchar(255)", nullable: false),
            PasswordHash = table.Column<string>(type: "varchar(255)", nullable: false),
            IsAdmin = table.Column<bool>(type: "tinyint(1)", nullable: false),
            Status = table.Column<string>(type: "varchar(100)", nullable: false),
            CreationDate = table.Column<DateTime>(type: "datetime", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_CmUsers", x => x.Id);
        });
}
```

### Connexion et contexte

```csharp
// Program.cs
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

---

## 8. Sécurité — OWASP Top 10

| Contrôle                                | Implémentation          | Détails                                                |
| --------------------------------------- | ----------------------- | ------------------------------------------------------ |
| **A01 Broken Access Control**           | Validation email unique | `409 Conflict` si email déjà utilisé                   |
| **A02 Cryptographic Failures**          | Secrets externalisés    | Variables dans `.env`, `.env.example` commité          |
| **A03 Injection**                       | Requêtes paramétrées    | EF Core protège automatiquement contre l'injection SQL |
| **A05 Security Misconfiguration**       | Headers de sécurité     | `SecurityHeadersMiddleware` personnalisé               |
|                                         |                         | Swagger désactivé en production                        |
|                                         |                         | Pas de stack traces en production                      |
|                                         |                         | Header `Server` supprimé                               |
| **A07 Identification and Auth Failure** | Rate limiting           | 60 requêtes/minute par IP                              |
|                                         |                         | Gestion d'erreurs propre sans fuite d'informations     |

### SecurityHeadersMiddleware

```csharp
public class SecurityHeadersMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        await _next(context);
    }
}
```

---

## 9. Tests — Backend et Frontend

### Tests backend (xUnit + Moq + FluentAssertions)

| Fichier                 | Tests                                                | Couverture                  |
| ----------------------- | ---------------------------------------------------- | --------------------------- |
| **UserServiceTests.cs** | 9 tests                                              | Service métier complet      |
|                         | `GetAllAsync_ReturnsMappedUsers_WithoutPasswordHash` | Mapping DTO, exclusion hash |
|                         | `GetByIdAsync_ReturnsUser_WhenFound`                 | Utilisateur trouvé          |
|                         | `GetByIdAsync_ReturnsNull_WhenNotFound`              | Utilisateur non trouvé      |
|                         | `CreateAsync_HashesPassword_AndReturnsUser`          | Hashage BCrypt              |
|                         | `CreateAsync_ThrowsException_WhenEmailExists`        | Unicité email               |
|                         | `UpdateAsync_UpdatesFields_AndRehashesPassword`      | Mise à jour complète        |
|                         | `UpdateAsync_DoesNotRehash_WhenPasswordEmpty`        | Pas de rehash si vide       |
|                         | `DeleteAsync_SetsStatusToDeleted`                    | Soft delete                 |
|                         | `DeleteAsync_ReturnsFalse_WhenNotFound`              | Utilisateur inexistant      |

| Fichier                    | Tests                               | Couverture               |
| -------------------------- | ----------------------------------- | ------------------------ |
| **UserControllerTests.cs** | 9 tests                             | API REST complète        |
|                            | `GetAll_Returns200_WithUsers`       | GET /users → 200         |
|                            | `GetById_Returns200_WhenFound`      | GET /users/{id} → 200    |
|                            | `GetById_Returns404_WhenNotFound`   | GET /users/{id} → 404    |
|                            | `Create_Returns201_WithUser`        | POST /users → 201        |
|                            | `Create_Returns409_WhenEmailExists` | POST /users → 409        |
|                            | `Update_Returns204_WhenSuccessful`  | PUT /users/{id} → 204    |
|                            | `Update_Returns404_WhenNotFound`    | PUT /users/{id} → 404    |
|                            | `Delete_Returns204_WhenSuccessful`  | DELETE /users/{id} → 204 |
|                            | `Delete_Returns404_WhenNotFound`    | DELETE /users/{id} → 404 |

### Tests frontend (Vitest)

| Fichier                  | Tests                                     | Couverture              |
| ------------------------ | ----------------------------------------- | ----------------------- |
| **user.service.spec.ts** | 6 tests                                   | Service Angular complet |
|                          | `should populate users signal on success` | État réactif succès     |
|                          | `should set error on failure`             | Gestion d'erreur        |
|                          | `should count admin users reactively`     | Computed signal         |
|                          | `should add user to list on create`       | Création avec ajout     |
|                          | `should remove user from list on delete`  | Suppression de liste    |
|                          | `should update user in list`              | Mise à jour en place    |

| Fichier                         | Tests                              | Couverture             |
| ------------------------------- | ---------------------------------- | ---------------------- |
| **user-list.component.spec.ts** | 5 tests                            | Composant liste        |
|                                 | `should show loading state`        | État chargement        |
|                                 | `should show error message`        | Affichage erreur       |
|                                 | `should render user table`         | Rendu tableau          |
|                                 | `should call delete on confirm`    | Suppression confirmée  |
|                                 | `should not call delete on cancel` | Annulation suppression |

| Fichier                         | Tests                                      | Couverture           |
| ------------------------------- | ------------------------------------------ | -------------------- |
| **user-form.component.spec.ts** | 6 tests                                    | Composant formulaire |
|                                 | `should create in create mode`             | Mode création        |
|                                 | `should require password in create mode`   | Validation création  |
|                                 | `should edit in edit mode`                 | Mode édition         |
|                                 | `should not require password in edit mode` | Validation édition   |
|                                 | `should submit and navigate on success`    | Soumission réussie   |
|                                 | `should navigate on cancel`                | Annulation           |

| Fichier         | Tests                 | Couverture    |
| --------------- | --------------------- | ------------- |
| **app.spec.ts** | 1 test                | Application   |
|                 | `should render title` | Titre corrigé |

---

## 10. Docker — Hardening et sécurité

### Configuration de sécurité par service

| Mesure                   | Backend           | Frontend                 | Base de données                       |
| ------------------------ | ----------------- | ------------------------ | ------------------------------------- |
| **Utilisateur non-root** | `appuser:1001`    | `nginx:101`              | `mysql:999`                           |
| **no-new-privileges**    | ✅                | ✅                       | ✅                                    |
| **cap_drop: ALL**        | ✅                | ✅                       | ✅                                    |
| **cap_add**              | -                 | -                        | `CHOWN, SETGID, SETUID, DAC_OVERRIDE` |
| **read_only**            | -                 | ✅                       | -                                     |
| **tmpfs**                | `/tmp`            | `/tmp, /var/cache/nginx` | -                                     |
| **Limites ressources**   | 256M RAM, 0.5 CPU | 64M RAM, 0.25 CPU        | 512M RAM, 0.5 CPU                     |
| **HEALTHCHECK**          | ✅ (curl /health) | ✅ (wget /)              | ✅ (mysqladmin)                       |

### Dockerfile sécurisés

#### Backend (multi-stage build)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Build stage

FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Runtime stage avec utilisateur non-root
RUN addgroup --system --gid 1001 appgroup \
    && adduser --system --uid 1001 --gid 1001 --no-create-home appuser
USER appuser
```

#### Frontend (nginx unprivileged)

```dockerfile
FROM nginxinc/nginx-unprivileged:alpine
# Utilise nginx:101 (non-root par défaut)
COPY nginx.conf /etc/nginx/conf.d/default.conf
```

### Configuration nginx sécurisée

```nginx
server {
    listen 8080;
    server_tokens off;  # Masque la version nginx

    # Security headers
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-Frame-Options "DENY" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header Referrer-Policy "no-referrer" always;
    add_header Permissions-Policy "camera=(), microphone=(), geolocation=()" always;
    add_header Content-Security-Policy "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self';" always;

    # API proxy
    location /api {
        proxy_pass http://back:5000;
        # Headers de sécurité proxy
    }
}
```

---

## 11. Configuration et variables d'environnement

### Variables Docker (.env)

```bash
# Base de données
DB_NAME=taskmanager
DB_USER=taskmanager_user
DB_PASSWORD=<mot_de_passe_fort>
DB_ROOT_PASSWORD=<mot_de_passe_root_fort>

# CORS (origines autorisées)
CORS_ALLOWED_ORIGINS=http://localhost:8080,http://127.0.0.1:8080

# Environnement
COMPOSE_PROJECT_NAME=taskmanager-devsecops
```

### Variables d'application

#### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Default": "Server=db;Port=3306;Database=taskmanager;User=taskmanager_user;Password=${DB_PASSWORD};"
  },
  "Cors": {
    "AllowedOrigins": "${CORS_ALLOWED_ORIGINS}"
  }
}
```

#### Frontend (environment.ts)

```typescript
export const environment = {
  production: false,
  apiUrl: "/api", // Via proxy nginx
};
```

### Gestion des secrets

- **Ansible Vault** : Secrets chiffrés dans `ansible/inventory/group_vars/vault.yml`
- **Variables d'environnement** : Pas de secrets en dur dans le code
- **Fichier .env** : Non commité (présent dans .gitignore)
- **.env.example** : Template commité pour référence

---

## 12. Ce qui a été fait

### ✅ Fonctionnalités implémentées

- **Backend .NET 8** : API REST complète avec EF Core
- **Frontend Angular 21** : Interface moderne avec signaux réactifs
- **Base de données** : Schéma relationnel avec migrations
- **Sécurité OWASP** : Contrôles A01, A02, A03, A05, A07
- **Conteneurisation** : Docker sécurisé avec hardening
- **Tests complets** : 25+ tests backend + frontend

### ✅ Tests backend (xUnit + Moq + FluentAssertions)

| Fichier                    | Tests                                                                                                                                                 |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| **UserServiceTests.cs**    | 9 tests : GetAll, GetById (trouvé/non trouvé), Create (hash password, email duplon), Update (champs, rehash, password vide), Delete (soft/non trouvé) |
| **UserControllerTests.cs** | 9 tests : 200/201/204/404/409 pour chaque endpoint                                                                                                    |

### ✅ Tests Angular (Vitest)

| Fichier                         | Tests                                                                                  |
| ------------------------------- | -------------------------------------------------------------------------------------- |
| **user.service.spec.ts**        | loadAll (succès/erreur/loading), adminCount computed, create, remove, getById, update  |
| **user-list.component.spec.ts** | loading/error states, rendu tableau, delete avec confirm/annul                         |
| **user-form.component.spec.ts** | create mode (password requis), edit mode (pre-fill, password optionnel), submit/cancel |
| **app.spec.ts**                 | Tests corrigés (plus de référence à 'Hello, taskmanager-front')                        |

### ✅ OWASP Top 10

| Contrôle                                | Implémentation                                                                                  |
| --------------------------------------- | ----------------------------------------------------------------------------------------------- |
| **A01 Broken Access Control**           | Duplicate email → 409 Conflict                                                                  |
| **A02 Cryptographic Failures**          | Secrets dans .env hors du code, .env.example committed                                          |
| **A03 Injection**                       | EF Core parameterized (déjà fait), DataAnnotations déjà présents                                |
| **A05 Security Misconfiguration**       | SecurityHeadersMiddleware, Swagger désactivé en prod, no stack traces en prod, no Server header |
| **A07 Identification and Auth Failure** | Rate limiting 60 req/min/IP, exception handler propre                                           |

### ✅ Docker Bench / Hardening containers

| Mesure                     | Détail                                                                            |
| -------------------------- | --------------------------------------------------------------------------------- |
| **Non-root users**         | Back: appuser uid 1001 ; Front: nginxinc/nginx-unprivileged uid 101               |
| **no-new-privileges:true** | Sur tous les containers                                                           |
| **cap_drop: ALL**          | + cap_add minimal pour MariaDB uniquement                                         |
| **read_only: true**        | Container front avec tmpfs pour /tmp et /var/cache/nginx                          |
| **Resource limits**        | memory + CPU sur chaque service                                                   |
| **HEALTHCHECK**            | Dans chaque Dockerfile ET docker-compose                                          |
| **server_tokens off**      | nginx ne divulgue plus sa version                                                 |
| **Security headers nginx** | X-Content-Type-Options, X-Frame-Options, CSP, Referrer-Policy, Permissions-Policy |

---

_Documentation générée le 30 avril 2026 - Projet final Bachelor Cybersécurité DevSecOps_
