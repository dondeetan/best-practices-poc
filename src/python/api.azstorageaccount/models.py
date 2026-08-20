from datetime import datetime
from typing import Optional

from pydantic import BaseModel, Field


class TokenResponse(BaseModel):
    access_token: str
    token_type: str = "bearer"
    expires_in_minutes: int


class ContainerSummary(BaseModel):
    name: str
    last_modified: Optional[datetime] = None


class BlobSummary(BaseModel):
    name: str
    size: int = 0
    content_type: Optional[str] = None
    last_modified: Optional[datetime] = None


class ContainerCreateRequest(BaseModel):
    metadata: dict[str, str] = Field(default_factory=dict)


class BlobUploadRequest(BaseModel):
    name: str = Field(min_length=1)
    content: str
    content_type: str = "text/plain"
    metadata: dict[str, str] = Field(default_factory=dict)
    overwrite: bool = True


class BlobUpdateRequest(BaseModel):
    content: str
    content_type: str = "text/plain"
    metadata: dict[str, str] = Field(default_factory=dict)


class StorageOperationResult(BaseModel):
    name: str
    status: str
