import copy
import json
import sys
from pathlib import Path

import pytest
from fastapi.testclient import TestClient


TESTS_DIR = Path(__file__).resolve().parent
API_DIR = TESTS_DIR.parent / "Api"
SOURCE_DB = API_DIR / "Sources" / "cars.json"


if str(API_DIR) not in sys.path:
    sys.path.insert(0, str(API_DIR))


@pytest.fixture
def client(tmp_path, monkeypatch):
    db_copy = tmp_path / "cars.json"
    db_copy.write_text(SOURCE_DB.read_text(encoding="utf-8"), encoding="utf-8")

    monkeypatch.chdir(API_DIR)

    import Carsharing
    import Entities.Cars as cars_module

    cars_module.file_path = str(db_copy)
    Carsharing.db = copy.deepcopy(cars_module.load_db())

    with TestClient(Carsharing.app) as test_client:
        yield test_client


@pytest.fixture
def auth_headers(client):
    response = client.post("/auth/token", auth=("userkey", "<insertkey>"))
    assert response.status_code == 200

    token = response.json()["access_token"]
    return {"Authorization": f"Bearer {token}"}


@pytest.fixture
def seeded_cars():
    return json.loads(SOURCE_DB.read_text(encoding="utf-8"))
