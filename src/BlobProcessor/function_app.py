"""
BlobProcessor — Azure Function (Python 3.11)

Triggered when a file is uploaded to Azure Blob Storage.
Demonstrates: Blob trigger, output binding to processed-files container,
logging, and error handling best practices.
"""

import logging
import json
import os
from datetime import datetime, UTC

import azure.functions as func

app = func.FunctionApp()

logger = logging.getLogger("BlobProcessor")


@app.blob_trigger(
    arg_name="input_blob",
    path="uploads/{name}",
    connection="AzureWebJobsStorage"
)
@app.blob_output(
    arg_name="output_blob",
    path="processed/{name}",
    connection="AzureWebJobsStorage"
)
def process_uploaded_blob(
    input_blob: func.InputStream,
    output_blob: func.Out[bytes]
) -> None:
    """
    Triggered on every new file uploaded to the 'uploads' container.
    Processes the file and writes result to the 'processed' container.

    In a real scenario this could:
    - Resize images (Pillow)
    - Extract text from PDFs
    - Run ML inference
    - Validate CSV schemas
    """
    blob_name = input_blob.name
    blob_size = input_blob.length or 0

    logger.info(
        "Processing blob: %s | Size: %d bytes | Triggered at: %s",
        blob_name,
        blob_size,
        datetime.now(UTC).isoformat()
    )

    try:
        content = input_blob.read()
        processed = _process_content(blob_name, content)
        output_blob.set(processed)

        logger.info("Blob %s processed and written to 'processed' container", blob_name)

    except Exception as exc:
        logger.error("Failed to process blob %s: %s", blob_name, str(exc), exc_info=True)
        raise  # Re-raise so Function runtime can handle retry/dead-letter


def _process_content(blob_name: str, content: bytes) -> bytes:
    """
    Core processing logic — extend this for your use case.
    Currently: wraps content in a JSON metadata envelope.
    """
    extension = os.path.splitext(blob_name)[1].lower()

    metadata = {
        "original_filename": blob_name,
        "processed_at": datetime.now(UTC).isoformat(),
        "size_bytes": len(content),
        "extension": extension,
        "status": "processed"
    }

    if extension in (".json",):
        # Validate and re-emit JSON files
        try:
            parsed = json.loads(content)
            metadata["record_count"] = len(parsed) if isinstance(parsed, list) else 1
        except json.JSONDecodeError as e:
            metadata["parse_error"] = str(e)

    elif extension in (".txt", ".csv"):
        lines = content.decode("utf-8", errors="replace").splitlines()
        metadata["line_count"] = len(lines)

    else:
        metadata["note"] = f"Binary file — extension: {extension}"

    result = {
        "metadata": metadata,
        "raw_size": len(content)
    }

    return json.dumps(result, indent=2).encode("utf-8")


# ── HTTP trigger for health check ──────────────────────────────────────────

@app.route(route="health", auth_level=func.AuthLevel.ANONYMOUS)
def health_check(req: func.HttpRequest) -> func.HttpResponse:
    """Simple health check endpoint."""
    return func.HttpResponse(
        json.dumps({"status": "healthy", "timestamp": datetime.now(UTC).isoformat()}),
        mimetype="application/json",
        status_code=200
    )
