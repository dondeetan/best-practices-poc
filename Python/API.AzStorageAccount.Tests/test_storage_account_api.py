def test_can_issue_jwt_token(client):
    response = client.post("/auth/token", auth=("userkey", "<insertkey>"))

    assert response.status_code == 200
    body = response.json()
    assert body["token_type"] == "bearer"
    assert body["access_token"]


def test_storage_endpoints_require_authentication(client):
    response = client.get("/api/storage/containers")

    assert response.status_code == 403


def test_create_and_list_containers(client, auth_headers):
    create_response = client.post(
        "/api/storage/containers/audit-logs",
        headers=auth_headers,
        json={"metadata": {"classification": "confidential"}},
    )

    assert create_response.status_code == 201
    assert create_response.json()["name"] == "audit-logs"

    list_response = client.get("/api/storage/containers", headers=auth_headers)
    assert list_response.status_code == 200
    assert list_response.json() == [{"name": "audit-logs", "last_modified": None}]


def test_upload_update_list_and_delete_blob(client, auth_headers):
    client.post("/api/storage/containers/evidence", headers=auth_headers, json={"metadata": {}})

    upload_response = client.post(
        "/api/storage/containers/evidence/blobs",
        headers=auth_headers,
        json={
            "name": "finding-001.txt",
            "content": "initial finding",
            "content_type": "text/plain",
            "metadata": {"owner": "security"},
        },
    )

    assert upload_response.status_code == 201
    assert upload_response.json()["size"] == len("initial finding")

    update_response = client.put(
        "/api/storage/containers/evidence/blobs/finding-001.txt",
        headers=auth_headers,
        json={
            "content": "updated finding",
            "content_type": "text/markdown",
            "metadata": {"owner": "security"},
        },
    )

    assert update_response.status_code == 200
    assert update_response.json()["content_type"] == "text/markdown"

    list_response = client.get("/api/storage/containers/evidence/blobs", headers=auth_headers)
    assert list_response.status_code == 200
    assert list_response.json()[0]["name"] == "finding-001.txt"

    delete_response = client.delete(
        "/api/storage/containers/evidence/blobs/finding-001.txt",
        headers=auth_headers,
    )
    assert delete_response.status_code == 200
    assert delete_response.json() == {"name": "finding-001.txt", "status": "deleted"}


def test_delete_container(client, auth_headers):
    client.post("/api/storage/containers/transient", headers=auth_headers, json={"metadata": {}})

    response = client.delete("/api/storage/containers/transient", headers=auth_headers)

    assert response.status_code == 204
