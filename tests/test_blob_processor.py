"""
Unit tests for BlobProcessor function.
Run with: pytest tests/ -v
"""

import json
import pytest
from unittest.mock import MagicMock
from src.BlobProcessor.function_app import _process_content


class TestProcessContent:

    def test_json_file_adds_record_count(self):
        """JSON files should have record_count in metadata."""
        data = json.dumps([{"id": 1}, {"id": 2}, {"id": 3}]).encode()
        result = json.loads(_process_content("orders.json", data))

        assert result["metadata"]["extension"] == ".json"
        assert result["metadata"]["record_count"] == 3
        assert result["metadata"]["status"] == "processed"

    def test_csv_file_adds_line_count(self):
        """CSV files should have line_count in metadata."""
        data = b"id,name\n1,Alice\n2,Bob\n3,Charlie\n"
        result = json.loads(_process_content("users.csv", data))

        assert result["metadata"]["extension"] == ".csv"
        assert result["metadata"]["line_count"] == 4  # includes header

    def test_binary_file_adds_note(self):
        """Binary files should have a descriptive note."""
        data = b"\x89PNG\r\n\x1a\n"  # PNG magic bytes
        result = json.loads(_process_content("image.png", data))

        assert "note" in result["metadata"]
        assert ".png" in result["metadata"]["note"]

    def test_metadata_always_present(self):
        """Every processed blob should have core metadata fields."""
        data = b"hello world"
        result = json.loads(_process_content("test.txt", data))

        assert "original_filename" in result["metadata"]
        assert "processed_at" in result["metadata"]
        assert "size_bytes" in result["metadata"]
        assert result["metadata"]["size_bytes"] == len(data)

    def test_invalid_json_captures_error(self):
        """Malformed JSON files should capture parse error gracefully."""
        data = b"{not valid json"
        result = json.loads(_process_content("bad.json", data))

        assert "parse_error" in result["metadata"]
