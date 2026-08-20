def test_can_issue_jwt_token(client):
    response = client.post("/auth/token", auth=("userkey", "<insertkey>"))

    assert response.status_code == 200
    body = response.json()
    assert body["token_type"] == "bearer"
    assert body["access_token"]


def test_get_cars_requires_authentication(client):
    response = client.get("/api/cars")

    assert response.status_code == 403


def test_get_cars_returns_seed_data(client, auth_headers, seeded_cars):
    response = client.get("/api/cars", headers=auth_headers)

    assert response.status_code == 200
    cars = response.json()
    assert len(cars) == len(seeded_cars)
    assert cars[0]["id"] == seeded_cars[0]["id"]


def test_add_car_persists_to_isolated_test_db(client, auth_headers):
    response = client.post(
        "/api/cars/",
        headers=auth_headers,
        json={
            "size": "m",
            "doors": 5,
            "fuel": "hybrid",
            "transmission": "manual",
        },
    )

    assert response.status_code == 200
    created_car = response.json()
    assert created_car["id"] == 10
    assert created_car["fuel"] == "hybrid"

    cars_response = client.get("/api/cars", headers=auth_headers)
    assert cars_response.status_code == 200
    assert len(cars_response.json()) == 10
