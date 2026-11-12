# Báo Cáo Kiểm Thử & Minh Chứng Chất Lượng (Phase 2 – Tuần 07)

## 1. Tổ chức Project Kiểm thử (`tests/QTKD.Api.Tests`) – Bước 14

Dự án kiểm thử xUnit (`tests/QTKD.Api.Tests`) được tách thành 2 nhóm rõ ràng:

1. **Unit Tests (`tests/QTKD.Api.Tests/Unit/`)**:
   - Kiểm thử trực tiếp logic nghiệp vụ trong `CustomerService` và `ContractService` thông qua **Fake In-Memory Repositories** ([`FakeRepositories.cs`](../tests/QTKD.Api.Tests/Unit/Fakes/FakeRepositories.cs)).
   - Hoàn toàn độc lập với cơ sở dữ liệu và HTTP, tập trung kiểm chứng các quy tắc **CTR-01, CTR-02, CTR-03** và **AC05** với cả trường hợp hợp lệ (Valid) và vi phạm (Invalid).
2. **Integration Tests (`tests/QTKD.Api.Tests/Integration/`)**:
   - Sử dụng `WebApplicationFactory<Program>` ([`CustomWebApplicationFactory.cs`](../tests/QTKD.Api.Tests/Integration/CustomWebApplicationFactory.cs)) để khởi chạy pipeline ASP.NET Core thật và gửi HTTP request qua `HttpClient`.
   - **Cơ sở dữ liệu kiểm thử riêng biệt (`qtkd_test_db`)**: Tự động chạy `EnsureDeletedAsync()` + `MigrateAsync()` trên database PostgreSQL riêng có tên `qtkd_test_db`, tuyệt đối không chạm vào dữ liệu của database chính `qtkd_db` (Bước 16).
   - **Kiểm tra 3 tầng ở mỗi Test Case (Bước 17)**:
     1. Kiểm tra **HTTP Status Code** (`200`, `201`, `400`, `409`, `422`).
     2. Kiểm tra **nội dung JSON phản hồi** (`CustomerResponse`, `ContractResponse`, `SubmitContractResponse`, `ValidationProblemDetails`, `ProblemDetails`).
     3. Kiểm tra **dữ liệu thực tế trong PostgreSQL (`qtkd_test_db`)** sau khi gọi API.

---

## 2. Bảng Kết Quả Kiểm Thử TC01 – TC08 (Mục 5.3 & 5.4)

Toàn bộ **8/8 Test Case tối thiểu (TC01–TC08)** cùng các test mở rộng (tổng cộng **24 automated tests**: 15 Unit Tests – tính cả 4 trường hợp `[Theory]` của TC02 – và 9 Integration Tests) đều được **tự động hóa 100%** bằng xUnit và đạt kết quả **PASS**:

