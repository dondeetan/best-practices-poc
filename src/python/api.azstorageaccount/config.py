from hashlib import sha256
from typing import Optional

from pydantic import Field
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    # Single Responsibility Principle: this type only owns application configuration
    # binding from appsettings/environment values.
    userkey: str

    storage_account_url: str = Field(
        default="https://<account-name>.blob.core.windows.net",
        description="Azure Blob Storage account URL.",
    )
    storage_sas_token: str = Field(
        default="<insert-sas-token>",
        description="SAS token used by the Azure Storage SDK.",
    )

    jwt_secret: Optional[str] = None
    jwt_algorithm: str = "HS256"
    jwt_exp_minutes: int = 60

    model_config = {"env_file": "appsettings"}


def load_settings() -> Settings:
    # Factory Method: centralizes Settings construction so callers do not need to
    # know how defaults such as the derived JWT secret are assembled.
    settings = Settings()
    if not settings.jwt_secret:
        settings.jwt_secret = sha256(f"derived::{settings.userkey}".encode("utf-8")).hexdigest()
    return settings
