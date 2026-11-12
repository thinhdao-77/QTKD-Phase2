# Hệ Thống Quản Trị Kinh Doanh – QTKD API (Phase 2 – Tuần 05, 06, 07)

Dự án xây dựng phân hệ Backend **ASP.NET Core Web API (.NET 9)** kết nối **PostgreSQL** qua **Entity Framework Core** và bộ kiểm thử tự động **xUnit** cho đề tài **Quản trị Kinh doanh (QTKD)** – Công ty Logistics ABC.

---

## 1. Cấu trúc Solution & Kiến trúc Phân tầng (Mục 6)

```text
ThucHanhPhase2/
├── QTKD.sln                                  # Solution xuyên suốt Tuần 05 – 07
├── docker-compose.yml                        # Khởi chạy PostgreSQL 17 container (qtkd_db & qtkd_test_db)
├── .env.example                              # Mẫu cấu hình mật khẩu PostgreSQL
├── .config/dotnet-tools.json                 # Công cụ cục bộ dotnet-ef 9.0.10
├── docs/
│   ├── architecture.md                       # [Tuần 05] Thiết kế kiến trúc thành phần & ERD
│   ├── week06_api_and_rules.md               # [Tuần 06] Tài liệu API, US03 & đối chiếu CTR-01..03
│   ├── week07_testing_report.md              # [Tuần 07] Báo cáo kiểm thử TC01..TC08 & nhật ký sửa lỗi
│   └── test-results/test_results.trx         # [Tuần 07] Minh chứng kết quả chạy dotnet test (TRX)
├── src/
│   └── QTKD.Api/
│       ├── Controllers/                      # API Layer: CustomersController, ContractsController, PriceListsController
│       ├── DTOs/                             # Request/Response DTOs (Customers/, Contracts/, PriceLists/)
│       ├── Services/                         # Business Logic Layer: Kiểm tra quy tắc CTR-01, CTR-02, CTR-03, US03
│       ├── Mappings/                         # Chuyển đổi Entity <-> DTO
│       ├── Repositories/                     # Data Access Layer: Truy vấn EF Core (AsNoTracking, Include)
│       ├── Data/                             # QtkdDbContext & Migrations (InitialCreate + Seed Data)
│       ├── Entities/                         # Customer, Contract, PriceList, PriceItem
│       ├── Exceptions/                       # Xử lý lỗi tập trung theo chuẩn RFC 7807 ProblemDetails
│       └── Program.cs                        # Cấu hình DI, Middleware pipeline & Swagger UI
└── tests/
    └── QTKD.Api.Tests/
        ├── Unit/                             # Unit Tests cho CTR-01, CTR-02, CTR-03 với Fake Repository
        │   ├── Fakes/FakeRepositories.cs
        │   └── ContractBusinessRuleUnitTests.cs
        └── Integration/                      # Integration Tests gọi API & DB kiểm thử riêng (qtkd_test_db)
            ├── CustomWebApplicationFactory.cs
            └── QtkdApiIntegrationTests.cs
```

### Tài liệu chi tiết theo từng tuần
- **Tuần 05 (Kiến trúc & ERD)**: [docs/architecture.md](docs/architecture.md)
- **Tuần 06 (Đặc tả API, US03 & Bảng đối chiếu CTR-01..03)**: [docs/week06_api_and_rules.md](docs/week06_api_and_rules.md)
- **Tuần 07 (Báo cáo kiểm thử TC01–TC08, Nhật ký phát hiện/sửa lỗi & File TRX)**: [docs/week07_testing_report.md](docs/week07_testing_report.md)

---

## 2. Hướng dẫn Cài đặt, Chạy Migration, Chạy API & Kiểm thử (Clone → Cấu hình → Migration → Chạy API → Chạy Test)

### Bước 1: Tạo file `.env` và khởi động PostgreSQL bằng Docker

```powershell
# Copy file .env.example thành .env (mặc định POSTGRES_PASSWORD=123456)
Copy-Item .env.example .env

# Khởi động container PostgreSQL (qtkd-postgres)
docker compose up -d
```

### Bước 2: Cấu hình Connection String an toàn qua User Secrets

```powershell
dotnet user-secrets set "ConnectionStrings:QtkdDb" "Host=localhost;Port=5432;Database=qtkd_db;Username=postgres;Password=123456" --project src/QTKD.Api
```

### Bước 3: Khôi phục công cụ `dotnet-ef` và cập nhật Database chính (`qtkd_db`)

```powershell
dotnet restore
dotnet tool restore
dotnet ef database update --project src/QTKD.Api
```

### Bước 4: Khởi động Web API

```powershell
dotnet run --project src/QTKD.Api --launch-profile http
```

