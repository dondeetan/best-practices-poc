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

Start the Azure Functions host:
```bash
   func start
```
