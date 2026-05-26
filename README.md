# Circle Backend

Circle is a loan operations management backend for digitizing customer records, loan origination, approvals, disbursements, repayments, reconciliation, collections, audit trails, and portfolio reporting for a money lending business.

This repository currently contains the backend API only. It is built with ASP.NET Core on .NET 10, Entity Framework Core, MySQL, JWT authentication, Serilog structured logging, and an onion architecture layout.

## Current Status

The backend foundation is live and deployable.

- Runtime: .NET 10
- API framework: ASP.NET Core Web API
- Database: Azure Database for MySQL Flexible Server
- ORM: Entity Framework Core
- Auth: JWT access tokens, refresh tokens, OTP verification, PBKDF2 password hashing
- Logging: Serilog console logs with correlation IDs
- Documentation/testing surface: Swagger
- Deployment: GitHub Actions to Azure App Service
- Live Swagger: https://circle-h3fqbbf2e2bhbsdz.southafricanorth-01.azurewebsites.net/swagger/index.html
- Live health check: https://circle-h3fqbbf2e2bhbsdz.southafricanorth-01.azurewebsites.net/health

## Product Decisions Implemented

The backend reflects the Phase 1 MVP decisions from the PRD.

- Payment gateway: Monnify
- Interest method: Flat rate
- Auto approval threshold: NGN 20,000
- Repayment frequencies: Daily and Weekly
- Bootstrap flow: first admin can be created only when no user exists
- Customer passbook continuity: customer statements and repayment schedules are preserved digitally

## Architecture

Circle uses onion architecture so the business rules stay independent from infrastructure and presentation concerns.

```text
Circle
|-- Circle.Domain
|   |-- Entities
|   |-- Enums
|   `-- Common
|
|-- Circle.Application
|   |-- Auth
|   |-- Users
|   |-- Customers
|   |-- CustomerDocuments
|   |-- VirtualAccounts
|   |-- Loans
|   |-- Disbursements
|   |-- Repayments
|   |-- Reconciliation
|   |-- Webhooks
|   |-- Collections
|   |-- Reports
|   |-- Dashboard
|   |-- Search
|   |-- Administration
|   `-- Abstractions
|
|-- Circle.Persistence
|   |-- Configurations
|   |-- Migrations
|   |-- Services
|   `-- CircleDbContext.cs
|
`-- Circle.WebApi
    |-- Controllers
    |-- Middleware
    |-- Services
    `-- Program.cs