- Ứng dụng lắng nghe tại: `http://localhost:5007`
- Giao diện **Swagger UI**: `http://localhost:5007/swagger`
- Kịch bản HTTP Demo tuần tự (Tuần 05 & Tuần 06): [src/QTKD.Api/QTKD.Api.http](src/QTKD.Api/QTKD.Api.http)

### Bước 5: Chạy bộ Kiểm thử Tự động (Unit Test + Integration Test) – Tuần 07

Bộ Integration Test tự động khởi tạo và chạy migration trên cơ sở dữ liệu kiểm thử riêng biệt **`qtkd_test_db`**, không làm ảnh hưởng đến dữ liệu của **`qtkd_db`**.

```powershell
# Chạy toàn bộ 24 Unit Tests & Integration Tests và xuất file TRX minh chứng
dotnet test QTKD.sln --logger "console;verbosity=normal" --logger "trx;LogFileName=test_results.trx" --results-directory docs/test-results
```

---

## 3. Tổng hợp các Endpoint API

| Phân hệ | Method | Endpoint | Chức năng & Quy tắc kiểm tra | Status Code |
|---|---|---|---|---|
| **Customer** | `GET` | `/api/customers` | Xem danh sách khách hàng | `200` |
| **Customer** | `GET` | `/api/customers/{id}` | Xem chi tiết khách hàng | `200`, `404` |
| **Customer** | `POST` | `/api/customers` | Tạo mới khách hàng (`TC01`, `TC02`) | `201`, `400`, `409` |
| **Customer** | `PUT` | `/api/customers/{id}` | Cập nhật khách hàng | `200`, `400`, `404` |
| **Contract** | `GET` | `/api/contracts` | Xem danh sách hợp đồng | `200` |
| **Contract** | `GET` | `/api/contracts/{id}` | Xem chi tiết hợp đồng | `200`, `404` |
| **Contract** | `POST` | `/api/contracts` | Tạo hợp đồng ở trạng thái `Draft` (kiểm tra `Customer` `Active`, `StartDate <= EndDate`, **CTR-03**) | `201`, `400`, `409`, `422` |
| **Contract** | `PUT` | `/api/contracts/{id}` | Cập nhật hợp đồng (**CTR-01**: chỉ sửa ở `Draft`/`RevisionRequested` – `TC03`, `TC04`; **CTR-03**: cấm chuyển thẳng `Approved` – `TC07`) | `200`, `400`, `404`, `422` |
| **Contract (US03)** | `POST` | `/api/contracts/{id}/submit` | Gửi hợp đồng trình duyệt (**CTR-02** – `TC05`, `TC06`; **CTR-03**; chống gửi lặp **AC05** – `TC08`) | `200`, `400`, `404`, `409` |
| **Pricing** | `GET` | `/api/pricelists` | Xem danh sách bảng giá kèm `PriceItem` | `200` |
| **Pricing** | `GET` | `/api/pricelists/{id}` | Xem chi tiết bảng giá | `200`, `404` |
| **Pricing** | `POST` | `/api/pricelists` | Tạo bảng giá mới (**PRC-02**: `EffectiveDate <= ExpiryDate`) | `201`, `400` |
| **Pricing** | `GET` | `/api/pricelists/{id}/items` | Xem danh sách `PriceItem` của bảng giá | `200`, `404` |
| **Pricing** | `POST` | `/api/pricelists/{id}/items` | Thêm `PriceItem` vào bảng giá (`UnitPrice > 0`, không trùng `ServiceCode`) | `201`, `400`, `404`, `409` |

---

## 4. Ghi nhận lỗi đã phát hiện, khắc phục và giới hạn hiện tại (Bước 18)

1. **Lỗi đã phát hiện và khắc phục qua Integration Test (`TC06`)**:
   - *Hiện tượng*: Khi gọi `POST /api/contracts/{id}/submit` không kèm JSON body và không có header `Content-Type: application/json`, ASP.NET Core trả về `415 Unsupported Media Type` mặc dù đặc tả US03 quy định phần `comment` trong body là tùy chọn.
   - *Khắc phục*: Đã bổ sung xử lý cho request rỗng trong `Program.cs` để kết hợp mượt mà với `EmptyBodyBehavior.Allow`. Toàn bộ 24/24 test (bao gồm `TC01–TC08`) đều đạt **PASS**.
2. **Các giới hạn trong phạm vi hiện tại (Tuần 05–07)**:
   - Theo đúng phạm vi bắt buộc của đề bài (Mục 2), hệ thống hiện mô phỏng việc khởi tạo phiên phê duyệt qua trạng thái `PendingApproval`, dấu thời gian `SubmittedAt` và mã `ApprovalId = "APR-{ContractNo}"`; chưa triển khai đầy đủ phân hệ `Approval` nhiều cấp, `Payment`, giao diện web hoặc tích hợp chữ ký điện tử bên thứ ba.
