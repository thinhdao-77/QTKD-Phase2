# Tài Liệu Đặc Tả API & Đối Chiếu Quy Tắc Nghiệp Vụ (Phase 2 – Tuần 06)

## 1. Đặc tả chi tiết US03 – Gửi hợp đồng trình duyệt (Bước 8)

### 1.1. Nội dung User Story (US03)
> **Là** Nhân viên Kinh doanh,  
> **Tôi muốn** gửi hợp đồng đã hoàn thiện vào quy trình phê duyệt,  
> **Để** hợp đồng được các bên có thẩm quyền rà soát và phê duyệt trước khi có hiệu lực.

### 1.2. Điều kiện gửi hợp đồng, Trạng thái trước/sau và Trường hợp từ chối

| Hạng mục | Quy định chi tiết |
|---|---|
| **Trạng thái trước khi gửi** | Hợp đồng phải tồn tại và đang ở trạng thái **`Draft`** (Bản nháp) hoặc **`RevisionRequested`** (Yêu cầu chỉnh sửa). |
| **Điều kiện bắt buộc (CTR-02)** | 1. **Khách hàng hợp lệ**: `Customer` tham chiếu phải tồn tại và có `Status == "Active"`.<br>2. **Thời gian hiệu lực hợp lệ**: `StartDate <= EndDate`.<br>3. **Đủ nội dung bắt buộc**: Trường `ContentSummary` không được rỗng.<br>4. **Đủ tệp đính kèm bắt buộc**: Trường `AttachmentUrl` không được rỗng. |
| **Trạng thái sau khi gửi thành công (CTR-03)** | - Chuyển trạng thái hợp đồng sang **`PendingApproval`** (Chờ phê duyệt) — tuyệt đối không chuyển thẳng sang `Approved`.<br>- Ghi nhận thời điểm gửi `SubmittedAt` (UTC) và khởi tạo mã phiên phê duyệt mô phỏng `ApprovalId = "APR-{ContractNo}"`. |
| **Các trường hợp phải từ chối** | - **404 Not Found**: Không tìm thấy hợp đồng theo `id`.<br>- **400 Bad Request (`ValidationProblemDetails`)**: Vi phạm **CTR-02** (thiếu `ContentSummary`, thiếu `AttachmentUrl`, `StartDate > EndDate`, hoặc `Customer` không `Active`). Trả về danh sách đầy đủ các điều kiện chưa đạt, **giữ nguyên trạng thái `Draft`**. <br>- **409 Conflict (`ProblemDetails`)**: Hợp đồng đã ở trạng thái `PendingApproval` (**AC05 – gửi lặp**) hoặc đang ở trạng thái không cho phép gửi (`Approved`, `Rejected`, `Active`, `Cancelled`, `Expired` – **AC02**). |

### 1.3. Giả định dùng trong bản Demo & Giới hạn phiên bản hiện tại (Bước 12 & Mục 4.4)
1. **Giả định về nội dung/tệp bắt buộc (CTR-02)**:
   - *Giả định trong bản demo*: Một hợp đồng đủ điều kiện trình duyệt khi có mặt đồng thời tóm tắt điều khoản (`ContentSummary`) và đường dẫn hồ sơ đính kèm (`AttachmentUrl`).
   - *Phần cần xác nhận thêm với nghiệp vụ*: Danh sách loại tệp bắt buộc cụ thể theo từng nhóm dịch vụ (`ServiceGroup`: Dịch vụ cảng, Vận chuyển nội địa, Lưu kho) và dung lượng/định dạng tệp tối đa.
2. **Cách mô phỏng việc gửi trình duyệt & Giới hạn của phiên bản hiện tại**:
   - Do ở Tuần 06 chưa yêu cầu xây dựng đầy đủ phân hệ `Approval Workflow Engine` nhiều bước, hành động gửi trình duyệt được mô phỏng bằng cách:
     - Chuyển trạng thái thực thể `Contract` sang `PendingApproval` và lưu dấu thời gian `SubmittedAt` trong PostgreSQL.
     - Sinh mã tham chiếu phiên phê duyệt `ApprovalId = "APR-{ContractNo}"` trả về trong phản hồi `SubmitContractResponse`.
   - **Xử lý chống gửi lặp (Double Submit – Bước 12 / AC05 / TC08)**: Khi gọi `POST /api/contracts/{id}/submit` lần thứ hai cho cùng một hợp đồng đã ở trạng thái `PendingApproval`, Service kiểm tra trạng thái hiện tại và từ chối ngay với mã `409 Conflict`, đảm bảo không ghi đè `SubmittedAt` và không sinh thêm phiên phê duyệt trùng lặp.

---

## 2. Bảng Đối Chiếu US03 & Quy Tắc Nghiệp Vụ CTR-01..03 Với Mã Nguồn (Mục 4.4 & 4.5)

