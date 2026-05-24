# Request–Response Flow

## API được phân tích

```
GET /api/enrollments?search=active&sort=-enrollDate&page=1&size=20&fields=enrollmentId,status&expand=student,course
```

API này là phức tạp nhất vì kích hoạt **đồng thời** toàn bộ pipeline:
search (filter qua navigation property) · sort dynamic · pagination · field shaping · expansion multi-level

---

## Sơ đồ tổng quan

```
Client
  │
  │  HTTP GET :8080/api/enrollments?search=active&sort=-enrollDate&...
  ▼
┌─────────────────────────────────────────────────────────────┐
│                    Docker Container                         │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              ASP.NET Core Middleware Pipeline        │  │
│  │                                                      │  │
│  │  1. GlobalExceptionMiddleware  ◄── try/catch toàn bộ│  │
│  │  2. UseSwagger / UseSwaggerUI                        │  │
│  │  3. UseHttpsRedirection  (bỏ qua – HTTP only)        │  │
│  │  4. UseAuthorization                                 │  │
│  │  5. MapControllers  ──► route matching               │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
  │
  ▼
┌─────────────────────────────────┐
│     API Layer (Controller)      │
│  EnrollmentsController.GetAll() │
└────────────────┬────────────────┘
                 │ gọi service
                 ▼
┌─────────────────────────────────┐
│     Service Layer               │
│  EnrollmentService.GetAllAsync()│
│  · build filter expression      │
│  · CountAsync (total items)     │
│  · build includes list          │
│  · QueryHelper.ApplySort        │
│  · GetAllAsync (paged data)     │
│  · Map Entity → BM → Response   │
└────────────────┬────────────────┘
                 │ gọi repository
                 ▼
┌─────────────────────────────────┐
│     Repository Layer            │
│  GenericRepository<Enrollment>  │
│  · Where(filter)                │
│  · Include(Student/Course/...)  │
│  · OrderBy(dynamic)             │
│  · Skip / Take                  │
│  · ToListAsync() → SQL          │
└────────────────┬────────────────┘
                 │ EF Core query
                 ▼
        ┌────────────────┐
        │   PostgreSQL   │
        │   (Docker)     │
        └────────────────┘
                 │ resultset
                 ▲
                 │
┌─────────────────────────────────┐
│     API Layer (trở về)          │
│  DataShaper<EnrollmentResponse> │
│  · ShapeData(data, "fields")    │
│  · Reflection → ExpandoObject   │
└────────────────┬────────────────┘
                 │
                 ▼
        JSON Response (camelCase)
  { success, message, data, pagination }
  │
  ▼
Client
```

---

## Chi tiết từng bước

### Bước 1 — HTTP Request vào Docker

Client gửi request tới `http://localhost:8080`. Docker map port `8080:8080` vào container. Kestrel nhận request, bắt đầu pipeline.

---

### Bước 2 — GlobalExceptionMiddleware

```csharp
// Middleware/GlobalExceptionMiddleware.cs
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);   // ← toàn bộ pipeline chạy trong này
    }
    catch (Exception ex)
    {
        await HandleExceptionAsync(context, ex);
    }
}
```

Toàn bộ pipeline từ đây trở xuống được bọc trong `try/catch`. Nếu bất kỳ tầng nào ném exception:

| Exception type       | HTTP code | Log level       | Message gửi client                              |
|----------------------|-----------|-----------------|--------------------------------------------------|
| `NotFoundException`  | 404       | `LogWarning`    | "Enrollment with ID '99' was not found."        |
| `BadRequestException`| 400       | `LogWarning`    | Message từ business logic                        |
| Bất kỳ exception khác| 500      | `LogError` (full stack trace) | "An unexpected error occurred..." |

---

### Bước 3 — Routing → EnrollmentsController

ASP.NET Core routing khớp:
- Method: `GET`
- Path: `/api/enrollments`
- → `EnrollmentsController.GetAll([FromQuery] QueryParameters)`

Model binding parse query string vào `QueryParameters`:

```
search   = "active"
sort     = "-enrollDate"
page     = 1
size     = 20
fields   = "enrollmentId,status"
expand   = "student,course"
```

---

### Bước 4 — Controller gọi Service

```csharp
// Controllers/EnrollmentsController.cs
public async Task<IActionResult> GetAll([FromQuery] QueryParameters queryParams)
{
    var (data, pagination) = await _enrollmentService.GetAllAsync(queryParams);
    var shapedData = _dataShaper.ShapeData(data, queryParams.Fields);
    return Ok(ApiResponse<object>.SuccessResponse(shapedData, "...", pagination));
}
```

Controller **không có logic nghiệp vụ**. Nó chỉ:
1. Gọi service lấy data
2. Gọi DataShaper để lọc fields
3. Đóng gói vào `ApiResponse` và trả về

---

### Bước 5 — EnrollmentService.GetAllAsync

#### 5a. Xây dựng includes

```csharp
var includes = new List<string> { "Student", "Course" };
// expand chứa "course" → thêm Semester
includes.Add("Course.Semester");
// includes cuối: ["Student", "Course", "Course.Semester"]
```

#### 5b. Xây dựng filter expression

```csharp
var searchLower = "active";
filter = e =>
    e.Status.ToLower().Contains("active") ||
    e.Student.FullName.ToLower().Contains("active") ||
    e.Course.CourseName.ToLower().Contains("active");
```

Filter tham chiếu navigation properties (`e.Student`, `e.Course`) — EF Core sẽ tự sinh JOIN khi dịch sang SQL.

#### 5c. CountAsync (đếm tổng để tính pagination)

```csharp
int totalItems = await _enrollmentRepository.CountAsync(filter);
// SQL: SELECT COUNT(*) FROM "Enrollment" e
//      JOIN "Student" s ON e."StudentId" = s."StudentId"
//      JOIN "Course" c ON e."CourseId" = c."CourseId"
//      WHERE lower(e."Status") LIKE '%active%'
//         OR lower(s."FullName") LIKE '%active%'
//         OR lower(c."CourseName") LIKE '%active%'

var pagination = new PaginationMetadata(page: 1, pageSize: 20, totalItems);
// → { page:1, pageSize:20, totalItems:X, totalPages:ceil(X/20) }
```

#### 5d. Xây dựng orderBy qua QueryHelper.ApplySort

```csharp
// sort = "-enrollDate" → dấu "-" = descending
orderBy = q => (IOrderedQueryable<Enrollment>)
    QueryHelper.ApplySort(q, "-enrollDate");

// QueryHelper dùng System.Linq.Dynamic.Core:
// "-enrollDate" → strip "-" → "enrollDate" → OrderByDescending
// q.OrderBy("enrollDate descending")
```

#### 5e. GetAllAsync từ Repository

```csharp
var enrollments = await _enrollmentRepository.GetAllAsync(
    filter: filter,
    orderBy: orderBy,
    includeProperties: ["Student", "Course", "Course.Semester"],
    page: 1,
    pageSize: 20
);
```

#### 5f. Map Entity → BusinessModel → Response

```csharp
enrollments.Select(e => MapToResponse(e, "student,course"))
```

Với mỗi `Enrollment`:
1. `MapToBusinessModel(e)` → `EnrollmentBM` (tách biệt domain logic với persistence)
2. Tạo `EnrollmentResponse` từ BM
3. Vì `expand` chứa `"student"` → gắn thêm `StudentResponse`
4. Vì `expand` chứa `"course"` → gắn thêm `CourseResponse` (kèm `SemesterName`)

---

### Bước 6 — GenericRepository.GetAllAsync

```csharp
IQueryable<Enrollment> query = _dbSet;          // FROM "Enrollment"

query = query.Where(filter);                     // WHERE ...

query = query.Include("Student")                 // JOIN Student
             .Include("Course")                  // JOIN Course
             .Include("Course.Semester");         // JOIN Semester qua Course

query = orderBy(query);                          // ORDER BY "EnrollDate" DESC

int skip = (1 - 1) * 20; // = 0
query = query.Skip(0).Take(20);                  // OFFSET 0 LIMIT 20

return await query.ToListAsync();                // thực thi SQL, trả List<Enrollment>
```

