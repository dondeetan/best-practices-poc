from typing import Protocol

from models import BlobSummary, ContainerSummary


class StorageGateway(Protocol):
    # Dependency Inversion Principle: route handlers depend on this abstraction
    # instead of directly depending on the Azure SDK client.
    def list_containers(self) -> list[ContainerSummary]:
        ...

    def create_container(self, name: str, metadata: dict[str, str]) -> ContainerSummary:
        ...

    def delete_container(self, name: str) -> None:
        ...

    def list_blobs(self, container_name: str) -> list[BlobSummary]:
        ...

    def upload_blob(
        self,
        container_name: str,
        blob_name: str,
        content: str,
        content_type: str,
        metadata: dict[str, str],
        overwrite: bool,
    ) -> BlobSummary:
        ...

    def update_blob(
        self,
        container_name: str,
        blob_name: str,
        content: str,
        content_type: str,
        metadata: dict[str, str],
    ) -> BlobSummary:
        ...

    def delete_blob(self, container_name: str, blob_name: str) -> None:
        ...


class AzureSasBlobStorageGateway:
    # Facade pattern: exposes REST-friendly storage operations while hiding the
    # lower-level Azure Blob SDK calls and SAS credential wiring.
    def __init__(self, account_url: str, sas_token: str):
        from azure.storage.blob import BlobServiceClient, ContentSettings

        # Adapter pattern: adapts Azure SDK clients/responses into this project's
        # StorageGateway protocol and Pydantic response models.
        self._content_settings_type = ContentSettings
        self._service_client = BlobServiceClient(account_url=account_url, credential=sas_token)

    def list_containers(self) -> list[ContainerSummary]:
        return [
            ContainerSummary(name=container["name"], last_modified=container.get("last_modified"))
            for container in self._service_client.list_containers()
        ]

    def create_container(self, name: str, metadata: dict[str, str]) -> ContainerSummary:
        container_client = self._service_client.create_container(name, metadata=metadata or None)
        properties = container_client.get_container_properties()
        return ContainerSummary(name=name, last_modified=properties.get("last_modified"))

    def delete_container(self, name: str) -> None:
        self._service_client.delete_container(name)

    def list_blobs(self, container_name: str) -> list[BlobSummary]:
        container_client = self._service_client.get_container_client(container_name)
        return [
            BlobSummary(
                name=blob["name"],
                size=blob.get("size") or 0,
                content_type=(blob.get("content_settings") or {}).get("content_type"),
                last_modified=blob.get("last_modified"),
            )
            for blob in container_client.list_blobs()
        ]

    def upload_blob(
        self,
        container_name: str,
        blob_name: str,
        content: str,
        content_type: str,
        metadata: dict[str, str],
        overwrite: bool,
    ) -> BlobSummary:
        blob_client = self._service_client.get_blob_client(container=container_name, blob=blob_name)
        blob_client.upload_blob(
            content.encode("utf-8"),
            overwrite=overwrite,
            metadata=metadata or None,
            content_settings=self._content_settings_type(content_type=content_type),
        )
        properties = blob_client.get_blob_properties()
        return BlobSummary(
            name=blob_name,
            size=properties.get("size") or len(content.encode("utf-8")),
            content_type=properties.get("content_settings", {}).get("content_type") or content_type,
            last_modified=properties.get("last_modified"),
        )

    def update_blob(
        self,
        container_name: str,
        blob_name: str,
        content: str,
        content_type: str,
        metadata: dict[str, str],
    ) -> BlobSummary:
        # DRY/Open-Closed Principle: update reuses the upload behavior with
        # overwrite enabled, so upload details stay in one extension point.
        return self.upload_blob(
            container_name=container_name,
            blob_name=blob_name,
            content=content,
            content_type=content_type,
            metadata=metadata,
            overwrite=True,
        )

    def delete_blob(self, container_name: str, blob_name: str) -> None:
        self._service_client.get_blob_client(container=container_name, blob=blob_name).delete_blob()