| Mã Quy Tắc / Tiêu Chí | Ý Nghĩa Nghiệp Vụ | Vị Trí Xử Lý Trong Code | Cách Kiểm Tra & Kết Quả Mong Đợi |
|---|---|---|---|
| **CTR-01** | Chỉ cho phép chỉnh sửa hợp đồng khi đang ở trạng thái `Draft` hoặc `RevisionRequested`. | [`ContractService.UpdateAsync`](../src/QTKD.Api/Services/ContractService.cs) | Gọi `PUT /api/contracts/{id}` trên hợp đồng đã `Submit` (`PendingApproval`) → Bị từ chối với **`422 Unprocessable Entity`**, dữ liệu trong DB giữ nguyên. |
| **CTR-02** *(US03 – AC03)* | Chỉ cho phép Submit khi `Customer` hợp lệ (`Active`), `StartDate <= EndDate`, và có đủ `ContentSummary` + `AttachmentUrl`. | [`ContractService.SubmitAsync`](../src/QTKD.Api/Services/ContractService.cs) | Gọi `POST /api/contracts/{id}/submit` khi thiếu `ContentSummary` hoặc `AttachmentUrl` → Bị từ chối với **`400 Bad Request`** (kèm danh sách chi tiết từng lỗi), trạng thái trong DB vẫn là `Draft`. |
| **CTR-03** *(US03 – AC04)* | Không được chuyển trực tiếp từ `Draft` sang `Approved`. Hợp đồng phải qua bước `PendingApproval`. | [`ContractService.CreateAsync`](../src/QTKD.Api/Services/ContractService.cs)<br>[`ContractService.UpdateAsync`](../src/QTKD.Api/Services/ContractService.cs)<br>[`ContractService.SubmitAsync`](../src/QTKD.Api/Services/ContractService.cs) | 1. Thử truyền `"status": "Approved"` vào `POST /api/contracts` hoặc `PUT /api/contracts/{id}` → Bị từ chối **`422 Unprocessable Entity`**.<br>2. Gọi `POST /api/contracts/{id}/submit` thành công → Trạng thái chỉ chuyển sang **`PendingApproval`**. |
| **US03 – AC02** | Từ chối gửi trình duyệt nếu hợp đồng không ở trạng thái cho phép gửi. | [`ContractService.SubmitAsync`](../src/QTKD.Api/Services/ContractService.cs) | Gọi `POST /api/contracts/{id}/submit` cho hợp đồng không ở `Draft`/`RevisionRequested` → Bị từ chối **`409 Conflict`**. |
| **US03 – AC05** *(TC08)* | Gửi lặp cùng một hợp đồng hai lần không được tạo thêm một lần xử lý tương đương. | [`ContractService.SubmitAsync`](../src/QTKD.Api/Services/ContractService.cs) | Gọi `POST /api/contracts/{id}/submit` lần thứ 2 liên tiếp → Bị từ chối **`409 Conflict`**, không đổi `SubmittedAt`, không tạo thêm phiên duyệt mới. |
| **PRC-02 & Pricing** | Bảng giá phải có `EffectiveDate <= ExpiryDate`; `PriceItem` phải liên kết đúng `PriceList` tồn tại và `UnitPrice > 0`. | [`PriceListService.CreateAsync`](../src/QTKD.Api/Services/PriceListService.cs)<br>[`PriceListService.AddItemAsync`](../src/QTKD.Api/Services/PriceListService.cs) | Tạo `PriceItem` với `UnitPrice <= 0` → Bị từ chối **`400 Bad Request`**. Thêm trùng `ServiceCode` trong cùng `PriceList` → Bị từ chối **`409 Conflict`**. |

---

## 3. Danh Sách API & Mẫu Request / Response (Bước 9 & Mục 4.5)

### 3.1. Phân hệ Khách hàng (`Customer`)

| Method | Endpoint | Chức năng | Status Code |
|---|---|---|---|
| `GET` | `/api/customers` | Xem danh sách khách hàng | `200 OK` |
| `GET` | `/api/customers/{id}` | Xem chi tiết khách hàng | `200 OK`, `404 Not Found` |
| `POST` | `/api/customers` | Tạo mới khách hàng | `201 Created`, `400 Bad Request`, `409 Conflict` |
| `PUT` | `/api/customers/{id}` | Cập nhật thông tin & trạng thái khách hàng | `200 OK`, `400 Bad Request`, `404 Not Found` |