```

### Project Responsibilities

`Circle.Domain`

Contains the core model of the business. This project should not depend on ASP.NET Core, EF Core, Azure, MySQL, or any delivery mechanism.

Main contents:

- Customer, User, Loan, Repayment, Disbursement, Virtual Account, Collection Activity, Audit Log, and Webhook entities
- Domain enums such as `LoanStatus`, `RepaymentFrequency`, `UserRole`, `CustomerStatus`, and `PaymentWebhookStatus`
- Shared entity base class

`Circle.Application`

Contains use cases, service contracts, DTOs, validation/business rules, and application-level orchestration. Controllers call this layer instead of talking directly to EF Core.

Main responsibilities:

- Authentication and OTP flow
- User and credit officer management
- Customer registration and customer statements
- Loan creation, approval, rejection, and repayment schedule generation
- Disbursement recording
- Repayment allocation
- Inbound payment reconciliation
- Monnify webhook processing
- Collection worklists and overdue refresh
- Dashboard, reports, search, admin settings, and audit log access

`Circle.Persistence`

Contains infrastructure implementation details.

Main responsibilities:

- EF Core `CircleDbContext`
- MySQL entity configurations
- Database migrations
- Application database abstraction implementation
- PBKDF2 password hashing
- Local file storage implementation
- Console email sender used during the current phase
- System date/time provider

`Circle.WebApi`

Contains the HTTP API boundary.

Main responsibilities:

- API controllers
- JWT authentication setup
- Swagger/OpenAPI setup
- Serilog request logging
- Correlation ID middleware
- Central exception handling
- Health endpoints
- Azure-friendly forwarded header support
- Response compression

## Backend Modules

### Authentication

Endpoints:

- `POST /api/auth/bootstrap-admin`
- `POST /api/auth/login`
- `POST /api/auth/verify-otp`
- `GET /api/auth/me`

Notes:

- `bootstrap-admin` is only allowed before any user exists.
- Passwords are stored using PBKDF2 hashing.
- Login creates an OTP challenge.
- OTP verification returns JWT access and refresh tokens.
- In local development only, `Otp__IncludeCodeInResponse=true` can expose the OTP in the response for easier testing.

### Users

Endpoints:

- `GET /api/users`
- `POST /api/users/credit-officers`
- `POST /api/users/{userId}/activate`
- `POST /api/users/{userId}/suspend`
- `POST /api/users/{userId}/password`

Purpose:

- Create and manage credit officers
- Activate/suspend users
- Reset or change user passwords

### Customers

Endpoints:

- `GET /api/customers`
- `GET /api/customers/{customerId}`
- `GET /api/customers/{customerId}/statement`
- `POST /api/customers`
- `PUT /api/customers/{customerId}`
- `POST /api/customers/{customerId}/reassign`
- `POST /api/customers/{customerId}/archive`

Purpose:

- Register customers
- Assign customers to credit officers
- Preserve passbook references
- Track guarantor and identity information
- Generate customer-level account statements

### Customer Documents

Endpoints:

- `GET /api/customers/{customerId}/documents`
- `POST /api/customers/{customerId}/documents`
- `GET /api/customers/{customerId}/documents/{documentId}/download`
- `DELETE /api/customers/{customerId}/documents/{documentId}`

Purpose:

- Upload and manage supporting documents for customer records
- Store metadata in MySQL
- Store physical files using the configured file storage service

### Virtual Accounts

Endpoints:

- `GET /api/virtual-accounts`
- `GET /api/customers/{customerId}/virtual-account`
- `POST /api/customers/{customerId}/virtual-account`
- `POST /api/customers/{customerId}/virtual-account/deactivate`

Purpose:

- Manage customer virtual account records for payment collection
- Support Monnify-based reconciliation in the MVP

### Loans

Endpoints:

- `GET /api/loans`
- `GET /api/loans/pending-approvals`
- `GET /api/loans/{loanId}`
- `GET /api/loans/{loanId}/statement`
- `POST /api/loans`
- `POST /api/loans/{loanId}/approve`
- `POST /api/loans/{loanId}/reject`

Purpose:

- Create loan requests
- Generate flat-rate repayment schedules
- Auto-approve loans up to NGN 20,000 when rules pass
- Require owner/admin approval for loans above NGN 20,000
- Reject loans with a reason
- View loan statements

### Disbursements

Endpoints:

- `GET /api/disbursements`
- `POST /api/disbursements`

Purpose:

- Record loan disbursements
- Track disbursement method and reference

### Repayments

Endpoints:

- `GET /api/repayments`
- `POST /api/repayments`

Purpose:

- Record repayments
- Allocate repayments against due schedules
- Update installment payment status

### Reconciliation

Endpoints:

- `GET /api/reconciliation/inbound-payments`
- `POST /api/reconciliation/inbound-payments`

Purpose:

- Record inbound payment notifications
- Match payments to customers, virtual accounts, and loans
- Support manual review where automatic matching is not enough

### Webhooks

Endpoints:

- `POST /api/webhooks/monnify`
- `GET /api/webhooks/monnify/events`

Purpose:

- Receive Monnify payment webhook payloads
- Store raw webhook event payloads
- Prevent duplicate processing
- Mark events as processed, ignored, duplicate, or failed
- Apply successful payment notifications into inbound payment reconciliation

Security note:

- Configure `Webhooks__MonnifySecret` in production.
- When configured, webhook requests must include the expected `X-Circle-Webhook-Secret` header.

### Collections

Endpoints:

- `GET /api/collections/activities`
- `GET /api/collections/worklist`
- `POST /api/collections/activities`
- `POST /api/collections/refresh-overdue`

Purpose:

- Track field collection activity
- Maintain overdue worklists
- Record call/visit/payment promise outcomes
- Refresh overdue loan schedule status

### Reports

Endpoints:

- `GET /api/reports/portfolio-summary`
- `GET /api/reports/cashflow`
- `GET /api/reports/overdue-installments`
- `GET /api/reports/arrears-aging`
- `GET /api/reports/officer-performance`

Purpose:

- Portfolio health visibility
- Repayment cashflow tracking
- Arrears and overdue analysis
- Credit officer performance reporting

### Dashboard

Endpoint:

- `GET /api/dashboard/overview`

Purpose:

- Provide a compact operational summary for the admin dashboard

### Search

Endpoint:

- `GET /api/search?query={searchTerm}&limit={limit}`

Purpose:

- Search across customers, loans, and operational records

### Administration

Endpoints:

- `GET /api/admin/settings`
- `PUT /api/admin/settings/{key}`
- `GET /api/admin/audit-logs`

Purpose:

- View and update system settings
- Inspect audit logs

### Health

Endpoints:

- `GET /health`
- `GET /api/health`
- `GET /health/database`

Purpose:

- Basic API health verification
- Database connectivity verification
- Azure deployment smoke testing

## Prerequisites

Install:

- .NET SDK 10.x
- Git
- Access to a MySQL database
- EF Core CLI tools

Install or update EF tools:

```powershell
dotnet tool update --global dotnet-ef
```

Check installed versions:

```powershell
dotnet --version
dotnet ef --version
```

## Local Setup

Clone the repository:

```powershell
git clone https://github.com/YastecHub/Circle.git
cd C:\Projects\Circle
```

Restore packages:

```powershell
dotnet restore
```

Build the solution:

```powershell
dotnet build --configuration Release
```

Configure the local connection string for the current terminal session:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=circle;User ID=root;Password=YOUR_LOCAL_PASSWORD;SslMode=Preferred;"
```

