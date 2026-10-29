# Tài Liệu Kiến Trúc & Thiết Kế Hệ Thống QTKD (Phase 2 – Tuần 05)

## 1. Bối cảnh và Phạm vi Tuần 05
Hệ thống **Quản trị Kinh doanh (QTKD)** hỗ trợ Công ty Logistics ABC quản lý vòng đời hồ sơ kinh doanh từ **Khách hàng (`Customer`)**, **Hợp đồng (`Contract`)**, **Bảng giá (`PriceList` / `PriceItem`)** đến quy trình trình duyệt hợp đồng (**US03**).

Trong **Tuần 05**, mục tiêu trọng tâm là:
1. Xây dựng nền tảng kiến trúc phân tầng rõ ràng (**API Layer – Business Logic Layer – Data Access Layer**).
2. Thiết kế mô hình dữ liệu (`Customer`, `Contract`, `PriceList`, `PriceItem`) bám sát đặc tả Phase 1 và chuẩn bị sẵn các trường phục vụ kiểm tra quy tắc nghiệp vụ **CTR-01, CTR-02, CTR-03** ở Tuần 06.
3. Cấu hình `QtkdDbContext` với **Entity Framework Core + PostgreSQL**, quản lý chuỗi kết nối an toàn qua **User Secrets** (không lộ mật khẩu trên Git).
4. Khởi tạo Migration (`InitialCreate`), cập nhật schema vào PostgreSQL và nạp dữ liệu mẫu (Seed Data) theo đúng kịch bản xuyên suốt (*Công ty Minh Phát – Hợp đồng HD-2026-018*).
5. Cung cấp các API đọc dữ liệu (`GET /api/customers`, `GET /api/contracts/{id}`, ...) và đối chiếu với dữ liệu thực tế trong PostgreSQL.

---

## 2. Sơ đồ các thành phần chính của Solution (Component Architecture)

```mermaid
flowchart TB
    subgraph Client["Client (Swagger UI / Postman / REST Client)"]
        REQ["HTTP Request"]
    end

    subgraph ApiLayer["1. API Layer (Controllers & DTOs)"]
        CTRL_C["CustomersController"]
        CTRL_CT["ContractsController"]
        CTRL_PL["PriceListsController"]
        EX["GlobalExceptionHandler (RFC 7807 ProblemDetails)"]
    end

    subgraph BusinessLayer["2. Business Logic Layer (Services & Mappings)"]
        SVC_C["CustomerService"]
        SVC_CT["ContractService"]
        SVC_PL["PriceListService"]
        MAP["Entity <-> DTO Mappings"]
    end

    subgraph DataLayer["3. Data Access Layer (Repositories & EF Core)"]
        REPO_C["CustomerRepository"]
        REPO_CT["ContractRepository"]
        REPO_PL["PriceListRepository"]
        DBCTX["QtkdDbContext"]
    end

    subgraph Database["PostgreSQL Database (qtkd_db)"]
        TBL_C[("customers")]
        TBL_CT[("contracts")]
        TBL_PL[("price_lists")]
        TBL_PI[("price_items")]
    end

    REQ --> CTRL_C & CTRL_CT & CTRL_PL
    CTRL_C --> SVC_C
    CTRL_CT --> SVC_CT
    CTRL_PL --> SVC_PL
    SVC_C & SVC_CT & SVC_PL -.-> MAP
    SVC_C --> REPO_C
    SVC_CT --> REPO_CT
    SVC_PL --> REPO_PL
    REPO_C & REPO_CT & REPO_PL --> DBCTX
    DBCTX --> TBL_C & TBL_CT & TBL_PL & TBL_PI
    SVC_C & SVC_CT & SVC_PL -. "throw AppException" .-> EX
```

### Phân định trách nhiệm giữa các tầng

| Tầng (Layer) | Thành phần | Trách nhiệm chính | Nguyên tắc thiết kế |
|---|---|---|---|
| **API Layer** | `Controllers/`, `DTOs/`, `Exceptions/` | Tiếp nhận HTTP Request, trả về DTO & HTTP Status Code chuẩn (`200 OK`, `404 Not Found` qua `ProblemDetails`). | Không chứa câu truy vấn DB, không gọi trực tiếp `QtkdDbContext`. |
| **Business Logic Layer** | `Services/`, `Mappings/` | Điều phối luồng nghiệp vụ, kiểm tra quy tắc nghiệp vụ (`CTR-01`, `CTR-02`, `CTR-03`), chuyển đổi giữa `Entity` và `DTO`. | Độc lập với HTTP (`IActionResult`) và độc lập với EF Core để dễ viết Unit Test ở Tuần 07. |
| **Data Access Layer** | `Repositories/`, `Data/`, `Entities/` | Đóng gói truy vấn EF Core (`AsNoTracking`, `Include`), cấu hình Fluent API, quản lý Migration và Seed Data. | Chỉ tập trung đọc/ghi dữ liệu, không tự quyết định quy tắc nghiệp vụ. |

