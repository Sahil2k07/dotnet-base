# .NET Base

.NET Base is a reusable backend template for building multiple applications and microservices within a single repository.

## What It Provides

- **Monorepo Architecture** — Supports multiple applications and microservices in a single repository while sharing common libraries.
- **Custom Database Migrations** — Provides custom migration logic with support for writing and controlling `.sql` migration files.
- **Soft Delete** — Provides a reusable soft-delete implementation for applications using Entity Framework Core.
- **Access & Refresh Token Authentication** — Uses short-lived access tokens with refresh tokens instead of a single long-lived JWT.
- **Session Management** — Stores user sessions for authentication auditing and session management.
- **Policy-Based Authorization** — Uses policies and permissions instead of relying only on fixed roles, allowing flexible roles to be created from permissions.
- **Testing Setup** — Includes a foundation for unit tests and integration tests.
- **Development & Deployment Support** — Includes Makefile, Procfile, and Docker configuration.

## Project Structure

- `Apps/DotnetBase.Server` — Main API server.
- `Apps/DotnetBase.Migrator` — Database migration application.
- Shared class libraries contain reusable authentication, data, service, and common functionality.

## Getting Started

### Prerequisites

- .NET SDK
- A supported database
- Docker (optional)
- Make (optional)

### Run the Server

```bash
cd Apps/DotnetBase.Server
dotnet run

```

### Run Migrations

```bash
cd Apps/DotnetBase.Migrator
dotnet run

```

Make sure the required database connection settings are configured before running the migrator.

## Docker

Build the Docker image:

```bash
docker build -t dotnet-base .

```

Run the container:

```bash
docker run -p 5000:5000 dotnet-base

```

## Makefile

The repository includes a Makefile for common development commands.

```bash
make

```

Check the Makefile for the available commands.

## Testing

The project includes setup for unit tests and integration tests.

```bash
dotnet test

```

## OpenAPI / Swagger

The server includes OpenAPI support and Swagger UI for development.

Swagger UI:

```text
/swagger

```

OpenAPI document:

```text
/openapi/v1.json

```

Swagger can be used for exploring endpoints and testing API requests, while Postman can be used for more complete authenticated API workflows.