For Azure MySQL, use the Azure host and require SSL:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=circle-server.mysql.database.azure.com;Port=3306;Database=circle;User ID=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;SslMode=Required;"
```

Never commit real passwords, production JWT secrets, database credentials, or Azure secrets.

Apply database migrations:

```powershell
dotnet ef database update --project Circle.Persistence/Circle.Persistence.csproj --startup-project Circle.WebApi/Circle.WebApi.csproj
```

Run the API:

```powershell
dotnet run --project Circle.WebApi/Circle.WebApi.csproj
```

Default local URLs:

- HTTP: http://localhost:5182
- HTTPS: https://localhost:7011
- Swagger: http://localhost:5182/swagger/index.html
- Health: http://localhost:5182/health
- Database health: http://localhost:5182/health/database

## Configuration

Configuration can be supplied through `appsettings.json`, `appsettings.Development.json`, environment variables, Azure App Service Application Settings, or GitHub Actions secrets.

Important settings:

| Setting | Required | Purpose |
| --- | --- | --- |
| `ConnectionStrings__DefaultConnection` | Yes | MySQL connection string |
| `Jwt__Issuer` | Yes | JWT issuer |
| `Jwt__Audience` | Yes | JWT audience |
| `Jwt__SigningKey` | Yes | Long private signing key for JWT tokens |
| `Jwt__AccessTokenMinutes` | No | Access token lifetime |
| `Jwt__RefreshTokenDays` | No | Refresh token lifetime |
| `Otp__CodeLength` | No | OTP code length |
| `Otp__ExpiryMinutes` | No | OTP expiry time |
| `Otp__IncludeCodeInResponse` | Local only | Allows Swagger/local testing of OTP flow |
| `FileStorage__RootPath` | No | Root path for locally stored uploaded files |
| `Webhooks__MonnifySecret` | Production recommended | Shared secret for Monnify webhook protection |

Production examples:

```powershell
ConnectionStrings__DefaultConnection=Server=circle-server.mysql.database.azure.com;Port=3306;Database=circle;User ID=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;SslMode=Required;
Jwt__Issuer=Circle
Jwt__Audience=Circle.Web
Jwt__SigningKey=USE_A_LONG_RANDOM_SECRET_FROM_A_PASSWORD_MANAGER
Otp__IncludeCodeInResponse=false
Webhooks__MonnifySecret=USE_A_PRIVATE_WEBHOOK_SECRET
```

## Database Migrations

Existing migrations:

- `InitialFoundation`
- `AddAuthenticationTables`
- `AddLoanOrigination`
- `AddRepayments`
- `AddLoanDisbursements`
- `AddCustomerVirtualAccounts`
- `AddInboundPaymentReconciliation`
- `AddCollectionActivities`
- `AddPaymentWebhookEvents`
- `AddOperationalIndexes`

Create a new migration:

```powershell
dotnet ef migrations add MigrationName --project Circle.Persistence/Circle.Persistence.csproj --startup-project Circle.WebApi/Circle.WebApi.csproj
```

Apply migrations:

```powershell
dotnet ef database update --project Circle.Persistence/Circle.Persistence.csproj --startup-project Circle.WebApi/Circle.WebApi.csproj
```

Script migrations for review:

```powershell
dotnet ef migrations script --project Circle.Persistence/Circle.Persistence.csproj --startup-project Circle.WebApi/Circle.WebApi.csproj --output migrations.sql
```

Azure MySQL notes:

- Public network access must allow the machine applying migrations, or the App Service must have valid network access.
- SSL should be required for Azure MySQL.
- If local migration commands time out, confirm the Azure MySQL firewall/network access and current public IP.

## Swagger Testing Flow

Open Swagger:

```text
http://localhost:5182/swagger/index.html
```

For the live Azure API:

```text
https://circle-h3fqbbf2e2bhbsdz.southafricanorth-01.azurewebsites.net/swagger/index.html
```

First-time auth flow:

1. Call `POST /api/auth/bootstrap-admin` to create the first admin.
2. Call `POST /api/auth/login` with the admin email/password.
3. Call `POST /api/auth/verify-otp`.
4. Copy the returned access token.
5. Click `Authorize` in Swagger.
6. Enter:

```text
Bearer YOUR_ACCESS_TOKEN
```

Important:

- Bootstrap admin can run only once while the `users` table is empty.
- To test the bootstrap flow again, delete the seeded/test users from the database first.
- Do not enable `Otp__IncludeCodeInResponse=true` in production.

## Logging and Observability

The API uses Serilog for structured logs.

Current logging behavior:

- Logs startup and request pipeline events
- Logs handled application exceptions
- Logs unexpected exceptions with trace IDs
- Adds or echoes `X-Correlation-ID` on responses
- Enriches request logs with correlation ID and user ID where available
- Reduces noisy Microsoft and EF command logs to warning level by default

Useful response header:

```text
X-Correlation-ID
```

When debugging a production issue, copy the correlation ID from the response and search for it in Azure App Service logs or Application Insights.

## Security Notes

Current safeguards:

- JWT authentication
- Role-aware current user service
- PBKDF2 password hashing
- Refresh token storage
- OTP verification step
- Admin bootstrap lock after the first user exists
- Central exception handling
- Correlation IDs for traceability
- Optional Monnify webhook secret
- Audit log support

Required production practices:

- Store secrets only in Azure App Settings or GitHub Secrets.
- Rotate exposed database credentials immediately.
- Keep `Otp__IncludeCodeInResponse=false` outside local development.
- Use a long random `Jwt__SigningKey`.
- Keep Azure MySQL firewall/network rules strict.
- Review admin and credit officer role permissions before frontend launch.

## Performance and Reliability Work Already Added

Recent backend hardening includes:

- EF Core `AddDbContextPool` for context pooling
- Response compression
- Operational indexes for common dashboard, reporting, loan, repayment, collection, reconciliation, and webhook queries
- Structured request logging
- Health endpoint DTOs
- Database health check endpoint
- Forwarded header support for Azure App Service
- Safer request IP detection behind proxies
- Centralized exception handling

## Azure Deployment

The repository includes GitHub Actions deployment in:

```text
.github/workflows/master_circle.yml
```

Current deployment flow:

1. Push to `master`.
2. GitHub Actions restores and builds the .NET solution.
3. GitHub Actions publishes `Circle.WebApi`.
4. The published artifact is zipped.
5. The artifact is deployed to Azure App Service `circle`.
6. Startup command runs:

```text
dotnet Circle.WebApi.dll
```

Azure App Service settings should include:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey`
- `Otp__IncludeCodeInResponse=false`
- `Webhooks__MonnifySecret` if webhook secret validation is enabled