| Mã Test | Dữ liệu / Thao tác | Kết quả mong đợi | Loại Test | Hàm Test Tự Động Tương Ứng | Kết Quả |
|---|---|---|---|---|---|
| **TC01** | Tạo Customer hợp lệ | Tạo được (`201 Created`) và đọc lại khớp từ DB | Tự động (Unit + Integration) | `TC01_Unit_CreateCustomer_WithValidData_CreatesAndReadsBackCustomer`<br>`TC01_Integration_CreateValidCustomer_Returns201AndPersistsInDatabase` | **PASS** |
| **TC02** | Tạo Customer thiếu thông tin bắt buộc | Bị từ chối (`400 Bad Request`); có thông báo lỗi chi tiết; không lưu DB | Tự động (Unit + Integration) | `TC02_Unit_CreateCustomer_MissingRequiredFields_ThrowsValidationException` (4 cases)<br>`TC02_Integration_CreateCustomerMissingRequiredFields_Returns400WithValidationErrorsAndDoesNotSave` | **PASS** |
| **TC03** | Sửa Contract ở trạng thái `Draft` (hoặc `RevisionRequested`) | Thành công (`200 OK`) khi dữ liệu hợp lệ; DB cập nhật đúng | Tự động (Unit + Integration) | `CTR01_Valid_TC03_UpdateContract_WhenInDraft_UpdatesSuccessfully`<br>`CTR01_Valid_UpdateContract_WhenInRevisionRequested_UpdatesSuccessfully`<br>`TC03_Integration_UpdateContractInDraft_WithValidData_Returns200AndUpdatesDatabase` | **PASS** |
| **TC04** | Sửa Contract ở trạng thái `PendingApproval` (**CTR-01**) | Bị từ chối (`422 Unprocessable Entity`); dữ liệu trong DB giữ nguyên | Tự động (Unit + Integration) | `CTR01_Invalid_TC04_UpdateContract_WhenInPendingApproval_ThrowsBusinessRuleExceptionAndPreservesData`<br>`TC04_Integration_UpdateContractInPendingApproval_Returns422AndPreservesDatabaseState` | **PASS** |
| **TC05** | Submit Contract thiếu điều kiện **CTR-02** | Bị từ chối (`400 Bad Request`); trạng thái trong DB vẫn là `Draft` | Tự động (Unit + Integration) | `CTR02_Invalid_TC05_SubmitContract_WhenMissingContentAndAttachment_ThrowsValidationExceptionAndRemainsDraft`<br>`CTR02_Invalid_SubmitContract_WhenCustomerIsNotActive_ThrowsValidationExceptionAndRemainsDraft`<br>`TC05_Integration_SubmitContractMissingCtr02Conditions_Returns400AndRemainsDraftInDatabase` | **PASS** |
| **TC06** | Submit Contract hợp lệ (**CTR-02 & CTR-03**) | Thành công (`200 OK`); chuyển sang `PendingApproval` và lưu `SubmittedAt` | Tự động (Unit + Integration) | `CTR02_Valid_TC06_SubmitContract_WhenAllConditionsMet_TransitionsToPendingApproval`<br>`TC06_Integration_SubmitValidContract_WithoutRequestBody_Returns200AndTransitionsToPendingApproval` | **PASS** |
| **TC07** | Thử chuyển `Draft` thẳng sang `Approved` (**CTR-03**) | Bị từ chối (`422 Unprocessable Entity`); vẫn là `Draft` | Tự động (Unit + Integration) | `CTR03_Invalid_TC07_UpdateContract_TryTransitionDraftDirectlyToApproved_ThrowsBusinessRuleException`<br>`CTR03_Invalid_CreateContract_WithApprovedStatus_ThrowsBusinessRuleException`<br>`TC07_Integration_TryTransitionDraftDirectlyToApproved_Returns422AndRemainsDraft` | **PASS** |
| **TC08** | Submit lặp cùng một Contract (**AC05**) | Bị từ chối (`409 Conflict`); không tạo thêm kết quả xử lý trùng, giữ nguyên `SubmittedAt` | Tự động (Unit + Integration) | `TC08_Unit_SubmitContractTwice_SecondCallThrowsConflictAndDoesNotDuplicateProcessing`<br>`TC08_Integration_SubmitContractTwice_SecondCallReturns409AndDoesNotOverwriteSubmission` | **PASS** |
| **E2E** | Luồng xuyên suốt: Tạo Customer → Tạo Contract Draft → Submit lỗi → Bổ sung → Submit thành công → Tạo Bảng giá | Chạy mượt toàn bộ kịch bản Bước 13 & Bước 18 trên DB thật | Tự động (Integration) | `EndToEnd_FullWorkflow_CreateCustomer_CreateDraft_SubmitFail_Update_SubmitSuccess_And_Pricing` | **PASS** |

---

## 3. Nhật Ký Phát Hiện & Sửa Lỗi Trong Quá Trình Kiểm Thử (Mục 5.4)

### Lỗi #1: Gọi `POST /api/contracts/{id}/submit` không kèm Request Body trả về `415 Unsupported Media Type`

