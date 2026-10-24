# Shop System API

A production-ready e-commerce backend API built with **ASP.NET Core 10**, **SQL Server**, and **Docker**. Designed as an educational project with enterprise-grade features including JWT authentication, role-based authorization, request auditing, rate limiting, and comprehensive test coverage.

---

## 🎯 Overview

Shop System API provides a complete backend for managing:
- **Authentication & Authorization** — User registration, login, JWT tokens, role-based access control
- **Products & Categories** — Full CRUD operations with inventory management
- **Orders** — Order creation, tracking, cancellation with idempotency support
- **Auditing** — Request/response logging for compliance and debugging
- **Rate Limiting** — Protection against abuse with configurable policies per endpoint

### Key Features

✅ **Security First**
- JWT access tokens (30 min default) + rotating refresh tokens (7 days)
- BCrypt password hashing with salt
- SHA-256 refresh-token rotation and reuse detection
- CORS with explicit origin whitelisting
- Idempotent order creation with payload validation

✅ **Production-Ready**
- Comprehensive error handling and validation (FluentValidation)
- Request correlation IDs for tracing
- Background auditing service (non-blocking writes)
- Health checks (`/health/live`, `/health/ready`)
- SQL Server integration tests and unit tests (xUnit + Moq)
- GitHub Actions CI/CD with code coverage reports

✅ **Developer-Friendly**
- Swagger/OpenAPI documentation (Development mode)
- Structured logging with JSON output
- Docker Compose for local development
- User Secrets for secure local configuration
- RESTful API conventions with 201 Created responses

---

## 📋 Technology Stack

| Component | Tech | Version |
|-----------|------|---------|
| **Language** | C# | .NET 10 |
| **Web Framework** | ASP.NET Core | 10.0 |
| **Database** | SQL Server | 2025+** (prod) | (Developer edition for local development; use an appropriately licensed edition in production) |
| **Data Access** | ADO.NET | Native SQL + Stored Procedures |
| **Authentication** | JWT Bearer | HS256 |
| **Validation** | FluentValidation | 11.3.1 |
| **Password Hashing** | BCrypt.Net-Next | 4.2.0 |
| **Testing** | xUnit + Moq | Latest |
| **Containerization** | Docker & Compose | Latest |
| **CI/CD** | GitHub Actions | Build, Test, Coverage |

---

## 🚀 Quick Start

### Prerequisites

- **Docker Desktop** (with Linux containers enabled)
- **Git**
- **.NET 10 SDK** (for local development)

### 1️⃣ Clone & Setup

```bash
# Clone the repository
git clone <repository-url>
cd Shop-System

# Copy environment template
cp .env.example .env

# Edit .env with strong local passwords
# - MSSQL_SA_PASSWORD: (min 8 chars, uppercase, lowercase, number, special char)
# - JWT_KEY: (random 32+ bytes, e.g., use: openssl rand -base64 32)
```

### 2️⃣ Run with Docker

```bash
# Start all services (SQL Server, API)
docker compose up --build -d

# Check status
docker compose ps

# View logs
docker compose logs -f api
```

Services will be available at:
- **API**: `http://localhost:8080`
- **SQL Server**: `localhost:1433` (sa user)
- **Swagger**: `http://localhost:8080/swagger` (Development mode)
- **Health Check**: `http://localhost:8080/health/ready`

### 3️⃣ Stop Services

```bash
docker compose down        # Stop containers, keep volumes
docker compose down -v    # Stop & delete everything (fresh start)
```

---

## 🔧 Local Development (Without Docker)

### Setup

```bash
# Restore NuGet packages
dotnet restore

# Initialize user secrets (one-time)
dotnet user-secrets init

# Set required secrets
dotnet user-secrets set "ConnectionStrings:ShopDbConnection" \
  "Server=localhost;Database=ShopDB;Trusted_Connection=True;TrustServerCertificate=True;"

dotnet user-secrets set "JwtSettings:Key" "<generate-random-32-bytes>"
```

### Run

```bash
# Start the API (runs on http://localhost:5235)
dotnet run

# Access Swagger
open http://localhost:5235/swagger
```

---

## 📚 API Endpoints

All endpoints use `/api` prefix. Authentication required unless noted.