Deployment checks after every release:

```powershell
curl https://circle-h3fqbbf2e2bhbsdz.southafricanorth-01.azurewebsites.net/health
curl https://circle-h3fqbbf2e2bhbsdz.southafricanorth-01.azurewebsites.net/health/database
```

Then open:

```text
https://circle-h3fqbbf2e2bhbsdz.southafricanorth-01.azurewebsites.net/swagger/index.html
```

## Development Guidelines

Keep the architecture boundaries clean.

- Put business entities and enums in `Circle.Domain`.
- Put use cases, DTOs, and interfaces in `Circle.Application`.
- Put EF Core, database mappings, migrations, and infrastructure services in `Circle.Persistence`.
- Put controllers, middleware, authentication wiring, and API-specific concerns in `Circle.WebApi`.
- Do not inject `CircleDbContext` directly into controllers.
- Do not put HTTP-specific logic into the domain layer.
- Prefer DTOs for endpoint requests and responses.
- Add indexes when new report/search/list queries become important.
- Add audit logs for sensitive operational actions.

## Testing and Verification

Current verification commands:

```powershell
dotnet restore
dotnet build --configuration Release
dotnet publish Circle.WebApi/Circle.WebApi.csproj -c Release -o .\publish-check
```

Smoke test locally:

```powershell
curl http://localhost:5182/health
curl http://localhost:5182/health/database
```