- **Phát hiện bởi test**: `TC06_Integration_SubmitValidContract_WithoutRequestBody_Returns200AndTransitionsToPendingApproval` trong [`QtkdApiIntegrationTests.cs`](../tests/QTKD.Api.Tests/Integration/QtkdApiIntegrationTests.cs).
- **Cách tái hiện**:
  1. Tạo một hợp đồng ở trạng thái `Draft` đã có đầy đủ điều kiện CTR-02 (`Customer` Active, `StartDate <= EndDate`, có `ContentSummary` và `AttachmentUrl`).
  2. Gửi request `POST /api/contracts/{id}/submit` mà không truyền body và không đính kèm header `Content-Type: application/json` (ví dụ: `await client.PostAsync($"/api/contracts/{id}/submit", content: null)` hoặc `curl -X POST http://localhost:5007/api/contracts/1/submit`), đúng theo đặc tả US03 ở Phase 1 (*"Body (nếu cần): { 'comment': '...' }"*).
  3. **Kết quả trước khi sửa**: API trả về `415 Unsupported Media Type` thay vì `200 OK`.
- **Nguyên nhân gốc rễ (Root Cause)**:
  - Trong `ContractsController.Submit`, tham số `[FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] SubmitContractRequest? request` khiến bộ lọc `Consumes` mặc định của `[ApiController]` trong ASP.NET Core yêu cầu request bắt buộc phải có header `Content-Type: application/json` ngay cả khi `Content-Length = 0`. Khi client gọi POST hành động `/submit` không có body và không gửi `Content-Type`, pipeline chặn lại ở bước kiểm tra Media Type với mã `415`.
- **Cách khắc phục & Commit sửa lỗi**:
  - Bổ sung middleware nhẹ trong [`Program.cs`](../src/QTKD.Api/Program.cs): khi request có `ContentLength` là `null` hoặc `0` và chưa có `ContentType`, tự động gán `ContentType = "application/json"` để `System.Text.Json` kết hợp với `EmptyBodyBehavior.Allow` ánh xạ `request = null` hợp lệ mà vẫn giữ nguyên schema body tùy chọn trên giao diện Swagger UI.
  - **Commit phát hiện lỗi (Test)**: `a3e1234` (`test(week07): add xUnit unit and integration test suite (TC01-TC08) with isolated qtkd_test_db`)
  - **Commit sửa lỗi (Fix)**: `e95be7a` (`fix(week07): allow empty body without Content-Type on POST /api/contracts/{id}/submit (found by TC06)`)
  - **Kết quả sau khi sửa**: chạy lại `dotnet test` → **24/24 PASS**.

### Lỗi #2 (môi trường build): xung đột phiên bản EF Core giữa project test và project API

- **Hiện tượng**: `dotnet test` báo lỗi biên dịch `CS1705` – `QTKD.Api` dùng `Microsoft.EntityFrameworkCore 9.0.10` nhưng project test tham chiếu bản `9.0.1` (kéo theo gián tiếp).
- **Khắc phục**: Khai báo tường minh `Microsoft.EntityFrameworkCore` và `Microsoft.EntityFrameworkCore.Relational` bản `9.0.10` trong [`QTKD.Api.Tests.csproj`](../tests/QTKD.Api.Tests/QTKD.Api.Tests.csproj) (cùng commit `e95be7a`).

---

## 4. Hướng dẫn chạy kiểm thử và xuất file minh chứng TRX

```powershell
# Đảm bảo container PostgreSQL đang chạy
docker compose up -d

# Chạy toàn bộ 24 Unit Tests & Integration Tests và lưu kết quả ra file TRX
dotnet test QTKD.sln --logger "console;verbosity=normal" --logger "trx;LogFileName=test_results.trx" --results-directory docs/test-results
```

File kết quả kiểm thử tự động được lưu tại: [docs/test-results/test_results.trx](test-results/test_results.trx).

