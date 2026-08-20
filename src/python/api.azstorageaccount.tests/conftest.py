import sys
from pathlib import Path

import pytest
from fastapi.testclient import TestClient


TESTS_DIR = Path(__file__).resolve().parent
API_DIR = TESTS_DIR.parent / "api.azstorageaccount"

if str(API_DIR) not in sys.path:
    sys.path.insert(0, str(API_DIR))


class FakeStorageGateway:
    def __init__(self):
        self.containers = {}

    def list_containers(self):
        from models import ContainerSummary

        return [ContainerSummary(name=name) for name in self.containers]

    def create_container(self, name, metadata):
        from models import ContainerSummary

        self.containers.setdefault(name, {"metadata": metadata, "blobs": {}})
        return ContainerSummary(name=name)

    def delete_container(self, name):
        if name not in self.containers:
            raise ValueError(f"No container with name={name}.")
        del self.containers[name]

    def list_blobs(self, container_name):
        from models import BlobSummary

        container = self._container(container_name)
        return [
            BlobSummary(name=name, size=len(blob["content"].encode("utf-8")), content_type=blob["content_type"])
            for name, blob in container["blobs"].items()
        ]

    def upload_blob(self, container_name, blob_name, content, content_type, metadata, overwrite):
        from models import BlobSummary

        container = self._container(container_name)
        if not overwrite and blob_name in container["blobs"]:
            raise ValueError(f"Blob already exists: {blob_name}.")

        container["blobs"][blob_name] = {
            "content": content,
            "content_type": content_type,
            "metadata": metadata,
        }
        return BlobSummary(name=blob_name, size=len(content.encode("utf-8")), content_type=content_type)

    def update_blob(self, container_name, blob_name, content, content_type, metadata):
        container = self._container(container_name)
        if blob_name not in container["blobs"]:
            raise ValueError(f"No blob with name={blob_name}.")
        return self.upload_blob(container_name, blob_name, content, content_type, metadata, True)

    def delete_blob(self, container_name, blob_name):
        container = self._container(container_name)
        if blob_name not in container["blobs"]:
            raise ValueError(f"No blob with name={blob_name}.")
        del container["blobs"][blob_name]

    def _container(self, container_name):
        if container_name not in self.containers:
            raise ValueError(f"No container with name={container_name}.")
        return self.containers[container_name]


@pytest.fixture
def fake_gateway():
    return FakeStorageGateway()


@pytest.fixture
def client(monkeypatch, fake_gateway):
    monkeypatch.chdir(API_DIR)

    import main

    main.app.dependency_overrides[main.get_storage_gateway] = lambda: fake_gateway
    with TestClient(main.app) as test_client:
        yield test_client
    main.app.dependency_overrides.clear()


@pytest.fixture
def auth_headers(client):
    response = client.post("/auth/token", auth=("userkey", "<insertkey>"))
    assert response.status_code == 200

    token = response.json()["access_token"]
    return {"Authorization": f"Bearer {token}"}