Smoke test production:

```powershell
curl https://circle-h3fqbbf2e2bhbsdz.southafricanorth-01.azurewebsites.net/health
curl https://circle-h3fqbbf2e2bhbsdz.southafricanorth-01.azurewebsites.net/health/database
```

Recommended next testing improvement:

- Add a `Circle.Tests` project.
- Add unit tests for loan schedule generation, repayment allocation, approval thresholds, and webhook idempotency.
- Add integration tests for authentication, customer creation, loan creation, repayment recording, and reconciliation.

## Common Troubleshooting

### Swagger Returns 404 in Azure

Check that the API artifact was deployed, not only the empty Azure App Service placeholder.

Confirm:

- GitHub Actions deployment succeeded.
- Azure startup command is `dotnet Circle.WebApi.dll`.
- App Service runtime is .NET 10 on Linux.
- The deployed artifact contains `Circle.WebApi.dll`.

### API Uses `localhost` for Database in Azure

This means Azure App Settings are missing or incorrect.

Set:

```text
ConnectionStrings__DefaultConnection=Server=circle-server.mysql.database.azure.com;Port=3306;Database=circle;User ID=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;SslMode=Required;
```

Restart the App Service after updating settings.

### Bootstrap Admin Says It Is Already Used

The bootstrap endpoint is intentionally locked after the first user exists.

For test environments only, remove test users from the database if you need to test bootstrap again.

### Database Migration Cannot Connect

Check:

- Azure MySQL server is running.
- Public network access or private endpoint rules allow your machine.
- Firewall includes your current public IP.
- Username and password are correct.
- `SslMode=Required` is used for Azure MySQL.

### OTP Is Not Visible in Swagger

In local development, set:

```powershell
$env:Otp__IncludeCodeInResponse="true"
```

Do not enable this in production.

## Roadmap

Near-term backend priorities:

- Add automated test project
- Add stronger role-based authorization policies per endpoint
- Replace console email sender with a production email/SMS provider
- Add object storage for customer documents
- Add richer webhook signature validation based on final Monnify integration details
- Add background jobs for overdue refresh and reconciliation checks
- Add database backup/restore operational documentation
- Add frontend integration documentation once the React app starts

## Ownership

This backend is part of the Circle MVP for loan operations management. The current phase is focused on building the reliable foundation before adding the full frontend workflow.