#### Mẫu Tạo Khách hàng (`POST /api/customers`)
- **Request Body**:
```json
{
  "customerCode": "KH-003",
  "name": "Công ty TNHH Tiếp Vận Sài Gòn",
  "taxCode": "0315558899",
  "address": "88 Nguyễn Huệ, Quận 1, TP.HCM",
  "contactEmail": "ops@saigonlogistics.vn",
  "status": "Active"
}
```
- **Response Thành công (`201 Created`)**:
```json
{
  "id": 3,
  "customerCode": "KH-003",
  "name": "Công ty TNHH Tiếp Vận Sài Gòn",
  "taxCode": "0315558899",
  "address": "88 Nguyễn Huệ, Quận 1, TP.HCM",
  "contactEmail": "ops@saigonlogistics.vn",
  "status": "Active",
  "createdAt": "2026-11-05T10:25:00Z"
}
```

---

### 3.2. Phân hệ Hợp đồng (`Contract` & `US03`)

| Method | Endpoint | Chức năng | Status Code |
|---|---|---|---|
| `GET` | `/api/contracts` | Xem danh sách hợp đồng | `200 OK` |
| `GET` | `/api/contracts/{id}` | Xem chi tiết hợp đồng | `200 OK`, `404 Not Found` |
| `POST` | `/api/contracts` | Tạo mới hợp đồng ở trạng thái `Draft` | `201 Created`, `400 Bad Request`, `409 Conflict`, `422 Unprocessable Entity` |
| `PUT` | `/api/contracts/{id}` | Cập nhật hợp đồng (kiểm tra `CTR-01`, `CTR-03`) | `200 OK`, `400 Bad Request`, `404 Not Found`, `422 Unprocessable Entity` |
| `POST` | `/api/contracts/{id}/submit` | **US03**: Gửi hợp đồng trình duyệt (`CTR-02`, `CTR-03`, `AC05`) | `200 OK`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |

#### Mẫu Gửi hợp đồng trình duyệt (`POST /api/contracts/{id}/submit`)
- **Request Body (tùy chọn)**:
```json
{
  "comment": "Đã bổ sung đầy đủ điều khoản dịch vụ cảng và file scan hợp đồng."
}
```
- **Response Thành công (`200 OK`)**:
```json
{
  "contractId": 3,
  "contractNo": "HD-2026-020",
  "previousStatus": "Draft",
  "status": "PendingApproval",
  "approvalId": "APR-HD-2026-020",
  "submittedAt": "2026-11-05T10:30:00Z",
  "comment": "Đã bổ sung đầy đủ điều khoản dịch vụ cảng và file scan hợp đồng.",
  "message": "Hợp đồng 'HD-2026-020' đã được gửi trình duyệt thành công và chuyển sang trạng thái 'PendingApproval'."
}
```
- **Response Lỗi khi vi phạm CTR-02 (`400 Bad Request` – `ValidationProblemDetails`)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "[CTR-02] Hợp đồng chưa đáp ứng đủ điều kiện bắt buộc để gửi trình duyệt.",
  "errors": {
    "ContentSummary": [
      "[CTR-02] Thiếu nội dung hợp đồng bắt buộc (ContentSummary)."
    ],
    "AttachmentUrl": [
      "[CTR-02] Thiếu tệp đính kèm bắt buộc (AttachmentUrl)."
    ]
  }
}
```
- **Response Lỗi khi gửi lặp hoặc sai trạng thái (`409 Conflict` – `ProblemDetails`)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "[US03/AC05] Hợp đồng 'HD-2026-020' đã được gửi trình duyệt trước đó và đang ở trạng thái 'PendingApproval'. Không tạo thêm lượt xử lý phê duyệt trùng lặp."
}
```
- **Response Lỗi khi cố sửa hợp đồng ở `PendingApproval` hoặc chuyển thẳng sang `Approved` (`422 Unprocessable Entity` – `ProblemDetails`)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
  "title": "Business rule violated",
  "status": 422,
  "detail": "[CTR-01] Chỉ được chỉnh sửa hợp đồng khi ở trạng thái 'Draft' hoặc 'RevisionRequested'. Trạng thái hiện tại: 'PendingApproval'."
}
```

---

### 3.3. Phân hệ Bảng giá (`PriceList` & `PriceItem`)

| Method | Endpoint | Chức năng | Status Code |
|---|---|---|---|
| `GET` | `/api/pricelists` | Xem danh sách bảng giá kèm các `PriceItem` | `200 OK` |
| `GET` | `/api/pricelists/{id}` | Xem chi tiết bảng giá | `200 OK`, `404 Not Found` |
| `POST` | `/api/pricelists` | Tạo bảng giá mới (`PRC-02`) | `201 Created`, `400 Bad Request` |
| `GET` | `/api/pricelists/{id}/items` | Xem danh sách `PriceItem` của bảng giá | `200 OK`, `404 Not Found` |
| `POST` | `/api/pricelists/{id}/items` | Thêm `PriceItem` vào bảng giá (`UnitPrice > 0`) | `201 Created`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |
