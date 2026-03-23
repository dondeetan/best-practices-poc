# Best Practices POC

This public repository is a side-by-side reference for two common Azure application shapes in both `.NET` and `Python`:

- HTTP APIs
- Azure Functions apps

If you are reading the repo on GitHub, the fastest path is:

1. Pick a stack: `.NET` or `Python`.
2. Open the application project first.
3. Open the matching test project next.
4. Use the build, test, run, and Docker commands below from the repository root unless a section says otherwise.

## Repository Map

```text
best-practices-poc/
|-- DotNet/
|   |-- Api/                 # ASP.NET Core API
|   |-- Api.Tests/           # Unit tests for DotNet/Api
|   |-- Functions/           # Azure Functions isolated worker app
|   `-- Functions.Tests/     # Unit tests for DotNet/Functions
|-- Python/
|   |-- Api/                 # FastAPI app
|   |-- Api.Tests/           # Pytest suite for Python/Api
|   |-- Functions/           # Python Azure Functions app
|   `-- Functions.Tests/     # Pytest suite for Python/Functions
|-- best-practices-poc.sln   # Visual Studio solution for .NET projects
`-- README.md
```

## Prerequisites

Install the tools that match the projects you want to explore:

- `.NET 8 SDK`
- `Python 3.11+`
- `Docker Desktop` or another Docker engine
- `Azure Functions Core Tools` for local Functions runs
- Optional for Functions local development: `Azurite`

## Project Guide

### DotNet/Api

Purpose:
ASP.NET Core Web API that serves employee data and integrates with external car data.

Read this project on GitHub:

- Start with `DotNet/Api/Program.cs` for startup and dependency wiring.
- Open `DotNet/Api/Controllers/EmployeeController.cs` for the API surface.
- Review `DotNet/Api/Services/` and `DotNet/Api/Entities/` for behavior and models.
- Open `DotNet/Api.Tests/` next to see the unit-tested scenarios.

Build:

```bash
dotnet build DotNet/Api/Api.csproj
```

Test:

```bash
dotnet test DotNet/Api.Tests/Api.Tests.csproj
```

Run:

```bash
dotnet run --project DotNet/Api/Api.csproj
```

Local URL:
`http://localhost:8085`

Docker image:

```bash
docker build -f DotNet/Api/DockerFile -t best-practices-dotnet-api:v1 DotNet/Api
docker run --rm -p 8085:8085 best-practices-dotnet-api:v1
```

### DotNet/Functions

Purpose:
Azure Functions isolated worker app with HTTP-triggered and timer-triggered functions.

Read this project on GitHub:

- Start with `DotNet/Functions/Program.cs` for host setup, telemetry, and `HttpClient` registration.
- Open `DotNet/Functions/HttpTriggers.cs` for the HTTP entry points.
- Open `DotNet/Functions/CarsSync.cs` for the timer-driven sync workflow.
- Review `DotNet/Functions/local.env` and `DotNet/Functions/docker-compose.yml` for expected local configuration.
- Open `DotNet/Functions.Tests/` next to see the unit-tested behavior.

Build:

```bash
dotnet build DotNet/Functions/Functions.csproj
```

Test:

```bash
dotnet test DotNet/Functions.Tests/Functions.Tests.csproj
```

Run:

Before starting locally, provide the values shown in `DotNet/Functions/local.env` as environment variables or create an equivalent `local.settings.json`. At minimum, the app expects storage, worker runtime, API credentials, and cache settings.

```bash
cd DotNet/Functions
func start
```

Default local URL:
`http://localhost:7071`

Docker image:

```bash
docker build -f DotNet/Functions/DockerFile -t best-practices-dotnet-functions:v1 DotNet/Functions
docker run --rm -p 7071:80 -e AzureWebJobsStorage=UseDevelopmentStorage=true -e FUNCTIONS_WORKER_RUNTIME=dotnet-isolated -e FUNCTIONS_EXTENSION_VERSION=~4 -e ASPNETCORE_URLS=http://0.0.0.0:80 -e CarsApiBaseUrl=http://host.docker.internal:8086/ -e CarsApiUser=userkey -e CarsApiKey=<insertkey> -e UseRedisCache=false best-practices-dotnet-functions:v1
```

### Python/Api

Purpose:
FastAPI app that exposes authentication and car-sharing endpoints.

Read this project on GitHub:

- Start with `Python/Api/Carsharing.py` for startup, auth, and routes.
- Open `Python/Api/Entities/Cars.py` for the persisted model and JSON storage helpers.
- Review `Python/Api/Sources/cars.json` for the seed data.
- Open `Python/Api.Tests/` next to see the isolated pytest coverage.

Build:

The local build step is installing the application dependencies.

```bash
python -m pip install --no-cache-dir -r Python/Api/requirements.txt
```

Test:

The API reads `Python/Api/appsettings`, so keep `userkey=<insertkey>` or another local value there before running tests.

```bash
python -m pip install --no-cache-dir -r Python/Api.Tests/requirements.txt
python -m pytest Python/Api.Tests -q
```

Run:

```bash
python -m uvicorn Carsharing:app --app-dir Python/Api --host 0.0.0.0 --port 8086
```

Alternative run command from the project folder:

```bash
cd Python/Api
python Carsharing.py
```

Local URL:
`http://localhost:8086`

Docker image:

```bash
docker build -f Python/Api/DockerFile -t best-practices-python-api:v1 Python/Api
docker run --rm -p 8086:8086 best-practices-python-api:v1
```

### Python/Functions

Purpose:
Python Azure Functions app with one HTTP trigger and one timer trigger.

Read this project on GitHub:

- Start with `Python/Functions/function_app.py` for the trigger definitions.
- Review `Python/Functions/requirements.txt` and `Python/Functions/host.json` for runtime dependencies and host configuration.
- Open `Python/Functions.Tests/` next to see the unit tests for the trigger functions.

Build:

The local build step is installing the function app dependencies.

```bash
python -m pip install --no-cache-dir -r Python/Functions/requirements.txt
```

Test:

```bash
python -m pip install --no-cache-dir -r Python/Functions.Tests/requirements.txt
python -m pytest Python/Functions.Tests -q
```

Run:

Set the local Functions settings first. The minimum useful values are:

```text
AzureWebJobsStorage=UseDevelopmentStorage=true
FUNCTIONS_WORKER_RUNTIME=python
```

Then start the local host:

```bash
cd Python/Functions
func start
```

Default local URL:
`http://localhost:7071`

Docker image:

```bash
docker build -f Python/Functions/DockerFile -t best-practices-python-functions:v1 Python/Functions
docker run --rm -p 7073:80 -e AzureWebJobsStorage=UseDevelopmentStorage=true -e FUNCTIONS_WORKER_RUNTIME=python best-practices-python-functions:v1
```

## Quick Verification After Clone

If you want to validate the repository after cloning it, run:

```bash
dotnet test DotNet/Api.Tests/Api.Tests.csproj
dotnet test DotNet/Functions.Tests/Functions.Tests.csproj
python -m pytest Python/Api.Tests -q
python -m pytest Python/Functions.Tests -q
```

## Notes

- `best-practices-poc.sln` tracks the `.NET` application and test projects.
- The Python test suites are separated into `Api.Tests` and `Functions.Tests` so each app keeps focused dependencies.
- The Docker commands above assume you run them from the repository root.
- The Function projects need local Azure Functions configuration before `func start` will succeed.
