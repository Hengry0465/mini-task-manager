# Architecture and Design Decisions

This document records the key technical decisions made while building the Mini Task Manager application, along with the reasoning behind them and known trade-offs.

## 1. .NET Version: .NET 10 instead of .NET 8

The assessment brief did not pin a specific .NET version. .NET 8 (released November 2023) reaches end of support in November 2026, only weeks away at the time of development. .NET 10 is the current Long-Term Support (LTS) release (November 2025, supported until November 2028). Building on .NET 10 avoids starting a new project on a framework version about to go out of support.

## 2. Backend Architecture: Controller → Service → Repository

The backend follows a three-layer architecture:

- **Controllers** handle HTTP concerns only (routing, request/response shaping, status codes).
- **Services** contain business logic (e.g. JWT generation, task status toggling).
- **Repositories** encapsulate all direct EF Core / database access.

This separation keeps each layer independently testable and makes it straightforward to swap the data access layer (e.g. for unit testing with an in-memory provider) without touching business logic.

## 3. Authentication: Custom JWT Implementation

JWT authentication was implemented manually (using `System.IdentityModel.Tokens.Jwt` and `Microsoft.AspNetCore.Authentication.JwtBearer`) rather than using ASP.NET Core Identity. Given the assessment explicitly allows a hardcoded user for evaluation purposes, a full Identity framework (with its own user store, role management, and additional tables) would have been disproportionate to the scope of the task. A lightweight, purpose-built implementation keeps the codebase easy to review.

## 4. Password Hashing: PBKDF2 (Built into .NET)

Passwords are hashed using PBKDF2 via `Rfc2898DeriveBytes`, a class built into the .NET base class library, rather than a third-party package such as BCrypt.Net. This avoids adding an extra dependency for a single hashing operation, while still following the standard practice of a per-password random salt and a high iteration count (100,000).

## 5. Enum Serialization: String-Based JSON

By default, .NET serializes enums (such as `TaskPriority` and `TaskStatus`) as integers in JSON. Since the assessment specifies priority values as `Low / Medium / High` and status values as `Todo / Done`, a `JsonStringEnumConverter` was registered globally so that both requests and responses use readable string values instead of numeric indices. This also makes the API more self-documenting and easier for the frontend to consume without a separate mapping layer.

## 6. Global Exception Handling Middleware

A custom `ExceptionHandlingMiddleware` wraps the entire request pipeline and converts unhandled exceptions into a consistent JSON error shape (`status`, `title`, `message`, `traceId`). Internal exception details and stack traces are never returned to the client — only a generic message is shown for unexpected (500-level) errors, while the actual exception is logged server-side. This prevents leaking implementation details through error responses, which is a standard security practice.

## 7. Swagger: Development-Only by Default

Swagger UI is only registered when the application runs in the `Development` environment. Exposing interactive API documentation in a production environment is generally avoided, as it can reveal the full API surface to unauthenticated parties. Since Docker Compose runs the backend in `Production` mode by default, Swagger must be temporarily enabled via an environment variable change for local testing (documented in `README.md`).

## 8. Database Migrations Applied Automatically on Startup

`Program.cs` calls `db.Database.MigrateAsync()` on application startup, so the container automatically brings the database schema up to date without requiring a manual migration step. This satisfies the requirement that `docker compose up --build` starts a fully working system with no additional manual commands. A separate `DesignTimeDbContextFactory` is provided so that the EF Core CLI can still generate new migrations locally without needing to execute the application's full startup pipeline (including JWT configuration, which would otherwise require environment variables to be present at design time).

## 9. Kubernetes Storage: emptyDir instead of PersistentVolumeClaim

The PostgreSQL Deployment in `k8s/db-deployment.yaml` uses an `emptyDir` volume rather than a `PersistentVolumeClaim`. This was a deliberate simplification for the scope of this assessment: `emptyDir` requires no cluster-specific storage class configuration and works out of the box on any local cluster (e.g. Docker Desktop's built-in Kubernetes). The trade-off is that data does not survive a pod restart. In a real production deployment, this would be replaced with a `PersistentVolumeClaim` backed by a durable storage class.

## 10. Kubernetes Secrets: Base64 Encoding Only

The `k8s/secret.yaml` manifest uses standard Kubernetes `Secret` objects, which store values as base64-encoded strings. Base64 encoding is not encryption — it is trivially reversible — and this is a known limitation of the vanilla Kubernetes Secret resource. In a production environment, this would typically be replaced with a solution such as Sealed Secrets, HashiCorp Vault, or a cloud provider's native secret manager (e.g. AWS Secrets Manager, Azure Key Vault) to ensure secrets are encrypted at rest and access-controlled.

## 11. Frontend Routing to Backend: nginx Reverse Proxy

In the Docker Compose and Kubernetes setups, the frontend's nginx server proxies all `/api` requests to the backend service directly (`proxy_pass http://backend:8080` in Docker Compose; via Ingress path routing in Kubernetes). This means the browser only ever talks to a single origin, avoiding the need for CORS configuration in the production build and keeping the backend's internal network address hidden from the client. CORS is only configured on the backend for local development, where the Angular dev server (`ng serve`, port 4200) calls the backend directly (port 5069 or 5000).

## 12. Angular: Standalone Components, No NgModules

The frontend uses Angular 18's standalone component API throughout, rather than the traditional NgModule-based structure. This is the current recommended approach for new Angular projects, reduces boilerplate, and keeps each feature (login, task list, task form) self-contained with its own explicit imports.

## 13. State Management: Angular Signals, No External Library

Component-level state (task list, loading state, form visibility) is managed using Angular's built-in `signal()` API rather than a third-party state management library such as NgRx. Given the application's scope — a single authenticated feature area with straightforward CRUD operations — a dedicated state management library would add unnecessary complexity without a corresponding benefit.

## 14. Task Form: Modal Overlay Instead of Separate Route

Task creation and editing are handled by a single reusable `TaskFormComponent`, rendered as a modal overlay within the task list page rather than as a separate routed page. This avoids a full page navigation for what is a short-lived, focused interaction, and keeps the task list visible in the background for context.

## Known Limitations

- Refresh tokens are not implemented; JWT tokens simply expire after the configured duration (`Jwt__ExpiryMinutes`), after which the user must log in again.
- The seeded admin account is the only user in the system; there is no registration, multi-user support, or role-based access control, consistent with the assessment's scope.
- The `emptyDir` volume and base64-only Kubernetes secrets (see decisions 9 and 10 above) are simplifications appropriate for an evaluation environment, not a production deployment.