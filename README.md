# Best Practices and POC
Dtan's Best Practices and POC 

## Folder Structure
The repository is organized as follows:

```text
Best Practices and POC # Sample Cloud Projects for Azure Hosting Solutions
├─ DotNet/ 
│ ├── Api/ # Contains the .NET API project 
│ │ ├── Controllers/ # API controllers for handling HTTP requests 
│ │ ├── Entities/ # Entity models used in the application 
│ │ ├── Helpers/ # Helper classes for common functionality (e.g., JWT validation) 
│ │ ├── Program.cs # Entry point for the .NET application 
│ │ └── appsettings.json # Configuration file for the .NET application 
│ └── Tests/ # Unit tests for the .NET API 
│ ├── Python/ 
│ ├── Api/ # Contains the FastAPI project 
│ │ ├── WeatherForecastService.py # Main FastAPI application file 
│ │ ├── models/ # Data models for the FastAPI application 
│ │ └── utils/ # Utility functions for the FastAPI application 
│ └── Tests/ # Unit tests for the FastAPI application 
│ └── README.md # Documentation for the repository
│ └── Changelog.md # Notable changes to this project
│ └── .gitignore.md # Specifies which files or directories should be ignored by Git
│ └── best-practices-poc.sln # Solution file for .Net that includes all .Net Projects
└──
```

## .NET
The `.NET` folder contains a sample API built with ASP.NET Core.

### Prerequisites
- Ensure that the .NET 8 runtime is installed 

### Run Sample API
1. Navigate to the `DotNet/Api` folder:
   ```bash
   cd DotNet/Api
   ```
2. Run the application:
   ```bash
   dotnet run
   ```
3. The API will be available at `http://localhost:8085` (or the configured URL in Program.cs).

### Run Unit Tests
1. Navigate to the `DotNet/Tests` folder:
   ```bash
   cd DotNet/Tests
   ```   
2. Run the tests:
   ```bash
   dotnet test
   ```

### Build Docker Container
1. Navigate to the Api folder:
   ```bash
   cd DotNet/Api
   ```
2. Build the Docker image:
   ```bash
   docker build -f ./DockerFile -t employeeservice-dotnet:v1 .
   ```
3. Run the Docker container:
   ```bash
   docker run -it --rm  employeeservice-dotnet:v1 
   ```


## Python
The Python folder contains a sample API built with FastAPI.

Prerequisites
- Install Python 3.9 or higher.
- Intall the required FastAPI modules:

Install fastapi modules:
Run: 
   ``` 
   python -m pip install "fastapi[all]"
   ```   
Create Python virtual environment:
1. Create a virtual environment:
   ```bash
   python -m venv fastapi 
   ```
2. Activate the virtual environment:
- On Windows: 
   ```bash
   cd Python/Api/fastapi/Scripts
   .\activate   
   ```
- On macOS/Linux: 
   ```bash
   source fastapi/bin/activate
   ```   
3. To deactivate the virtual environment:
   ```bash
   deactivate
   ```
4. Install API Modules
   ```bash
   pip install --no-cache-dir -r requirements.txt
   ```

### Run Sample API
1. Navigate to the Api folder:
   ```bash
   cd Python/Api
   ```   
2. Run the application:
   ```bash
   python Carsharing.py
   ```   
### Run Using Uvicorn
1. Navigate to the Api folder:
   ```bash
   cd Python/Api
   ```
2. Run the application with Uvicorn:
   ```bash
   uvicorn Carsharing:app --port=8086 --reload
   ```
3. The API will be available at `http://localhost:8086`
   
### Run Unit Tests
1. Navigate to the Python/Tests folder:
   ```bash
   cd Python/Tests
   ```
2. Run the tests:
   ```bash
   pytest
   ```

### Build Docker Container
1. Navigate to the Api folder:
   ```bash
   cd Python/Api
   ```
2. Build the Docker image:
   ```bash
   docker build -t python-api .
   ```
3. Run the Docker container:
   ```bash
   docker run -p 8086:8086 python-api
   ```

Notes
- Ensure that the required dependencies are installed before running the applications.
- Update the configuration files (e.g., appsettings.json for .NET) as needed for your environment.

This version includes:
1. **Descriptions for the folder structure**: Each folder and file is described in the `Folder Structure` section.
2. **Instructions for running the applications**: Detailed steps for running both the .NET and Python applications, including prerequisites, running the APIs, running unit tests, and building Docker containers.This version includes:
3. **Descriptions for the folder structure**: Each folder and file is described in the `Folder Structure` section.
4. **Instructions for running the applications**: Detailed steps for running both the .NET and Python applications, including prerequisites, running the APIs, running unit tests, and building Docker containers.