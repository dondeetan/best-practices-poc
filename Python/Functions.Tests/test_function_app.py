import logging
import sys
from pathlib import Path

import azure.functions as func


TESTS_DIR = Path(__file__).resolve().parent
FUNCTIONS_DIR = TESTS_DIR.parent / "Functions"


if str(FUNCTIONS_DIR) not in sys.path:
    sys.path.insert(0, str(FUNCTIONS_DIR))


import function_app


def test_http_trigger_welcome_returns_personalized_message():
    request = func.HttpRequest(
        method="GET",
        url="http://localhost/api/httptriggerwelcomeanonymous",
        params={"name": "Ada"},
        body=b"",
    )

    response = function_app.http_trigger_welcome(request)

    assert response.status_code == 200
    assert response.get_body().decode("utf-8") == "Hello, Ada. This HTTP triggered function executed successfully."


def test_http_trigger_welcome_returns_default_message_without_name():
    request = func.HttpRequest(
        method="GET",
        url="http://localhost/api/httptriggerwelcomeanonymous",
        params={},
        body=b"",
    )

    response = function_app.http_trigger_welcome(request)

    assert response.status_code == 200
    assert "Pass a name in the query string" in response.get_body().decode("utf-8")


def test_timer_trigger_logs_success_message(caplog):
    timer = type("TimerStub", (), {"past_due": False})()

    with caplog.at_level(logging.INFO):
        function_app.timer_trigger_hourly(timer)

    assert "Timmer triggered function executed successfully." in caplog.text
