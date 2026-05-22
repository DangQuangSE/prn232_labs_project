# HƯỚNG DẪN TRIỂN KHAI HỆ THỐNG LMS LÊN DOCKER
> Dự án PRN232 LMS System RESTful API sử dụng mô hình 3 lớp (3-layer architecture), kết nối với cơ sở dữ liệu PostgreSQL và được đóng gói hoàn toàn bằng Docker Compose.

Tệp tài liệu này hướng dẫn chi tiết cách chạy dự án (Backend API + Database) trên bất kỳ máy tính nào khác chỉ với một vài câu lệnh đơn giản.

---

## 1. Yêu Cầu Cài Đặt Ban Đầu (Prerequisites)
Trước khi khởi chạy dự án ở máy tính mới, hãy đảm bảo máy tính đó đã cài đặt:
1. **Docker Desktop:** [Tải xuống tại đây](https://www.docker.com/products/docker-desktop/) (Đã được kích hoạt WSL2 backend đối với hệ điều hành Windows).
2. **Git:** Để nhân bản dự án (nếu cần).
3. *(Không bắt buộc)* Một công cụ quản trị Database như **DBeaver** hoặc **pgAdmin** nếu bạn muốn kết nối trực tiếp vào DB để truy vấn.

---

## 2. Hướng Dẫn Khởi Chạy Hệ Thống (Deployment Steps)

Hãy làm theo các bước sau để khởi chạy cả Database và Web API:

### Bước 1: Mở Terminal tại thư mục gốc của dự án
Di chuyển vào thư mục chứa tệp `docker-compose.yml` (ví dụ: `PRN232.LMSSystem/`).

### Bước 2: Chạy lệnh Docker Compose
Mở PowerShell hoặc Command Prompt (CMD) và thực thi lệnh sau:
```bash
docker compose up -d --build
```
> **Ý nghĩa của lệnh:**
> - `-d` (detached mode): Chạy ngầm các container dưới nền, giải phóng cửa sổ terminal.
> - `--build`: Docker sẽ tiến hành build lại hình ảnh (image) cho Backend API dựa trên mã nguồn mới nhất.

### Bước 3: Xác minh các container đang chạy
Chạy lệnh sau để đảm bảo cả hai container đều đang ở trạng thái `Up` (đang hoạt động):
```bash
docker ps
```
Bạn sẽ nhìn thấy 2 container đang chạy:
* `lms_web_api` tại cổng `8080` (hoặc `8081`).
* `lms_postgres_db` tại cổng `5432`.

---

## 3. Cách Thức Tự Động Nạp Dữ Liệu (Database Seeding)
Dự án đã được cấu hình tự động hóa hoàn toàn cơ sở dữ liệu:
- Khi container database khởi chạy **lần đầu tiên**, Docker sẽ quét thư mục `/docker-entrypoint-initdb.d/` bên trong container và tự động thực thi tệp [**`init.sql`**](init.sql).
- Tệp `init.sql` sẽ tự động tạo đầy đủ cấu trúc các bảng dạng số ít (`Semester`, `Course`, `Subject`, `Student`, `Enrollment`) và sinh **500 bản ghi dữ liệu mẫu** ngẫu nhiên.

### ⚠️ Cách Reset lại Database về trạng thái sạch ban đầu:
Nếu trong quá trình thử nghiệm bạn làm thay đổi dữ liệu hoặc cấu trúc bảng trong `init.sql` và muốn chạy lại sạch sẽ từ đầu, hãy chạy lệnh sau:
```bash
# Tắt container và xóa Volume lưu trữ dữ liệu cũ
docker compose down -v

# Khởi chạy lại từ đầu và nạp lại script init.sql mới
docker compose up -d --build
```
*(Tham số `-v` cực kỳ quan trọng để xóa các ổ đĩa ảo lưu trữ tạm thời của Postgres, ép Postgres phải khởi tạo lại từ đầu).*

---

## 4. Các Thông Tin Kết Nối Quan Trọng

### 🌐 1. Truy cập Web API & Swagger UI
Khi hệ thống đã chạy thành công, bạn mở trình duyệt web và truy cập:
* **Swagger UI Documentation:** [**http://localhost:8080/swagger**](http://localhost:8080/swagger)
* Tại đây, bạn có thể thực hiện kiểm thử đầy đủ các endpoint RESTful API (Search, Filter, Sort, Pagination).

### 🗄️ 2. Kết nối trực tiếp tới Database (PostgreSQL)
Bạn có thể kết nối tới cơ sở dữ liệu PostgreSQL đang chạy trong Docker qua các phần mềm quản lý như DBeaver, pgAdmin bằng thông tin sau:
* **Host:** `localhost`
* **Port:** `5432`
* **Database Name:** ``
* **Username:** ``
* **Password:** ``

---

## 5. Hướng Dẫn Xử Lý Sự Cố (Troubleshooting)

### Lỗi 1: Không lấy được dữ liệu hoặc API trả về lỗi 500
* **Nguyên nhân phổ biến:** Bạn điền nhầm các tham số tìm kiếm/sắp xếp của bảng khác vào endpoint không tương ứng (ví dụ: sắp xếp theo `-enrollDate` trên API của `Courses`).
* **Cách sửa:** Đảm bảo nhập đúng các trường thuộc tính của thực thể tương ứng, hoặc để trống các ô lọc để kiểm tra dữ liệu gốc trước.

### Lỗi 2: Muốn kiểm tra nhật ký lỗi (Logs) của hệ thống
Nếu gặp lỗi không rõ nguyên nhân, bạn có thể xem log trực tiếp từ các container bằng các lệnh sau:
- **Xem log của Web API:**
  ```bash
  docker logs lms_web_api
  ```
- **Xem log của Database:**
  ```bash
  docker logs lms_postgres_db
  ```
- **Xem log thời gian thực (real-time):** thêm tham số `-f` (ví dụ: `docker logs -f lms_web_api`).