### Authentication (Public)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/Auth/Register` | Create new account |
| `POST` | `/api/Auth/Login` | Authenticate and get tokens |
| `POST` | `/api/Auth/RefreshToken` | Refresh expired access token |
| `POST` | `/api/Auth/Logout` | Invalidate refresh token |

### Products

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/Product/paged` | No | List products (paginated) |
| `GET` | `/api/Product/{id}` | No | Get product details |
| `POST` | `/api/Product` | Admin | Create product |
| `PUT` | `/api/Product/{id}` | Admin | Update product |
| `DELETE` | `/api/Product/{id}` | Admin | Delete product |

### Orders

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/api/Order` | Yes | Create order (idempotent) |
| `GET` | `/api/Order/mine/paged` | Yes | List my orders |
| `GET` | `/api/Order/{id}` | Owner/Admin | Get order details |
| `PUT` | `/api/Order/{id}/cancel` | Owner/Admin | Cancel order |

### People (Admin Only)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `GET` | `/api/Person/{id}` | Yes | Get person profile |
| `PUT` | `/api/Person/{id}` | Owner/Admin | Update profile |
| `POST` | `/api/Person/{id}/ban` | Admin | Ban user |

### Health Checks

| Endpoint | Purpose |
|----------|---------|
| `GET /health/live` | Liveness probe (is API running?) |
| `GET /health/ready` | Readiness probe (can API connect to DB?) |

### Example: Login & Create Order

```bash
# 1. Register
curl -X POST http://localhost:8080/api/Auth/Register \
  -H "Content-Type: application/json" \
  -d '{
    "personName": "John Doe",
    "phoneNumber": "0912345678",
    "address": "Damascus",
    "password": "MySecurePass123"
  }'

# 2. Login (save the access token)
TOKEN=$(curl -X POST http://localhost:8080/api/Auth/Login \
  -H "Content-Type: application/json" \
  -d '{
    "phoneNumber": "0912345678",
    "password": "MySecurePass123"
  }' | jq -r '.data')

# 3. Create Order (with idempotency key)
curl -X POST http://localhost:8080/api/Order \
  -H "Authorization: Bearer $TOKEN" \
  -H "Idempotency-Key: order-unique-123" \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      { "productId": 1, "quantity": 2 }
    ]
  }'
```

---

## 🗄️ Database

### Schema & Initialization

The database is auto-initialized on first Docker Compose run via `Database/init-db.sh`:

1. Waits for SQL Server to accept connections
2. Checks if `ShopDB` exists and is initialized
3. Runs `Database/ShopDB.Docker.sql` if new
4. Records initialization marker to prevent re-runs

**Schemas:**
- `Persons` — Users with roles (Customer, Admin)
- `Products` — Catalog with inventory
- `Categories` — Product grouping
- `Orders` — Order headers
- `OrderItems` — Order line items
- `RefreshTokens` — Token rotation storage
- `InventoryLog` — Audit trail for stock changes
- `AuditLog` — API request/response logging
- **14 tables** | **20+ stored procedures**

### Local Development Database

If using SQL Server locally (not Docker):

1. Restore `Database/ShopDB.Full.sql`
2. Update `ConnectionStrings:ShopDbConnection` in user secrets

---

## 🔐 Configuration & Security

### Environment Variables (`.env`)

| Variable | Required | Example |
|----------|----------|---------|
| `MSSQL_SA_PASSWORD` | ✅ | `SecureP@ss123!` |
| `JWT_KEY` | ✅ | `base64-random-32-bytes` |
| `CORS_ORIGIN` | ✅ | `http://localhost:3000` |
| `TRUSTED_PROXY_IP` | No | `10.0.0.1` (if behind proxy) |

### Security Best Practices

⚠️ **DO NOT commit `.env`** — `.gitignore` excludes it by default

✅ **Production Checklist:**
- Use **HTTPS only** (enable TLS in reverse proxy)
- Set **strong passwords** (≥15 chars, mixed case, numbers, symbols)
- Rotate **JWT_KEY** periodically
- Use **environment variables** or **Azure Key Vault** for secrets (never in code)
- Enable **CORS** only for trusted origins
- Set **rate limiting** policies based on expected load
- Configure **SQL Server authentication** with strong SA password
- Use **read-only database replicas** for reporting
- Monitor **AuditLog** for suspicious activity

