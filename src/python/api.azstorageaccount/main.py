import uvicorn
from fastapi import Depends, FastAPI, Response, status
from fastapi.responses import JSONResponse

from auth import create_access_token, get_current_user, settings, verify_basic
from config import load_settings
from models import (
    BlobSummary,
    BlobUpdateRequest,
    BlobUploadRequest,
    ContainerCreateRequest,
    ContainerSummary,
    StorageOperationResult,
    TokenResponse,
)
from storage_gateway import AzureSasBlobStorageGateway, StorageGateway


app = FastAPI(title="Azure Storage Account API", version="1.0.0")


def get_storage_gateway() -> StorageGateway:
    # Factory Method + Dependency Inversion: FastAPI calls this factory to supply
    # a StorageGateway abstraction, which tests can replace with a fake gateway.
    app_settings = load_settings()
    return AzureSasBlobStorageGateway(
        account_url=app_settings.storage_account_url,
        sas_token=app_settings.storage_sas_token,
    )


@app.post("/auth/token", response_model=TokenResponse)
def login_and_issue_token(user: str = Depends(verify_basic)) -> TokenResponse:
    # Single Responsibility Principle: this endpoint only exchanges validated
    # Basic credentials for a bearer token.
    token = create_access_token(sub=user)
    return TokenResponse(access_token=token, expires_in_minutes=settings.jwt_exp_minutes)


@app.get("/api/storage/containers", response_model=list[ContainerSummary])
def list_containers(
    user: str = Depends(get_current_user),
    gateway: StorageGateway = Depends(get_storage_gateway),
) -> list[ContainerSummary]:
    # REST Resource pattern: GET is safe/read-only and returns typed container
    # representations instead of leaking Azure SDK objects.
    return gateway.list_containers()


@app.post(
    "/api/storage/containers/{container_name}",
    response_model=ContainerSummary,
    status_code=status.HTTP_201_CREATED,
)
def create_container(
    container_name: str,
    request: ContainerCreateRequest,
    user: str = Depends(get_current_user),
    gateway: StorageGateway = Depends(get_storage_gateway),
) -> ContainerSummary:
    # REST Resource pattern: POST creates a child resource and returns 201 with
    # the created container representation.
    return gateway.create_container(container_name, request.metadata)


@app.delete("/api/storage/containers/{container_name}", status_code=status.HTTP_204_NO_CONTENT)
def delete_container(
    container_name: str,
    user: str = Depends(get_current_user),
    gateway: StorageGateway = Depends(get_storage_gateway),
) -> Response:
    # Command pattern: the delete request is represented as a focused operation
    # with no response body when it succeeds.
    gateway.delete_container(container_name)
    return Response(status_code=status.HTTP_204_NO_CONTENT)


@app.get("/api/storage/containers/{container_name}/blobs", response_model=list[BlobSummary])
def list_blobs(
    container_name: str,
    user: str = Depends(get_current_user),
    gateway: StorageGateway = Depends(get_storage_gateway),
) -> list[BlobSummary]:
    # Interface Segregation Principle: the route only needs list_blobs from the
    # gateway contract, keeping each endpoint dependent on the smallest behavior.
    return gateway.list_blobs(container_name)


@app.post(
    "/api/storage/containers/{container_name}/blobs",
    response_model=BlobSummary,
    status_code=status.HTTP_201_CREATED,
)
def upload_blob(
    container_name: str,
    request: BlobUploadRequest,
    user: str = Depends(get_current_user),
    gateway: StorageGateway = Depends(get_storage_gateway),
) -> BlobSummary:
    # DTO pattern: request/response models define the API contract and keep
    # validation concerns outside the storage implementation.
    return gateway.upload_blob(
        container_name=container_name,
        blob_name=request.name,
        content=request.content,
        content_type=request.content_type,
        metadata=request.metadata,
        overwrite=request.overwrite,
    )


@app.put("/api/storage/containers/{container_name}/blobs/{blob_name}", response_model=BlobSummary)
def update_blob(
    container_name: str,
    blob_name: str,
    request: BlobUpdateRequest,
    user: str = Depends(get_current_user),
    gateway: StorageGateway = Depends(get_storage_gateway),
) -> BlobSummary:
    # REST Resource pattern: PUT replaces the named blob content while preserving
    # a stable resource URI.
    return gateway.update_blob(
        container_name=container_name,
        blob_name=blob_name,
        content=request.content,
        content_type=request.content_type,
        metadata=request.metadata,
    )


@app.delete(
    "/api/storage/containers/{container_name}/blobs/{blob_name}",
    response_model=StorageOperationResult,
)
def delete_blob(
    container_name: str,
    blob_name: str,
    user: str = Depends(get_current_user),
    gateway: StorageGateway = Depends(get_storage_gateway),
) -> StorageOperationResult:
    # Command pattern: encapsulates a storage mutation behind a clear endpoint
    # and returns a small operation result.
    gateway.delete_blob(container_name, blob_name)
    return StorageOperationResult(name=blob_name, status="deleted")


@app.exception_handler(ValueError)
def storage_not_found_handler(request, exc: ValueError):
    # Facade pattern: converts internal gateway errors into a consistent REST
    # response so route handlers stay focused on resource behavior.
    return JSONResponse(status_code=404, content={"detail": str(exc)})


if __name__ == "__main__":
    uvicorn.run("main:app", reload=True, host="0.0.0.0", port=8087)
