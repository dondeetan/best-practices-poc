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
|-- src/
|   |-- dotnet/
|   |   |-- api/                 # ASP.NET Core API
|   |   |-- api.tests/           # Unit tests for src/dotnet/api
|   |   |-- functions/           # Azure Functions isolated worker app
|   |   |-- functions.tests/     # Unit tests for src/dotnet/functions
|   |   `-- patterns/            # Runnable .NET console samples for principles and patterns
|   |       |-- designprinciples/
|   |       |   `-- solidprinciples/
|   |       `-- designpatterns/
|   |           |-- creational/
|   |           |   `-- creationalpatterns/
|   |           |-- structural/
|   |           |   `-- structuralpatterns/
|   |           `-- behavioral/
|   |               `-- behavioralpatterns/
|   `-- python/
|       |-- api/                         # FastAPI app
|       |-- api.tests/                   # Pytest suite for src/python/api
|       |-- api.azstorageaccount/        # FastAPI app for Azure Blob Storage account operations
|       |-- api.azstorageaccount.tests/  # Pytest suite for src/python/api.azstorageaccount
|       |-- functions/                   # Python Azure Functions app
|       `-- functions.tests/             # Pytest suite for src/python/functions
|-- best-practices-poc.sln   # Visual Studio solution for .NET projects
`-- README.md
```

## Prerequisites

Install the tools that match the projects you want to explore:

- `.NET 10 SDK`
- `Python 3.11+`
- `Docker Desktop` or another Docker engine
- `Azure Functions Core Tools` for local Functions runs
- Optional for Functions local development: `Azurite`

## Project Guide

### src/dotnet/api

Purpose:
ASP.NET Core Web API that serves employee data and integrates with external car data.

Read this project on GitHub:

- Start with `src/dotnet/api/Program.cs` for startup and dependency wiring.
- Open `src/dotnet/api/controllers/EmployeeController.cs` for the API surface.
- Review `src/dotnet/api/services/` and `src/dotnet/api/entities/` for behavior and models.
- Open `src/dotnet/api.tests/` next to see the unit-tested scenarios.

Build:

```bash
dotnet build src/dotnet/api/Api.csproj
```

Test:

```bash
dotnet test src/dotnet/api.tests/Api.Tests.csproj
```

Run:

```bash
dotnet run --project src/dotnet/api/Api.csproj
```

Local URL:
`http://localhost:8085`

Docker image:

```bash
docker build -f src/dotnet/api/DockerFile -t best-practices-dotnet-api:v1 src/dotnet/api
docker run --rm -p 8085:8085 best-practices-dotnet-api:v1
```

### src/dotnet/functions

Purpose:
Azure Functions isolated worker app with HTTP-triggered and timer-triggered functions.

Read this project on GitHub:

- Start with `src/dotnet/functions/Program.cs` for host setup, telemetry, and `HttpClient` registration.
- Open `src/dotnet/functions/HttpTriggers.cs` for the HTTP entry points.
- Open `src/dotnet/functions/CarsSync.cs` for the timer-driven sync workflow.
- Review `src/dotnet/functions/local.env` and `src/dotnet/functions/docker-compose.yml` for expected local configuration.
- Open `src/dotnet/functions.tests/` next to see the unit-tested behavior.

Build:

```bash
dotnet build src/dotnet/functions/Functions.csproj
```

Test:

```bash
dotnet test src/dotnet/functions.tests/Functions.Tests.csproj
```

Run:

Before starting locally, provide the values shown in `src/dotnet/functions/local.env` as environment variables or create an equivalent `local.settings.json`. At minimum, the app expects storage, worker runtime, API credentials, and cache settings.

```bash
cd src/dotnet/functions
func start
```

Default local URL:
`http://localhost:7071`

Docker image:

```bash
docker build -f src/dotnet/functions/DockerFile -t best-practices-dotnet-functions:v1 src/dotnet/functions
docker run --rm -p 7071:80 -e AzureWebJobsStorage=UseDevelopmentStorage=true -e FUNCTIONS_WORKER_RUNTIME=dotnet-isolated -e FUNCTIONS_EXTENSION_VERSION=~4 -e ASPNETCORE_URLS=http://0.0.0.0:80 -e CarsApiBaseUrl=http://host.docker.internal:8086/ -e CarsApiUser=userkey -e CarsApiKey=<insertkey> -e UseRedisCache=false best-practices-dotnet-functions:v1
```

### src/dotnet/patterns

Purpose:
Runnable `.NET 10` console projects that demonstrate SOLID principles and the full GoF design pattern categories in current C# examples. Each principle or pattern is extracted into its own source file with a description and a practical usage-frequency note.

Read this area on GitHub:

- Start with the project that matches the topic you want to explore.
- Open the `Program.cs` file in each sample to see the console output flow.
- Open the individual principle or pattern class file for the implementation, description, and usage-frequency note.
- These samples intentionally use simple in-memory scenarios so the design ideas stay easy to understand.

#### src/dotnet/patterns/designprinciples/SolidPrinciples

Purpose:
Demonstrates the five SOLID principles:

- `SingleResponsibilityPrinciple.cs`
- `OpenClosedPrinciple.cs`
- `LiskovSubstitutionPrinciple.cs`
- `InterfaceSegregationPrinciple.cs`
- `DependencyInversionPrinciple.cs`

`Program.cs` seeds the sample data and invokes each principle runner. Shared work-item support types live in `WorkItems.cs`.

Build:

```bash
dotnet build src/dotnet/patterns/designprinciples/solidprinciples/SolidPrinciples.csproj
```

Run:

```bash
dotnet run --project src/dotnet/patterns/designprinciples/solidprinciples/SolidPrinciples.csproj
```

#### src/dotnet/patterns/designpatterns/creational/CreationalPatterns

Purpose:
Demonstrates the creational design patterns:

- `AbstractFactoryPattern.cs`
- `BuilderPattern.cs`
- `FactoryMethodPattern.cs`
- `PrototypePattern.cs`
- `SingletonPattern.cs`

`Program.cs` invokes the pattern runners in the classic creational pattern order.

Build:

```bash
dotnet build src/dotnet/patterns/designpatterns/creational/creationalpatterns/CreationalPatterns.csproj
```

Run:

```bash
dotnet run --project src/dotnet/patterns/designpatterns/creational/creationalpatterns/CreationalPatterns.csproj
```

#### src/dotnet/patterns/designpatterns/structural/StructuralPatterns

Purpose:
Demonstrates the structural design patterns:

- `AdapterPattern.cs`
- `BridgePattern.cs`
- `CompositePattern.cs`
- `DecoratorPattern.cs`
- `FacadePattern.cs`
- `FlyweightPattern.cs`
- `ProxyPattern.cs`

`Program.cs` invokes the pattern runners in the classic structural pattern order.

Build:

```bash
dotnet build src/dotnet/patterns/designpatterns/structural/structuralpatterns/StructuralPatterns.csproj
```

Run:

```bash
dotnet run --project src/dotnet/patterns/designpatterns/structural/structuralpatterns/StructuralPatterns.csproj
```

#### src/dotnet/patterns/designpatterns/behavioral/BehavioralPatterns

Purpose:
Demonstrates the behavioral design patterns:

- `ChainOfResponsibilityPattern.cs`
- `CommandPattern.cs`
- `InterpreterPattern.cs`
- `IteratorPattern.cs`
- `MediatorPattern.cs`
- `MementoPattern.cs`
- `ObserverPattern.cs`
- `StatePattern.cs`
- `StrategyPattern.cs`
- `TemplateMethodPattern.cs`
- `VisitorPattern.cs`

`Program.cs` invokes the pattern runners in the classic behavioral pattern order.

Build:

```bash
dotnet build src/dotnet/patterns/designpatterns/behavioral/behavioralpatterns/BehavioralPatterns.csproj
```

Run:

```bash
dotnet run --project src/dotnet/patterns/designpatterns/behavioral/behavioralpatterns/BehavioralPatterns.csproj
```

### src/python/api

Purpose:
FastAPI app that exposes authentication and car-sharing endpoints.

Read this project on GitHub:

- Start with `src/python/api/Carsharing.py` for startup, auth, and routes.
- Open `src/python/api/entities/Cars.py` for the persisted model and JSON storage helpers.
- Review `src/python/api/sources/cars.json` for the seed data.
- Open `src/python/api.tests/` next to see the isolated pytest coverage.

Create and activate a virtual environment from the repository root:

```powershell
# Windows PowerShell
python -m venv .venv/python-api
.\.venv\python-api\Scripts\Activate.ps1
python -m pip install --upgrade pip
```

```bash
# macOS or Linux
python3 -m venv .venv/python-api
source .venv/python-api/bin/activate
python -m pip install --upgrade pip
```

Use this environment for both `src/python/api` and `src/python/api.tests`. Run `deactivate` when you are finished or before activating another project's environment.

Build:

The local build step is installing the application dependencies.

```bash
python -m pip install --no-cache-dir -r src/python/api/requirements.txt
```

Test:

The API reads `src/python/api/appsettings`, so keep `userkey=<insertkey>` or another local value there before running tests.

```bash
python -m pip install --no-cache-dir -r src/python/api.tests/requirements.txt
python -m pytest src/python/api.tests -q
```

Run:

```bash
python -m uvicorn Carsharing:app --app-dir src/python/api --host 0.0.0.0 --port 8086
```

Alternative run command from the project folder:

```bash
cd src/python/api
python Carsharing.py
```

Local URL:
`http://localhost:8086`

Docker image:

```bash
docker build -f src/python/api/DockerFile -t best-practices-python-api:v1 src/python/api
docker run --rm -p 8086:8086 best-practices-python-api:v1
```

### src/python/api.azstorageaccount

Purpose:
FastAPI app that exposes authenticated REST endpoints for Azure Blob Storage account container and blob operations using SAS token authentication through the Azure Storage SDK.

Read this project on GitHub:

- Start with `src/python/api.azstorageaccount/main.py` for startup, dependency wiring, and routes.
- Open `src/python/api.azstorageaccount/auth.py` for the Basic-to-JWT authentication flow copied from the current Python API shape.
- Open `src/python/api.azstorageaccount/storage_gateway.py` for the Azure SDK gateway and design-pattern comments.
- Review `src/python/api.azstorageaccount/appsettings` for the configurable storage account URL and SAS token values.
- Open `src/python/api.azstorageaccount.tests/` next to see the isolated pytest coverage.

Create and activate a virtual environment from the repository root:

```powershell
# Windows PowerShell
python -m venv .venv/python-az-storage-api
.\.venv\python-az-storage-api\Scripts\Activate.ps1
python -m pip install --upgrade pip
```

```bash
# macOS or Linux
python3 -m venv .venv/python-az-storage-api
source .venv/python-az-storage-api/bin/activate
python -m pip install --upgrade pip
```

Use this environment for both `src/python/api.azstorageaccount` and `src/python/api.azstorageaccount.tests`. Run `deactivate` when you are finished or before activating another project's environment.

Build:

The local build step is installing the application dependencies.

```bash
python -m pip install --no-cache-dir -r src/python/api.azstorageaccount/requirements.txt
```

Test:

The tests use a fake storage gateway, so they do not require a live Azure Storage account or real SAS token.

```bash
python -m pip install --no-cache-dir -r src/python/api.azstorageaccount.tests/requirements.txt
python -m pytest src/python/api.azstorageaccount.tests -q
```

Run:

Before running locally, update `src/python/api.azstorageaccount/appsettings` with:

```text
userkey=<insertkey>
storage_account_url=https://<account-name>.blob.core.windows.net
storage_sas_token=<insert-sas-token>
```

Then start the API:

```bash
python -m uvicorn main:app --app-dir src/python/api.azstorageaccount --host 0.0.0.0 --port 8087
```

Alternative run command from the project folder:

```bash
cd src/python/api.azstorageaccount
python main.py
```

Local URL:
`http://localhost:8087`

### src/python/functions

Purpose:
Python Azure Functions app with one HTTP trigger and one timer trigger.

Read this project on GitHub:

- Start with `src/python/functions/function_app.py` for the trigger definitions.
- Review `src/python/functions/requirements.txt` and `src/python/functions/host.json` for runtime dependencies and host configuration.
- Open `src/python/functions.tests/` next to see the unit tests for the trigger functions.

Create and activate a virtual environment from the repository root:

```powershell
# Windows PowerShell
python -m venv .venv/python-functions
.\.venv\python-functions\Scripts\Activate.ps1
python -m pip install --upgrade pip
```

```bash
# macOS or Linux
python3 -m venv .venv/python-functions
source .venv/python-functions/bin/activate
python -m pip install --upgrade pip
```

Use this environment for both `src/python/functions` and `src/python/functions.tests`. Run `deactivate` when you are finished or before activating another project's environment.

Build:

The local build step is installing the function app dependencies.

```bash
python -m pip install --no-cache-dir -r src/python/functions/requirements.txt
```

Test:

```bash
python -m pip install --no-cache-dir -r src/python/functions.tests/requirements.txt
python -m pytest src/python/functions.tests -q
```

Run:

Set the local Functions settings first. The minimum useful values are:

```text
AzureWebJobsStorage=UseDevelopmentStorage=true
FUNCTIONS_WORKER_RUNTIME=python
```

Then start the local host:

```bash
cd src/python/functions
func start
```

Default local URL:
`http://localhost:7071`

Docker image:

```bash
docker build -f src/python/functions/DockerFile -t best-practices-python-functions:v1 src/python/functions
docker run --rm -p 7073:80 -e AzureWebJobsStorage=UseDevelopmentStorage=true -e FUNCTIONS_WORKER_RUNTIME=python best-practices-python-functions:v1
```

## Quick Verification After Clone

If you want to validate the repository after cloning it, run the `.NET` commands directly. For each Python test command, first create or activate its matching virtual environment as described above and install the test project's requirements.

```bash
dotnet test src/dotnet/api.tests/Api.Tests.csproj
dotnet test src/dotnet/functions.tests/Functions.Tests.csproj
dotnet build src/dotnet/patterns/designprinciples/solidprinciples/SolidPrinciples.csproj
dotnet build src/dotnet/patterns/designpatterns/creational/creationalpatterns/CreationalPatterns.csproj
dotnet build src/dotnet/patterns/designpatterns/structural/structuralpatterns/StructuralPatterns.csproj
dotnet build src/dotnet/patterns/designpatterns/behavioral/behavioralpatterns/BehavioralPatterns.csproj
python -m pytest src/python/api.tests -q
python -m pytest src/python/api.azstorageaccount.tests -q
python -m pytest src/python/functions.tests -q
```

## Notes

- `best-practices-poc.sln` tracks the `.NET` application, test, and patterns sample projects.
- The Python test suites are separated into `api.tests`, `api.azstorageaccount.tests`, and `functions.tests` so each app keeps focused dependencies.
- The Docker commands above assume you run them from the repository root.
- The Function projects need local Azure Functions configuration before `func start` will succeed.