---

## 🧪 Testing

### Run Tests Locally

```bash
# Restore dependencies
dotnet restore

# Run all tests
dotnet test ShopAPI.slnx

# Run specific test project
dotnet test ShopAPI.UnitTests --logger "console;verbosity=minimal"
dotnet test ShopAPI.IntegrationTests

# With coverage
dotnet test ShopAPI.slnx /p:CollectCoverage=true
```

### Test Projects

- **ShopAPI.UnitTests** — Business logic, validation, service layers
- **ShopAPI.IntegrationTests** — SQL Server end-to-end flows

### CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/build.yml`):
- ✅ Restore dependencies
- ✅ Build Release configuration
- ✅ Run all tests
- ✅ Collect code coverage
- ✅ Upload test results as artifacts

---

## 🏗️ Project Structure

```
ShopAPI/
├── Controllers/          # HTTP endpoints (Auth, Product, Order, Person, Category)
├── Services/             # Business logic (Auth, Order, Product, etc.)
├── Repositories/         # Data access layer (SQL + stored procedures)
├── DTOClasses/          # Request/response data transfer objects
├── FluentValidation/    # Input validation rules
├── Authorization/       # Policies, handlers, requirements
├── Middlewares/         # Correlation ID, exception handling, telemetry
├── Observability/       # Auditing, health checks, logging
├── Infrastructure/      # Database connection factory
├── Settings/            # Configuration classes (JwtSettings, etc.)
├── Response/            # Standardized API response wrapper
├── Extensions/          # Helper methods
├── Database/            # SQL scripts (schema, stored procedures)
├── docker/              # Dockerfile, docker-compose, init scripts
├── .github/workflows/   # CI/CD (GitHub Actions)
├── ShopAPI.UnitTests/   # Unit & integration tests
└── Program.cs           # Dependency injection, middleware, startup
```

---

## 🐛 Troubleshooting

### Docker Connection Issue
```
Error: dial tcp: lookup mcr.microsoft.com: no such host
```
**Solution:** Check internet connection and DNS. Ensure Docker Desktop is running.

### Database Connection Failed
```
Error: Cannot open database "ShopDB" requested by the login
```
**Solution:** 
- Verify `MSSQL_SA_PASSWORD` is set in `.env`
- Ensure SQL Server is healthy: `docker compose logs db`
- Check `ConnectionStrings__ShopDbConnection` in compose.yaml

### JWT Configuration Error
```
Error: JwtSettings:Key must be at least 32 bytes long
```
**Solution:** Generate a proper key and update `.env`:
```bash
# Linux/Mac
openssl rand -base64 32

# Windows PowerShell
[Convert]::ToBase64String((1..32 | % { Get-Random -Maximum 256 })) 
```

### CORS Error in Browser
```
Error: Access to XMLHttpRequest blocked by CORS policy
```
**Solution:** Add your frontend URL to `CORS_ORIGIN` in `.env`:
```
CORS_ORIGIN=http://localhost:3000
```

---

## 📖 Documentation Files

- **README.md** — This file
- **CHANGELOG.md** — Version history and fixes
- **.env.example** — Environment variable template
- **ShopAPI.http** — REST client file (VS Code REST Client extension)

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Standards

- Use **C# naming conventions** (PascalCase for public members)
- Write **unit tests** for new services
- Add **integration tests** for database operations
- Follow **SOLID principles**
- Add **XML comments** for public APIs
- Ensure tests pass locally before PR

---

## 📄 License

This project is for educational purposes. Refer to LICENSE file for details.

---

## 📞 Support

For issues, questions, or suggestions:
1. Check existing **GitHub Issues**
2. Review **CHANGELOG.md** for recent changes
3. Verify **troubleshooting section** above
4. Check Docker logs: `docker compose logs -f`

---

## 🎓 Learning Resources

This project demonstrates:
- RESTful API design with proper HTTP semantics
- Asynchronous programming patterns in C#
- Dependency injection and service-oriented architecture
- SQL Server integration with stored procedures
- JWT authentication and authorization
- Docker containerization best practices
- Unit and integration testing strategies
- CI/CD with GitHub Actions

---

**Built with ❤️ using ASP.NET Core 10 and SQL Server**
