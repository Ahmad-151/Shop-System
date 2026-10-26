# Shop System API

An e-commerce backend API built with **ASP.NET Core 10**, **SQL Server** (ADO.NET + stored procedures) and **Docker**.

Built as a portfolio project to practice secure, testable API design: JWT authentication with refresh-token rotation, role- and ownership-based authorization, idempotent order creation, request auditing, rate limiting, health checks, and unit + integration tests.

> The Docker setup is meant for **local demo and development**. Read [Production Notes](#-production-notes) before deploying anywhere real.

<!-- Add a Swagger screenshot or a short GIF of the Quick Start here -->

---

## 🚀 Quick Start (Docker)

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and **running** (Linux containers)
- At least **2 GB of RAM** available to Docker (required by SQL Server)
- Ports **8080** and **1433** free on your machine
- Git

### Run it

**Windows (PowerShell):**

```powershell
git clone https://github.com/Ahmad-151/Shop-System.git
cd Shop-System
copy .env.example .env
docker compose up --build -d
```

**macOS / Linux:**

```bash
git clone https://github.com/Ahmad-151/Shop-System.git
cd Shop-System
cp .env.example .env
docker compose up --build -d
```

The first run downloads the SQL Server and .NET images, so it can take several minutes. Later runs start in about a minute.

Check that everything is up:

```bash
docker compose ps -a
```

Expected: `db` and `api` are **Up**, and `db-init` is **Exited (0)** (it is a one-time job that creates the database).

### Try the API

1. Open **http://localhost:8080/swagger**
2. Register a user, then log in to get an access token.
3. Click **Authorize** and paste the token.
4. Create an order and explore the other endpoints.

Health checks: `http://localhost:8080/health/live` and `http://localhost:8080/health/ready`.

### Stop and clean up

```bash
docker compose down        # stop containers, keep the database volume
docker compose down -v     # stop and delete everything (fresh start)
```

> ⚠️ **Local demo only.** `.env.example` ships with demo credentials so the project runs out of the box. Never reuse them in a real deployment.

---

## ✨ Features

**Security**
- JWT access tokens + rotating refresh tokens, with reuse detection
- BCrypt password hashing
- Role-based and ownership-based authorization (customer / admin)
- Explicit CORS origin whitelisting
- Idempotent order creation (`Idempotency-Key` header)
- Rate limiting with per-endpoint policies

**Reliability and observability**
- Centralized exception handling and input validation (FluentValidation)
- Request correlation IDs
- Background audit logging (non-blocking writes)
- Liveness and readiness health checks
- Unit tests (xUnit + Moq) and SQL Server integration tests
- GitHub Actions workflow for build and tests

**Developer experience**
- Swagger / OpenAPI (enabled in the Development environment)
- One-command Docker Compose setup
- User Secrets support for local configuration without Docker

---

## 📋 Technology Stack

| Component | Technology |
|-----------|------------|
| Language / Framework | C# on .NET 10, ASP.NET Core 10 |
| Database | SQL Server 2025 (Developer edition for local use; use a properly licensed edition in production) |
| Data access | ADO.NET with stored procedures |
| Authentication | JWT Bearer (HS256) |
| Validation | FluentValidation |
| Password hashing | BCrypt.Net-Next |
| Testing | xUnit, Moq |
| Containers | Docker, Docker Compose |
| CI | GitHub Actions |

---

## 📚 API Endpoints

All routes use the `/api` prefix.

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/Auth/Register` | Create a new account |
| `POST` | `/api/Auth/Login` | Authenticate and receive tokens |
| `POST` | `/api/Auth/RefreshToken` | Exchange a refresh token for new tokens |
| `POST` | `/api/Auth/Logout` | Invalidate the refresh token |

### Products

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/Product/paged` | Public | List products (paginated) |
| `GET` | `/api/Product/{id}` | Public | Get product details |
| `POST` | `/api/Product` | Admin | Create a product |
| `PUT` | `/api/Product/{id}` | Admin | Update a product |
| `DELETE` | `/api/Product/{id}` | Admin | Delete a product |

### Orders

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/Order` | User | Create an order (idempotent) |
| `GET` | `/api/Order/mine/paged` | User | List my orders |
| `GET` | `/api/Order/{id}` | Owner / Admin | Get order details |
| `PUT` | `/api/Order/{id}/cancel` | Owner / Admin | Cancel an order |

### People

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/Person/{id}` | User | Get a person profile |
| `PUT` | `/api/Person/{id}` | Owner / Admin | Update a profile |
| `POST` | `/api/Person/{id}/ban` | Admin | Ban a user |

A ready-to-use request collection is available in [`ShopAPI.http`](ShopAPI.http) (VS Code REST Client extension).

---

## 🗄️ Database

With Docker, the database is created automatically on first run by the `db-init` service (`Database/init-db.sh`), which:

1. Waits for SQL Server to accept connections
2. Checks whether `ShopDB` already exists and is initialized
3. Runs the schema script if it is new
4. Records an initialization marker so it never runs twice

The scripts live in the [`Database/`](Database) folder.

---

## 🔐 Configuration

### Environment variables (`.env`)

| Variable | Description |
|----------|-------------|
| `MSSQL_SA_PASSWORD` | SQL Server `sa` password. Must be 8+ characters with at least 3 of: uppercase, lowercase, digits, symbols. Avoid `$`, `#` and quotes. |
| `JWT_KEY` | Secret used to sign JWTs. Use at least 32 random bytes. |

Generate a random key:

```bash
# Linux / macOS
openssl rand -base64 32
```

```powershell
# Windows PowerShell
[Convert]::ToBase64String((1..32 | % { Get-Random -Maximum 256 }))
```

CORS allowed origins are configured in `compose.yaml` (`Cors__AllowedOrigins__0`). Change it if your frontend runs on another origin.

> `.env` is excluded by `.gitignore`. Never commit it.

---

## 🔧 Local Development (without Docker)

```bash
dotnet restore

# One-time: store secrets outside the repository
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:ShopDbConnection" "Server=localhost;Database=ShopDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:Key" "<random-32-byte-secret>"

dotnet run
```

You need a local SQL Server with the schema from the [`Database/`](Database) folder applied.

---

## 🧪 Testing

```bash
dotnet test ShopAPI.slnx
```

| Project | Scope |
|---------|-------|
| `ShopAPI.UnitTests` | Business logic, validation, services |
| `ShopAPI.IntegrationTests` | End-to-end flows against SQL Server |

A GitHub Actions workflow in [`.github/workflows`](.github/workflows) builds the solution and runs the tests on every push.

---

## 🏗️ Project Structure

```
Shop-System/
├── Authorization/             # Policies, handlers, requirements
├── Controllers/               # HTTP endpoints
├── Database/                  # SQL scripts and init-db.sh
├── DTOClasses/                # Request / response DTOs
├── Extensions/                # Helper extensions
├── FluentValidation/          # Input validators
├── Infrastructure/            # Database connection factory
├── Middlewares/               # Exception handling, correlation IDs
├── Observability/             # Auditing, health checks, logging
├── Repositories/              # Data access (SQL + stored procedures)
├── Response/                  # Standard API response wrapper
├── Services/                  # Business logic
├── Settings/                  # Strongly typed configuration
├── ShopAPI.UnitTests/
├── ShopAPI.IntegrationTests/
├── .github/workflows/         # CI
├── compose.yaml
├── Dockerfile
├── .env.example
└── Program.cs
```

---

## 🛠 Troubleshooting

| Problem | Fix |
|---|---|
| `failed to connect to the docker API` | Docker Desktop is not running. Start it and wait until the engine is running. |
| `no configuration file provided` | Run the commands from inside the `Shop-System` folder. |
| `port is already allocated` | Another container, or a local SQL Server, is using port 1433 or 8080. Run `docker ps`, stop the other container, or change the published port in `compose.yaml`. |
| `db` exits right after starting | The SA password in `.env` is too weak. It needs 8+ characters and at least 3 of: uppercase, lowercase, digits, symbols. Avoid `$`, `#` and quotes. Then run `docker compose down -v` and start again. |
| `db-init` fails with `invalid option name` | `Database/init-db.sh` has Windows (CRLF) line endings. Convert it to LF (`sed -i 's/\r$//' Database/init-db.sh`), then `docker compose down -v` and `up` again. The repository's `.gitattributes` normally prevents this on a fresh clone. |
| `db-init` keeps waiting | SQL Server never became ready. Check `docker compose logs db` for the reason (usually the password). |
| Everything is slow on first run | The first build downloads large images. Later runs are much faster. |

---

## 🔒 Production Notes

The default Docker setup is not production-hardened. Before deploying:

- Generate your own strong SA password and a random JWT key. Never reuse the demo values.
- Run behind HTTPS (reverse proxy) and set `ASPNETCORE_ENVIRONMENT=Production`, which disables Swagger.
- Do not expose port 1433 publicly.
- Keep secrets in environment variables or a secret manager, never in the repository.
- Use a properly licensed SQL Server edition.
- Monitor the audit log for suspicious activity.

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit your changes and push the branch
4. Open a Pull Request

Please add tests for new services and make sure `dotnet test` passes before opening a PR.

---

## 📄 License

MIT License. See [LICENSE](LICENSE).