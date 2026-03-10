# Best Practices and POC

Sample .NET and Python projects for Azure-hosted API and function scenarios.

## Repository Structure

```text
best-practices-poc/
├─ DotNet/
│  ├─ Api/                 # ASP.NET Core sample API
│  ├─ Api.Tests/           # Unit tests for the .NET API
│  └─ Functions/           # Azure Functions sample
├─ Python/
│  ├─ Api/                 # FastAPI sample API
│  │  ├─ Carsharing.py
│  │  ├─ WeatherForecastService.py
│  │  ├─ Entities/
│  │  ├─ Sources/
│  │  ├─ appsettings
│  │  └─ requirements.txt
│  ├─ Api.Tests/           # Pytest-based tests for the Python API
│  │  ├─ conftest.py
│  │  ├─ test_carsharing.py
│  │  ├─ pytest.ini
│  │  └─ requirements.txt
│  └─ Functions/           # Python Azure Functions sample
├─ best-practices-poc.sln  # Visual Studio solution for .NET projects
└─ README.md
```

## .NET

The `DotNet` folder contains a sample API built with ASP.NET Core and a matching unit test project.

### Prerequisites

- .NET 8 SDK

### Run the .NET API

1. Change to the API folder.
   ```bash
   cd DotNet/Api
   ```
2. Start the application.
   ```bash
   dotnet run
   ```
3. The API is available at `http://localhost:8085` unless changed in configuration.

### Run .NET Unit Tests

1. Change to the test project.
   ```bash
   cd DotNet/Api.Tests
   ```
2. Run the tests.
   ```bash
   dotnet test
   ```

You can also run the tests from the repository root:

```bash
dotnet test DotNet/Api.Tests/Api.Tests.csproj
```

### Build the .NET Docker Image

1. Change to the API folder.
   ```bash
   cd DotNet/Api
   ```
2. Build the image.
   ```bash
   docker build -f ./DockerFile -t employeeservice-dotnet:v1 .
   ```
3. Run the container.
   ```bash
   docker run -it --rm employeeservice-dotnet:v1
   ```

## Python

The `Python` folder contains a FastAPI sample API, a Python Azure Functions sample, and a dedicated `Api.Tests` test project for the API.

### Prerequisites

- Python 3.9 or later
- `pip`

### Set Up a Virtual Environment

1. Change to the Python API folder.
   ```bash
   cd Python/Api
   ```
2. Create a virtual environment.
   ```bash
   python -m venv .venv
   ```
3. Activate it.

On Windows:

```bash
.venv\Scripts\activate
```

On macOS/Linux:

```bash
source .venv/bin/activate
```

4. Install the API dependencies.
   ```bash
   python -m pip install --no-cache-dir -r requirements.txt
   ```

### Configure the Python API

The FastAPI app reads `Python/Api/appsettings` at startup. Set a value for `userkey` before running locally.

Example:

```text
userkey=local-dev-password
```

### Run the Python API

1. Change to the API folder.
   ```bash
   cd Python/Api
   ```
2. Start the application directly.
   ```bash
   python Carsharing.py
   ```

The API is available at `http://localhost:8086`.

### Run the Python API with Uvicorn

1. Change to the API folder.
   ```bash
   cd Python/Api
   ```
2. Start Uvicorn.
   ```bash
   uvicorn Carsharing:app --port 8086 --reload
   ```

### Run Python Unit Tests

The Python API tests live in `Python/Api.Tests` and use `pytest` plus FastAPI `TestClient`.

1. Install the test dependencies.
   ```bash
   python -m pip install --no-cache-dir -r Python/Api.Tests/requirements.txt
   ```
2. Run the tests from the repository root.
   ```bash
   python -m pytest Python/Api.Tests -q
   ```

The test fixture copies `Python/Api/Sources/cars.json` to a temporary location so the real seed data is not modified during test runs.

### Build the Python Docker Image

1. Change to the API folder.
   ```bash
   cd Python/Api
   ```
2. Build the image.
   ```bash
   docker build -f ./DockerFile -t python-api:v1 .
   ```
3. Run the container.
   ```bash
   docker run -p 8086:8086 python-api:v1
   ```

## Summary Notes

- The repository contains parallel .NET and Python API examples plus Azure Functions samples.
- The Python API now has a dedicated `Python/Api.Tests` project with isolated `pytest` fixtures and baseline coverage for authentication and car endpoints.
- Python tests depend on both the API packages and test packages listed in `Python/Api.Tests/requirements.txt`.
- The Python API requires `Python/Api/appsettings` to define `userkey` before the app or tests can start successfully.
