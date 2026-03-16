# UI Integration Guide

This guide explains how to integrate the RittDocConverter API with your UI application.

## Overview

The RittDocConverter system provides:
1. **Webhook notifications** - Real-time notifications when conversions complete
2. **Download endpoints** - Direct file downloads by ISBN or job ID
3. **Status endpoints** - Query conversion status and metadata

---

## Webhook Integration

### Webhook Endpoint (Your UI Must Implement)

Your UI backend must expose this endpoint to receive notifications:

```
POST /api/files/webhook/complete
Content-Type: application/json
```

### Webhook Payload Structure

```json
{
  "jobId": "9781234567890",
  "status": "completed",
  "fileType": "epub",
  "metadata": {
    "isbn": "9781234567890",
    "output_zip": "/app/Output/9781234567890_all_fixes.zip",
    "validation_passed": true,
    "errors_fixed": 15,
    "remaining_errors": 0,
    "report_path": "/app/Output/9781234567890_validation_report.xlsx"
  },
  "downloadUrls": {
    "package": "http://epub-converter-api:5001/api/v1/files/9781234567890/package",
    "report": "http://epub-converter-api:5001/api/v1/files/9781234567890/report",
    "info": "http://epub-converter-api:5001/api/v1/files/9781234567890"
  }
}
```

### Payload Fields

| Field | Type | Description |
|-------|------|-------------|
| `jobId` | string | Job identifier (usually ISBN) |
| `status` | string | `"completed"` or `"failed"` |
| `fileType` | string | `"epub"` or `"pdf"` |
| `metadata.isbn` | string | ISBN of the converted book |
| `metadata.output_zip` | string | Server-side path to output ZIP (for reference) |
| `metadata.validation_passed` | boolean | Whether DTD validation passed |
| `metadata.errors_fixed` | number | Number of errors auto-fixed |
| `metadata.remaining_errors` | number | Number of unresolved errors |
| `metadata.report_path` | string | Server-side path to validation report |
| `metadata.error` | string | Error message (only if `status: "failed"`) |
| `downloadUrls.package` | string | Full URL to download the ZIP package |
| `downloadUrls.report` | string | Full URL to download the validation report |
| `downloadUrls.info` | string | Full URL to get file info JSON |

### Example Webhook Handler (Node.js/Express)

```javascript
app.post('/api/files/webhook/complete', (req, res) => {
  const { jobId, status, fileType, metadata, downloadUrls } = req.body;

  console.log(`Conversion ${status} for ${jobId}`);

  if (status === 'completed') {
    // Store download URLs for later use
    await db.conversions.update(jobId, {
      status: 'completed',
      packageUrl: downloadUrls.package,
      reportUrl: downloadUrls.report,
      validationPassed: metadata.validation_passed,
      errorsFixes: metadata.errors_fixed
    });

    // Optionally download files immediately
    // const packageResponse = await fetch(downloadUrls.package);
    // const reportResponse = await fetch(downloadUrls.report);
  } else {
    await db.conversions.update(jobId, {
      status: 'failed',
      error: metadata.error
    });
  }

  res.status(200).json({ received: true });
});
```

---

## Download Endpoints

### Base URL

| Environment | Base URL |
|-------------|----------|
| Docker (internal) | `http://epub-converter-api:5001` |
| Docker (external) | `http://localhost:5001` |
| Production | Configure via `EPUB_API_URL` env var |

### GET /api/v1/files/{isbn}/package

Download the converted ZIP package.

**Request:**
```
GET /api/v1/files/9781234567890/package
```

**Response:**
- **Success (200):** Binary ZIP file download
- **Not Found (404):**
  ```json
  {"error": "No package found for ISBN: 9781234567890"}
  ```

**File Naming Priority:**
1. `{isbn}_all_fixes.zip` (preferred - fully validated)
2. `{isbn}_fixed.zip`
3. `{isbn}.zip`
4. `{isbn}_NEEDS_REVIEW.zip` (has unresolved errors)

---

### GET /api/v1/files/{isbn}/report

Download the validation report (Excel file).

**Request:**
```
GET /api/v1/files/9781234567890/report
```

**Response:**
- **Success (200):** Binary Excel file download (`.xlsx`)
- **Not Found (404):**
  ```json
  {"error": "No validation report found for ISBN: 9781234567890"}
  ```

**Report Contents:**
- Sheet 1: Error details (file, line, error type, description)
- Sheet 2: Verification checklist (manual review items)
- Summary row with pre/post fix error counts

---

### GET /api/v1/files/{isbn}

Get information about available files for an ISBN.

**Request:**
```
GET /api/v1/files/9781234567890
```

**Response (200):**
```json
{
  "isbn": "9781234567890",
  "files": {
    "package": {
      "exists": true,
      "filename": "9781234567890_all_fixes.zip",
      "size_mb": 2.45
    },
    "report": {
      "exists": true,
      "filename": "9781234567890_validation_report.xlsx",
      "size_mb": 0.12
    }
  },
  "download_urls": {
    "package": "/api/v1/files/9781234567890/package",
    "report": "/api/v1/files/9781234567890/report"
  }
}
```

---

## Job-Based Endpoints (Alternative)

If you have a job ID from the conversion API (not the editor), use these endpoints:

### GET /api/v1/jobs/{job_id}

Get job status and metadata.

**Response:**
```json
{
  "job_id": "abc123",
  "status": "COMPLETED",
  "progress": 100,
  "input_file": "/path/to/input.epub",
  "created_at": "2024-01-15T10:30:00Z",
  "completed_at": "2024-01-15T10:32:15Z",
  "result": {
    "output_file": "/app/Output/9781234567890_all_fixes.zip",
    "output_size_mb": 2.45
  }
}
```

