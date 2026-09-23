# Mini Task Manager

A full-stack task management application built with Angular, ASP.NET Core, PostgreSQL, Docker, and Kubernetes.

## Tech Stack

- **Frontend:** Angular 18 (standalone components, TypeScript)
- **Backend:** ASP.NET Core 10 Web API
- **Database:** PostgreSQL 16
- **Containerization:** Docker, Docker Compose
- **Orchestration:** Kubernetes manifests

## Prerequisites

- Docker Desktop (with Docker Compose)
- (Optional, for local development without Docker) .NET 10 SDK, Node.js 20+, Angular CLI 18

## Quick Start (Docker Compose)

1. Clone the repository:
```bash
   git clone <repository-url>
   cd mini-task-manager
```

2. Copy the example environment file and adjust values if needed:
```bash
   cp .env.example .env
```

3. Build and start all services:
```bash
   docker compose up --build
```

4. Once all containers are running, open the app:
   - **Frontend:** http://localhost:4200
   - **Backend API:** http://localhost:5000/api
   - **Database (for direct inspection):** localhost:5432

5. Log in with the seeded admin account:
   - **Email:** `admin@taskflow.com`
   - **Password:** `Admin@123`

6. Stop all services:
```bash
   docker compose down
```

   To also remove the database volume (full reset):
```bash
   docker compose down -v
```

## Ports and URLs

| Service   | Container Port | Host Port | URL                          |
|-----------|-----------------|-----------|-------------------------------|
| Frontend  | 80               | 4200      | http://localhost:4200        |
| Backend   | 8080             | 5000      | http://localhost:5000/api    |
| Database  | 5432             | 5432      | localhost:5432                |

Swagger UI (API documentation) is only enabled when `ASPNETCORE_ENVIRONMENT=Development`. See [Swagger Access](#swagger-access) below.

## Environment Variables

All environment variables are defined in `.env.example`. Copy this file to `.env` before running Docker Compose. `.env` is excluded from version control via `.gitignore`.

| Variable | Description |
|---|---|
| `POSTGRES_DB` | Database name |
| `POSTGRES_USER` | Database username |
| `POSTGRES_PASSWORD` | Database password |
| `ConnectionStrings__DefaultConnection` | Full EF Core connection string used by the backend |
| `Jwt__Key` | Secret key used to sign JWT tokens (minimum 32 characters) |
| `Jwt__Issuer` | JWT issuer claim |
| `Jwt__Audience` | JWT audience claim |
| `Jwt__ExpiryMinutes` | Token expiry duration in minutes |
| `API_BASE_URL` | Base API URL reference (used for local reference only; the frontend container resolves the API via nginx reverse proxy in production) |

No secrets are hardcoded in source code or in `docker-compose.yml`. All sensitive values are injected via environment variables at runtime.

## Swagger Access

For security, Swagger UI is only enabled when the backend runs in the `Development` environment. By default, Docker Compose runs the backend in `Production` mode, so Swagger is not exposed.

To access Swagger UI while testing locally with Docker:

1. Open `docker-compose.yml`.
2. Change the backend service's environment variable:
```yaml
   ASPNETCORE_ENVIRONMENT: Development
```
3. Rebuild and restart:
```bash
   docker compose down
   docker compose up --build
```
4. Open http://localhost:5000/swagger

Remember to change `ASPNETCORE_ENVIRONMENT` back to `Production` before final submission or deployment.

## Database Migrations

Entity Framework Core migrations are included in the repository under `backend/TaskManager.Api/Migrations/`. Migrations are applied automatically on backend startup (see `Program.cs`), so no manual step is required when running via Docker Compose.

To generate a new migration manually (requires the EF Core CLI tool and a running PostgreSQL instance):

```bash
dotnet tool install --global dotnet-ef   # if not already installed
cd backend/TaskManager.Api
dotnet ef migrations add <MigrationName>
```

A design-time factory (`Data/DesignTimeDbContextFactory.cs`) is included so the EF CLI can generate migrations without needing to run the full application startup pipeline.

To apply migrations manually against a running database:

```bash
dotnet ef database update
```

## Running Locally Without Docker (Development Mode)

### Backend

```bash
# Start only the database container
docker compose up -d db

cd backend/TaskManager.Api
dotnet run
```

The backend will start on the port defined in `Properties/launchSettings.json` (typically `http://localhost:5069`).

### Frontend

```bash
cd frontend/task-manager-client
ng serve
```

The frontend will be available at http://localhost:4200 and will proxy API calls to the URL configured in `src/environments/environment.development.ts`.

## API Endpoint Summary

### Authentication

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| POST | `/api/auth/login` | Log in with email and password, returns a JWT token | No |

### Tasks

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| GET | `/api/tasks` | List all tasks | Yes |
| GET | `/api/tasks/{id}` | Get a single task by ID | Yes |
| POST | `/api/tasks` | Create a new task | Yes |
| PUT | `/api/tasks/{id}` | Update an existing task | Yes |
| PATCH | `/api/tasks/{id}/status` | Toggle task status between Todo and Done | Yes |
| DELETE | `/api/tasks/{id}` | Delete a task | Yes |

### Health

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| GET | `/api/health` | Health check endpoint used by Kubernetes probes | No |

All authenticated endpoints require an `Authorization: Bearer <token>` header, obtained from `POST /api/auth/login`.

## Kubernetes Deployment

Kubernetes manifests are located under `k8s/`:

- `backend-deployment.yaml`, `backend-service.yaml`
- `frontend-deployment.yaml`, `frontend-service.yaml`
- `db-deployment.yaml`, `db-service.yaml`
- `configmap.yaml` — non-sensitive configuration
- `secret.yaml` — sensitive configuration (base64-encoded)
- `ingress.yaml` — optional Ingress routing (`/api` → backend, `/` → frontend)

To deploy (requires a running Kubernetes cluster, e.g. Docker Desktop's built-in Kubernetes):

```bash
kubectl apply -f k8s/
```

The backend also exposes liveness and readiness probes at `/api/health`.

## Project Structure
mini-task-manager/
├── backend/
│ └── TaskManager.Api/
│ ├── Controllers/
│ ├── Services/
│ ├── Repositories/
│ ├── Models/
│ ├── DTOs/
│ ├── Data/
│ ├── Middleware/
│ ├── Validators/
│ ├── Migrations/
│ └── Dockerfile
├── frontend/
│ └── task-manager-client/
│ ├── src/app/
│ │ ├── core/ (services, guards, interceptors, models)
│ │ └── features/ (auth, tasks)
│ ├── nginx.conf
│ └── Dockerfile
├── k8s/
├── docker-compose.yml
├── .env.example
├── README.md
└── DECISIONS.md


## Notes

- The admin account is seeded automatically on backend startup if it does not already exist (see `Data/DbInitializer.cs`).
- CORS is configured to allow requests from `http://localhost:4200` during local development. In the Docker Compose setup, the frontend's nginx server proxies `/api` requests to the backend container directly, so no cross-origin request is made from the browser's perspective.