**SQL EF Core sinh ra (tóm tắt):**

```sql
SELECT e.*, s.*, c.*, sem.*
FROM   "Enrollment" e
       JOIN "Student" s   ON e."StudentId"  = s."StudentId"
       JOIN "Course"  c   ON e."CourseId"   = c."CourseId"
       JOIN "Semester" sem ON c."SemesterId" = sem."SemesterId"
WHERE  lower(e."Status")       LIKE '%active%'
    OR lower(s."FullName")     LIKE '%active%'
    OR lower(c."CourseName")   LIKE '%active%'
ORDER  BY e."EnrollDate" DESC
LIMIT  20 OFFSET 0;
```

---

### Bước 7 — DataShaper (trở về Controller)

```csharp
_dataShaper.ShapeData(data, "enrollmentId,status")
```

`DataShaper<EnrollmentResponse>` dùng **Reflection** lúc runtime:

```
EnrollmentResponse có 8 properties:
  EnrollmentId, StudentId, StudentName, CourseId, CourseName,
  EnrollDate, Status, Student, Course

fields = "enrollmentId,status"
→ lọc chỉ giữ: EnrollmentId, Status

Mỗi item → ExpandoObject { enrollmentId: 5, status: "Active" }
```

Kết quả: `IEnumerable<ExpandoObject>` chỉ có 2 field được yêu cầu.

---

### Bước 8 — Đóng gói ApiResponse và Serialize

```csharp
return Ok(ApiResponse<object>.SuccessResponse(shapedData, "...", pagination));
```

JSON serialization với `System.Text.Json` (cấu hình trong `Program.cs`):
- `PropertyNamingPolicy = CamelCase` → `enrollmentId` (không phải `EnrollmentId`)
- `ReferenceHandler = IgnoreCycles` → không bị vòng lặp circular reference
- `DefaultIgnoreCondition = WhenWritingNull` → bỏ qua field null

**Response trả về client:**

```json
{
  "success": true,
  "message": "Enrollments retrieved successfully",
  "data": [
    { "enrollmentId": 12, "status": "Active" },
    { "enrollmentId": 47, "status": "Active" },
    ...
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 243,
    "totalPages": 13
  }
}
```

---

## Luồng khi có lỗi (Error Path)

```
Ví dụ: GET /api/enrollments/9999
       → EnrollmentService.GetByIdAsync(9999)
       → _enrollmentRepository.GetByIdAsync(9999) trả về null
       → throw new NotFoundException("Enrollment", 9999)
       → exception bubble up qua Service → Controller
       → GlobalExceptionMiddleware.HandleExceptionAsync() bắt lại
       → _logger.LogWarning("[404 Not Found] GET /api/enrollments/9999 — ...")
       → context.Response.StatusCode = 404
       → trả JSON: { "success": false, "message": "Enrollment with ID '9999' was not found." }
```

---

## Tóm tắt phân công trách nhiệm

| Tầng              | File chính                        | Trách nhiệm                                              |
|-------------------|-----------------------------------|----------------------------------------------------------|
| Middleware        | `GlobalExceptionMiddleware.cs`    | Bắt exception, map HTTP code, log BE                     |
| Controller        | `EnrollmentsController.cs`        | Parse request, gọi service, gọi DataShaper, trả response |
| DataShaper        | `DataShaper.cs`                   | Lọc fields bằng Reflection → ExpandoObject               |
| Service           | `EnrollmentService.cs`            | Business logic: filter, sort, pagination, mapping        |
| QueryHelper       | `QueryHelper.cs`                  | Dynamic sort bằng System.Linq.Dynamic.Core               |
| Repository        | `GenericRepository.cs`            | Dịch sang EF Core query: Where/Include/OrderBy/Skip/Take |
| DbContext         | `LMSDbContext.cs`                 | Cấu hình entity mapping, foreign key, index              |
| Database          | PostgreSQL (Docker)               | Thực thi SQL, trả resultset                              |