---

## 3. Sơ đồ Quan hệ Thực thể (ERD) trong PostgreSQL

```mermaid
erDiagram
    customers ||--o{ contracts : "1 Customer có N Contract (FK: CustomerId, Restrict)"
    price_lists ||--|{ price_items : "1 PriceList có N PriceItem (FK: PriceListId, Cascade)"

    customers {
        int Id PK
        varchar(50) CustomerCode UK "Mã khách hàng (VD: KH-001)"
        varchar(200) Name "Tên khách hàng"
        varchar(50) TaxCode "Mã số thuế"
        varchar(300) Address "Địa chỉ"
        varchar(150) ContactEmail "Email liên hệ"
        varchar(50) Status "Active | Suspended | Inactive"
        timestamp CreatedAt "DEFAULT now()"
    }

    contracts {
        int Id PK
        varchar(50) ContractNo UK "Số hợp đồng (VD: HD-2026-018)"
        int CustomerId FK "Tham chiếu customers(Id)"
        date StartDate "Ngày bắt đầu hiệu lực"
        date EndDate "Ngày kết thúc hiệu lực"
        varchar(100) ServiceGroup "Nhóm dịch vụ (VD: Dịch vụ cảng)"
        varchar(1000) ContentSummary "Tóm tắt nội dung bắt buộc (phục vụ CTR-02)"
        varchar(500) AttachmentUrl "Đường dẫn tệp đính kèm bắt buộc (phục vụ CTR-02)"
        varchar(50) Status "Draft | PendingApproval | RevisionRequested | Approved | Rejected"
        timestamp SubmittedAt "Thời điểm gửi trình duyệt"
        timestamp CreatedAt "DEFAULT now()"
    }

    price_lists {
        int Id PK
        varchar(200) Name "Tên bảng giá"
        date EffectiveDate "Ngày bắt đầu hiệu lực"
        date ExpiryDate "Ngày hết hiệu lực"
        varchar(50) Status "Draft | Effective | Expired"
        timestamp CreatedAt "DEFAULT now()"
    }

    price_items {
        int Id PK
        int PriceListId FK "Tham chiếu price_lists(Id)"
        varchar(50) ServiceCode "Mã dịch vụ (Unique trong cùng PriceList)"
        varchar(200) ServiceName "Tên dịch vụ"
        varchar(50) Unit "Đơn vị tính"
        numeric(18_2) UnitPrice "Đơn giá"
    }
```

---

## 4. Liên kết với Kết quả Phase 1 (US03 & Business Rules)

Để đảm bảo schema tạo ở Tuần 05 sử dụng xuyên suốt sang Tuần 06 và Tuần 07 mà không phải đập đi xây lại, mô hình `Contract` và `Customer` đã tích hợp sẵn các thuộc tính phục vụ **US03 (Gửi hợp đồng trình duyệt)**:
- **CTR-01** (*Chỉ được chỉnh sửa hợp đồng ở trạng thái `Draft` hoặc `RevisionRequested`*): Kiểm soát qua trường `Contract.Status` (`ContractStatus` constants).
- **CTR-02** (*Hợp đồng chỉ được Submit khi có khách hàng hợp lệ, thời gian hiệu lực hợp lệ và có ít nhất một tệp/hạng mục nội dung bắt buộc*):
  - Khách hàng hợp lệ: `Customer` tồn tại và `Customer.Status == "Active"`.
  - Thời gian hiệu lực hợp lệ: `StartDate <= EndDate`.
  - Nội dung & tệp đính kèm bắt buộc: `ContentSummary` và `AttachmentUrl` (đã khai báo sẵn trên bảng `contracts`).
- **CTR-03** (*Không được chuyển trực tiếp từ `Draft` sang `Approved`*): Luồng Submit chỉ chuyển `Status` sang `PendingApproval` và ghi nhận `SubmittedAt`.