### GET /api/v1/download/{job_id}

Download ZIP by job ID.

### GET /api/v1/download/{job_id}/report

Download validation report by job ID.

---

## Dashboard/Statistics Endpoints

### GET /api/v1/mongodb/dashboard

Get full dashboard data including statistics and recent conversions.

**Response:**
```json
{
  "statistics": {
    "total": 150,
    "successful": 142,
    "failed": 8,
    "success_rate": 94.67,
    "by_type": {"ePub": 120, "PDF": 30}
  },
  "recent": [...],
  "failed": [...]
}
```

### GET /api/v1/mongodb/conversions

Query conversions with filters.

**Query Parameters:**
- `status` - Filter by status: `Success`, `Failure`, `In Progress`
- `type` - Filter by type: `ePub`, `PDF`
- `start_date` - ISO date string
- `end_date` - ISO date string
- `limit` - Max results (default: 100)

---

## Environment Configuration

### Editor Service Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `UI_BACKEND_URL` | `http://demo-ui-backend:3001` | Your UI backend URL for webhooks |
| `EPUB_API_URL` | `http://epub-converter-api:5001` | API server URL (used in webhook download URLs) |

### Docker Compose Example

```yaml
services:
  epub-editor:
    environment:
      - UI_BACKEND_URL=http://your-ui-backend:3001
      - EPUB_API_URL=http://epub-converter-api:5001

  epub-converter-api:
    ports:
      - "5001:5001"
```

---

## Complete Integration Flow

```
┌─────────────┐     1. Upload EPUB      ┌──────────────────┐
│   UI App    │ ───────────────────────>│  Converter API   │
└─────────────┘                         └──────────────────┘
                                                 │
                                                 │ 2. Convert
                                                 v
┌─────────────┐     3. Edit Package     ┌──────────────────┐
│   UI App    │ <───────────────────────│     Editor       │
└─────────────┘                         └──────────────────┘
       │                                         │
       │ 4. Save                                 │
       v                                         v
┌─────────────┐     5. Webhook POST     ┌──────────────────┐
│ UI Backend  │ <───────────────────────│     Editor       │
└─────────────┘                         └──────────────────┘
       │
       │ 6. Download files using URLs from webhook
       v
┌─────────────┐     GET /files/{isbn}/* ┌──────────────────┐
│ UI Backend  │ ───────────────────────>│  Converter API   │
└─────────────┘                         └──────────────────┘
       │
       │ 7. Store/serve files
       v
┌─────────────┐
│   UI App    │  (Display to user)
└─────────────┘
```

---

## Error Handling

### Webhook Failures

The converter will log webhook failures but won't fail the conversion. Implement retry logic if needed:

```javascript
// Your webhook handler should return 200 quickly
app.post('/api/files/webhook/complete', async (req, res) => {
  // Acknowledge immediately
  res.status(200).json({ received: true });

  // Process asynchronously
  setImmediate(async () => {
    try {
      await processConversion(req.body);
    } catch (error) {
      console.error('Failed to process webhook:', error);
      // Queue for retry
    }
  });
});
```

### Download Failures

Always check for 404 responses when downloading:

```javascript
async function downloadPackage(isbn) {
  const response = await fetch(`${API_URL}/api/v1/files/${isbn}/package`);

  if (response.status === 404) {
    const error = await response.json();
    throw new Error(error.error);
  }

  if (!response.ok) {
    throw new Error(`Download failed: ${response.status}`);
  }

  return response.blob();
}
```

---

## TypeScript Types

```typescript
interface WebhookPayload {
  jobId: string;
  status: 'completed' | 'failed';
  fileType: 'epub' | 'pdf';
  metadata: {
    isbn: string;
    output_zip?: string;
    validation_passed?: boolean;
    errors_fixed?: number;
    remaining_errors?: number;
    report_path?: string;
    error?: string;
  };
  downloadUrls: {
    package: string;
    report: string;
    info: string;
  };
}

interface FileInfo {
  isbn: string;
  files: {
    package: { exists: boolean; filename: string | null; size_mb: number | null };
    report: { exists: boolean; filename: string | null; size_mb: number | null };
  };
  download_urls: {
    package: string;
    report: string;
  };
}
```

---

## Testing

### Test Webhook Locally

```bash
# Simulate a webhook call to your UI backend
curl -X POST http://localhost:3001/api/files/webhook/complete \
  -H "Content-Type: application/json" \
  -d '{
    "jobId": "9781234567890",
    "status": "completed",
    "fileType": "epub",
    "metadata": {
      "isbn": "9781234567890",
      "validation_passed": true,
      "errors_fixed": 5,
      "remaining_errors": 0
    },
    "downloadUrls": {
      "package": "http://localhost:5001/api/v1/files/9781234567890/package",
      "report": "http://localhost:5001/api/v1/files/9781234567890/report",
      "info": "http://localhost:5001/api/v1/files/9781234567890"
    }
  }'
```

### Test Download Endpoints

```bash
# Check file availability
curl http://localhost:5001/api/v1/files/9781234567890

# Download package
curl -O http://localhost:5001/api/v1/files/9781234567890/package

# Download report
curl -O http://localhost:5001/api/v1/files/9781234567890/report
```

---

## Support

For issues or questions about integration:
- Check API health: `GET /api/v1/health`
- View API docs: `GET /api/v1/openapi.yaml`
- Service info: `GET /api/v1/service-info`
