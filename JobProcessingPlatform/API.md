# API Reference

## Base URL
```
https://localhost:7000/api
```

## Authentication
Include JWT token in request header:
```
Authorization: Bearer <token>
```

## Response Format
All responses are JSON:
```json
{
  "data": {},
  "error": null,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## Authentication Endpoints

### Register User
```http
POST /auth/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePassword123!"
}
```

**Response:** `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "username": "johndoe",
  "email": "john@example.com"
}
```

### Login
```http
POST /auth/login
Content-Type: application/json

{
  "username": "johndoe",
  "password": "SecurePassword123!"
}
```

**Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "username": "johndoe"
}
```

---

## Job Endpoints

### Create Job
```http
POST /jobs
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "ProcessUserData",
  "description": "Process batch of user records",
  "payloadJson": "{\"userId\": 123, \"batchSize\": 100}",
  "priority": 1,
  "maxRetries": 3,
  "initialDelaySeconds": 60
}
```

**Request Body:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| name | string | Yes | Job name (max 255 chars) |
| description | string | No | Job description |
| payloadJson | string | Yes | JSON payload for job |
| priority | integer | No | 0=Low, 1=Normal, 2=High, 3=Critical (default: 1) |
| maxRetries | integer | No | Max retry attempts (default: 3) |
| initialDelaySeconds | integer | No | Initial retry delay in seconds |

**Response:** `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001"
}
```

**Errors:**
- `400 Bad Request` — Invalid payload
- `401 Unauthorized` — Missing/invalid token

### Get Job by ID
```http
GET /jobs/{jobId}
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "name": "ProcessUserData",
  "description": "Process batch of user records",
  "status": "Running",
  "priority": 1,
  "createdBy": "550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2024-01-15T10:00:00Z",
  "startedAt": "2024-01-15T10:00:30Z",
  "completedAt": null,
  "resultJson": null,
  "errorMessage": null,
  "retryCount": 0
}
```

**Status Values:**
- `Pending` — Queued, not yet processed
- `Running` — Currently being processed
- `Completed` — Successfully completed
- `Failed` — Failed after max retries
- `Retrying` — Waiting for next retry attempt
- `Cancelled` — Manually cancelled

**Errors:**
- `404 Not Found` — Job doesn't exist
- `401 Unauthorized` — Not authenticated

### List Jobs
```http
GET /jobs?skip=0&take=10&status=Completed
Authorization: Bearer <token>
```

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| skip | integer | 0 | Number of results to skip (pagination) |
| take | integer | 10 | Number of results to return |
| status | string | null | Filter by status (Pending, Running, Completed, etc.) |

**Response:** `200 OK`
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "name": "ProcessUserData",
    "status": "Completed",
    ...
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "name": "SendNotifications",
    "status": "Completed",
    ...
  }
]
```

### Cancel Job
```http
DELETE /jobs/{jobId}
Authorization: Bearer <token>
```

**Response:** `204 No Content`

**Errors:**
- `404 Not Found` — Job doesn't exist
- `403 Forbidden` — Not authorized to cancel
- `400 Bad Request` — Job already completed/failed

---

## Statistics Endpoints

### Get Queue Statistics
```http
GET /stats
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "Pending": 42,
  "Running": 5,
  "Completed": 1250,
  "Failed": 8,
  "Retrying": 3,
  "Cancelled": 12,
  "QueueLength": 42
}
```

### Health Check
```http
GET /stats/health
```

**Response:** `200 OK`
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## Error Responses

### 400 Bad Request
```json
{
  "error": "Invalid job creation parameters: Job name cannot be empty",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 401 Unauthorized
```json
{
  "error": "Invalid credentials",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 404 Not Found
```json
{
  "error": "Job with ID 550e8400-e29b-41d4-a716-446655440000 not found",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 500 Internal Server Error
```json
{
  "error": "An unexpected error occurred",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

---

## Examples

### Create and Monitor a Job
```bash
# 1. Login
TOKEN=$(curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"AdminPassword123!"}' \
  | jq -r '.token')

# 2. Create job
JOB_ID=$(curl -X POST https://localhost:7000/api/jobs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "DataProcessing",
    "description": "Process customer data",
    "payloadJson": "{\"customerId\": 123}",
    "priority": 2
  }' | jq -r '.id')

# 3. Check job status
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:7000/api/jobs/$JOB_ID | jq

# 4. Get queue stats
curl -H "Authorization: Bearer $TOKEN" \
  https://localhost:7000/api/stats | jq
```

### Using Postman
1. **Set Bearer Token**
   - Authorization tab
   - Type: Bearer Token
   - Token: `<your-jwt-token>`

2. **POST Create Job**
   ```
   URL: {{base_url}}/api/jobs
   Body (JSON):
   {
     "name": "MyJob",
     "description": "Job description",
     "payloadJson": "{}",
     "priority": 1
   }
   ```

3. **GET Job Details**
   ```
   URL: {{base_url}}/api/jobs/{{jobId}}
   ```

---

## Rate Limiting
Currently not implemented. Consider adding:
- Rate limiting middleware
- Per-user quotas
- Sliding window algorithm

## Versioning
API version: `v1` (included in base URL path `/api/v1/`)

Future versions: `/api/v2/`, `/api/v3/`, etc.

---

For more details, see [README.md](README.md) and [ARCHITECTURE.md](ARCHITECTURE.md).
