To run Python Azure Function locally, follow these steps:

Navigate to your Python Functions directory:
```bash
   cd c:\SourceControl\CloudPoC\Python\Functions
```

(Optional) Create and activate a virtual environment:
```bash
   python -m venv .venv
```
```bash
   .\.venv\Scripts\Activate
```

Install dependencies:
```bash
   pip install -r requirements.txt
```

Application Insights telemetry environment config values:
```json
{
  "APPLICATIONINSIGHTS_CONNECTION_STRING": "InstrumentationKey=<your-key>;IngestionEndpoint=https://<region>.in.applicationinsights.azure.com/;LiveEndpoint=https://<region>.livediagnostics.monitor.azure.com/",
  "PYTHON_APPLICATIONINSIGHTS_ENABLE_TELEMETRY": "true"
}
```

Start the Azure Functions host:
```bash
   func start
```

Run in Docker on a non-7071 port (host `7073` -> container `80`):
```bash
   docker build -t functions-python:v1 -f DockerFile .
   docker run --rm -p 7073:80 -e AzureWebJobsStorage=UseDevelopmentStorage=true -e FUNCTIONS_WORKER_RUNTIME=python -e APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=<your-key>;IngestionEndpoint=https://<region>.in.applicationinsights.azure.com/;LiveEndpoint=https://<region>.livediagnostics.monitor.azure.com/" -e PYTHON_APPLICATIONINSIGHTS_ENABLE_TELEMETRY=true functions-python:v1
```
