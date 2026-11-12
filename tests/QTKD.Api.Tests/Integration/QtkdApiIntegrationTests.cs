using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using QTKD.Api.DTOs.Contracts;
using QTKD.Api.DTOs.Customers;
using QTKD.Api.DTOs.PriceLists;
using QTKD.Api.Entities;

namespace QTKD.Api.Tests.Integration;

/// <summary>
/// Integration Test cho các API chính trên cơ sở dữ liệu kiểm thử riêng 'qtkd_test_db' (Bước 16 & Bước 17):
/// - Bao phủ toàn bộ danh sách test tối thiểu TC01 – TC08 (Mục 5.3).
/// - Kiểm tra đầy đủ cả 3 yếu tố ở mỗi test (Bước 17):
///   (1) HTTP Status Code
///   (2) Nội dung JSON trả về từ API
///   (3) Dữ liệu thực tế lưu trong PostgreSQL sau khi gọi API.
/// </summary>
public sealed class QtkdApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QtkdApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // =========================================================================
    // TC01: Tạo Customer hợp lệ -> Tạo được và đọc lại từ DB
    // =========================================================================
    [Fact]
    public async Task TC01_Integration_CreateValidCustomer_Returns201AndPersistsInDatabase()
    {
        var request = new CreateCustomerRequest(
            CustomerCode: "KH-TC01",
            Name: "Công ty TNHH Tiếp Vận TC01",
            TaxCode: "0319991111",
            Address: "100 Lê Lợi, Quận 1, TP.HCM",
            ContactEmail: "tc01@logistics.vn",
            Status: CustomerStatus.Active
        );

        // 1. Kiểm tra HTTP status 201 Created
        var response = await _client.PostAsJsonAsync("/api/customers", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // 2. Kiểm tra nội dung trả về
        var created = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(created);
        Assert.True(created!.Id > 0);
        Assert.Equal("KH-TC01", created.CustomerCode);
        Assert.Equal("Công ty TNHH Tiếp Vận TC01", created.Name);
        Assert.Equal(CustomerStatus.Active, created.Status);

        // 3. Đọc lại từ API GET /api/customers/{id} và đối chiếu trực tiếp trong DB
        var getResponse = await _client.GetAsync($"/api/customers/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal("Công ty TNHH Tiếp Vận TC01", fetched.Name);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbEntity = await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == created.Id);
        Assert.NotNull(dbEntity);
        Assert.Equal("KH-TC01", dbEntity!.CustomerCode);
        Assert.Equal("0319991111", dbEntity.TaxCode);
    }

    // =========================================================================
    // TC02: Tạo Customer thiếu thông tin bắt buộc -> Bị từ chối; có thông báo lỗi
    // =========================================================================
    [Fact]
    public async Task TC02_Integration_CreateCustomerMissingRequiredFields_Returns400WithValidationErrorsAndDoesNotSave()
    {
        var invalidRequest = new
        {
            customerCode = "",
            name = "",
            taxCode = "0310000000"
        };

        // 1. Kiểm tra HTTP status 400 Bad Request
        var response = await _client.PostAsJsonAsync("/api/customers", invalidRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // 2. Kiểm tra nội dung thông báo lỗi ValidationProblemDetails
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.True(problem.Errors.ContainsKey("CustomerCode"));
        Assert.True(problem.Errors.ContainsKey("Name"));

        // 3. Kiểm tra DB không có bản ghi rác nào được tạo với taxCode = "0310000000"
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var exists = await db.Customers.AnyAsync(c => c.TaxCode == "0310000000");
        Assert.False(exists);
    }

    // =========================================================================
    // TC03: Sửa Contract ở Draft -> Thành công khi dữ liệu hợp lệ
    // =========================================================================
    [Fact]
    public async Task TC03_Integration_UpdateContractInDraft_WithValidData_Returns200AndUpdatesDatabase()
    {
        // Tạo một Contract mới ở trạng thái Draft
        var createReq = new CreateContractRequest(
            ContractNo: "HD-TC03-2026",
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Nội dung ban đầu",
            AttachmentUrl: null,
            Status: null
        );
        var createResp = await _client.PostAsJsonAsync("/api/contracts", createReq);
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = (await createResp.Content.ReadFromJsonAsync<ContractResponse>())!;

        // Cập nhật Contract khi đang ở Draft
        var updateReq = new UpdateContractRequest(
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 15),
            EndDate: new DateOnly(2027, 10, 14),
            ServiceGroup: "Dịch vụ khai thác cảng & lưu bãi",
            ContentSummary: "Nội dung đã bổ sung đầy đủ theo TC03",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-TC03-2026.pdf",
            Status: null
        );

        // 1. Kiểm tra HTTP status 200 OK
        var updateResp = await _client.PutAsJsonAsync($"/api/contracts/{created.Id}", updateReq);
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

        // 2. Kiểm tra nội dung trả về
        var updated = await updateResp.Content.ReadFromJsonAsync<ContractResponse>();
        Assert.NotNull(updated);
        Assert.Equal(ContractStatus.Draft, updated!.Status);
        Assert.Equal("Nội dung đã bổ sung đầy đủ theo TC03", updated.ContentSummary);
        Assert.Equal("https://storage.qtkd.local/contracts/HD-TC03-2026.pdf", updated.AttachmentUrl);

        // 3. Kiểm tra dữ liệu thực tế trong PostgreSQL
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbContract = await db.Contracts.AsNoTracking().FirstAsync(c => c.Id == created.Id);
        Assert.Equal("Nội dung đã bổ sung đầy đủ theo TC03", dbContract.ContentSummary);
        Assert.Equal("https://storage.qtkd.local/contracts/HD-TC03-2026.pdf", dbContract.AttachmentUrl);
        Assert.Equal(ContractStatus.Draft, dbContract.Status);
    }

    // =========================================================================
    // TC04: Sửa Contract ở PendingApproval -> Bị từ chối (CTR-01); dữ liệu giữ nguyên
    // =========================================================================
    [Fact]
    public async Task TC04_Integration_UpdateContractInPendingApproval_Returns422AndPreservesDatabaseState()
    {
        // Bước chuẩn bị: Tạo Contract hợp lệ và Submit sang PendingApproval
        var createReq = new CreateContractRequest(
            ContractNo: "HD-TC04-2026",
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Nội dung chuẩn đã trình duyệt",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-TC04-2026.pdf",
            Status: null
        );
        var created = (await (await _client.PostAsJsonAsync("/api/contracts", createReq))
            .Content.ReadFromJsonAsync<ContractResponse>())!;

        var submitResp = await _client.PostAsJsonAsync($"/api/contracts/{created.Id}/submit", new SubmitContractRequest("Trình duyệt TC04"));
        Assert.Equal(HttpStatusCode.OK, submitResp.StatusCode);

        // Thử sửa Contract khi đã ở PendingApproval
        var illegalUpdateReq = new UpdateContractRequest(
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2029, 9, 30),
            ServiceGroup: "Dịch vụ bị sửa trái phép",
            ContentSummary: "Nội dung bị sửa trái phép",
            AttachmentUrl: "https://storage.qtkd.local/contracts/hacked.pdf",
            Status: null
        );

        // 1. Kiểm tra HTTP status 422 Unprocessable Entity
        var updateResp = await _client.PutAsJsonAsync($"/api/contracts/{created.Id}", illegalUpdateReq);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, updateResp.StatusCode);

        // 2. Kiểm tra nội dung thông báo lỗi có chứa mã rule CTR-01
        var problem = await updateResp.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(422, problem!.Status);
        Assert.Contains("CTR-01", problem.Detail);

        // 3. Kiểm tra dữ liệu trong PostgreSQL vẫn giữ nguyên giá trị trước khi gọi PUT
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbContract = await db.Contracts.AsNoTracking().FirstAsync(c => c.Id == created.Id);
        Assert.Equal(ContractStatus.PendingApproval, dbContract.Status);
        Assert.Equal("Nội dung chuẩn đã trình duyệt", dbContract.ContentSummary);
        Assert.Equal("Dịch vụ cảng", dbContract.ServiceGroup);
    }

    // =========================================================================
    // TC05: Submit Contract thiếu điều kiện CTR-02 -> Bị từ chối; vẫn là Draft
    // =========================================================================
    [Fact]
    public async Task TC05_Integration_SubmitContractMissingCtr02Conditions_Returns400AndRemainsDraftInDatabase()
    {
        // Tạo Contract ở trạng thái Draft nhưng thiếu ContentSummary và AttachmentUrl
        var createReq = new CreateContractRequest(
            ContractNo: "HD-TC05-2026",
            CustomerId: 1,
            StartDate: new DateOnly(2026, 11, 1),
            EndDate: new DateOnly(2027, 10, 31),
            ServiceGroup: "Dịch vụ vận chuyển",
            ContentSummary: null,
            AttachmentUrl: null,
            Status: null
        );
        var created = (await (await _client.PostAsJsonAsync("/api/contracts", createReq))
            .Content.ReadFromJsonAsync<ContractResponse>())!;

        // 1. Kiểm tra HTTP status 400 Bad Request khi Submit thiếu điều kiện CTR-02
        var submitResp = await _client.PostAsJsonAsync($"/api/contracts/{created.Id}/submit", new SubmitContractRequest("Gửi thử khi thiếu hồ sơ"));
        Assert.Equal(HttpStatusCode.BadRequest, submitResp.StatusCode);

        // 2. Kiểm tra nội dung trả về liệt kê đủ các lỗi CTR-02
        var problem = await submitResp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        Assert.True(problem.Errors.ContainsKey("ContentSummary"));
        Assert.True(problem.Errors.ContainsKey("AttachmentUrl"));

        // 3. Kiểm tra dữ liệu trong PostgreSQL vẫn giữ nguyên trạng thái Draft và SubmittedAt == null
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbContract = await db.Contracts.AsNoTracking().FirstAsync(c => c.Id == created.Id);
        Assert.Equal(ContractStatus.Draft, dbContract.Status);
        Assert.Null(dbContract.SubmittedAt);
    }

    // =========================================================================
    // TC06: Submit Contract hợp lệ -> Chuyển sang PendingApproval (kể cả khi không gửi body JSON)
    // =========================================================================
    [Fact]
    public async Task TC06_Integration_SubmitValidContract_WithoutRequestBody_Returns200AndTransitionsToPendingApproval()
    {
        var createReq = new CreateContractRequest(
            ContractNo: "HD-TC06-2026",
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Cung cấp dịch vụ bốc xếp container trọn gói",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-TC06-2026.pdf",
            Status: null
        );
        var created = (await (await _client.PostAsJsonAsync("/api/contracts", createReq))
            .Content.ReadFromJsonAsync<ContractResponse>())!;

        // Gửi POST /api/contracts/{id}/submit không kèm body (vì comment là tùy chọn theo đặc tả US03)
        var submitResp = await _client.PostAsync($"/api/contracts/{created.Id}/submit", content: null);

        // 1. Kiểm tra HTTP status 200 OK
        Assert.Equal(HttpStatusCode.OK, submitResp.StatusCode);

        // 2. Kiểm tra nội dung phản hồi SubmitContractResponse
        var submitResult = await submitResp.Content.ReadFromJsonAsync<SubmitContractResponse>();
        Assert.NotNull(submitResult);
        Assert.Equal(created.Id, submitResult!.ContractId);
        Assert.Equal(ContractStatus.Draft, submitResult.PreviousStatus);
        Assert.Equal(ContractStatus.PendingApproval, submitResult.Status);
        Assert.Equal("APR-HD-TC06-2026", submitResult.ApprovalId);

        // 3. Đọc lại qua API GET và kiểm tra trực tiếp trong PostgreSQL
        var getResp = await _client.GetFromJsonAsync<ContractResponse>($"/api/contracts/{created.Id}");
        Assert.NotNull(getResp);
        Assert.Equal(ContractStatus.PendingApproval, getResp!.Status);
        Assert.NotNull(getResp.SubmittedAt);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbContract = await db.Contracts.AsNoTracking().FirstAsync(c => c.Id == created.Id);
        Assert.Equal(ContractStatus.PendingApproval, dbContract.Status);
        Assert.NotNull(dbContract.SubmittedAt);
    }

    // =========================================================================
    // TC07: Thử chuyển Draft thẳng sang Approved -> Bị từ chối (CTR-03)
    // =========================================================================
    [Fact]
    public async Task TC07_Integration_TryTransitionDraftDirectlyToApproved_Returns422AndRemainsDraft()
    {
        var createReq = new CreateContractRequest(
            ContractNo: "HD-TC07-2026",
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Hợp đồng thử nghiệm TC07",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-TC07-2026.pdf",
            Status: null
        );
        var created = (await (await _client.PostAsJsonAsync("/api/contracts", createReq))
            .Content.ReadFromJsonAsync<ContractResponse>())!;

        // Thử dùng PUT để chuyển thẳng trạng thái từ Draft sang Approved
        var updateToApprovedReq = new UpdateContractRequest(
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Hợp đồng thử nghiệm TC07",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-TC07-2026.pdf",
            Status: ContractStatus.Approved
        );

        // 1. Kiểm tra HTTP status 422 Unprocessable Entity
        var response = await _client.PutAsJsonAsync($"/api/contracts/{created.Id}", updateToApprovedReq);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        // 2. Kiểm tra nội dung thông báo lỗi chứa CTR-03
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(422, problem!.Status);
        Assert.Contains("CTR-03", problem.Detail);

        // 3. Kiểm tra dữ liệu trong PostgreSQL vẫn là Draft
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbContract = await db.Contracts.AsNoTracking().FirstAsync(c => c.Id == created.Id);
        Assert.Equal(ContractStatus.Draft, dbContract.Status);
    }

    // =========================================================================
    // TC08: Submit lặp một Contract -> Không tạo thêm kết quả xử lý trùng (409 Conflict)
    // =========================================================================
    [Fact]
    public async Task TC08_Integration_SubmitContractTwice_SecondCallReturns409AndDoesNotOverwriteSubmission()
    {
        var createReq = new CreateContractRequest(
            ContractNo: "HD-TC08-2026",
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Hợp đồng thử nghiệm gửi lặp TC08",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-TC08-2026.pdf",
            Status: null
        );
        var created = (await (await _client.PostAsJsonAsync("/api/contracts", createReq))
            .Content.ReadFromJsonAsync<ContractResponse>())!;

        // Lần Submit thứ nhất -> 200 OK
        var firstResp = await _client.PostAsJsonAsync($"/api/contracts/{created.Id}/submit", new SubmitContractRequest("Gửi lần 1"));
        Assert.Equal(HttpStatusCode.OK, firstResp.StatusCode);
        var firstResult = (await firstResp.Content.ReadFromJsonAsync<SubmitContractResponse>())!;

        // Lần Submit thứ hai (lặp) -> 409 Conflict
        var secondResp = await _client.PostAsJsonAsync($"/api/contracts/{created.Id}/submit", new SubmitContractRequest("Gửi lặp lần 2"));
        Assert.Equal(HttpStatusCode.Conflict, secondResp.StatusCode);

        var problem = await secondResp.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem!.Status);
        Assert.Contains("AC05", problem.Detail);

        // Kiểm tra dữ liệu trong PostgreSQL giữ nguyên thời điểm SubmittedAt của lần gửi đầu tiên
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var dbContract = await db.Contracts.AsNoTracking().FirstAsync(c => c.Id == created.Id);
        Assert.Equal(ContractStatus.PendingApproval, dbContract.Status);
        Assert.NotNull(dbContract.SubmittedAt);
        // Sai số mili-giây do PostgreSQL lưu microsecond
        Assert.True(Math.Abs((dbContract.SubmittedAt!.Value - firstResult.SubmittedAt).TotalMilliseconds) < 5);
    }

    // =========================================================================
    // Kịch bản End-to-End xuyên suốt (Bước 13 & Bước 18):
    // Tạo Customer Minh Phát -> Tạo Contract Draft thiếu dữ liệu -> Submit thất bại
    // -> Bổ sung dữ liệu -> Submit thành công -> Đọc lại trạng thái & Quản lý Bảng giá
    // =========================================================================
    [Fact]
    public async Task EndToEnd_FullWorkflow_CreateCustomer_CreateDraft_SubmitFail_Update_SubmitSuccess_And_Pricing()
    {
        // 1. Tạo Customer mới
        var custResp = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest(
            CustomerCode: "KH-E2E-01",
            Name: "Công ty Cổ phần Logistics Minh Phát Mở Rộng",
            TaxCode: "0318889999",
            Address: "KCN Hiệp Phước, Nhà Bè, TP.HCM",
            ContactEmail: "e2e@minhphat.vn",
            Status: CustomerStatus.Active
        ));
        Assert.Equal(HttpStatusCode.Created, custResp.StatusCode);
        var customer = (await custResp.Content.ReadFromJsonAsync<CustomerResponse>())!;

        // 2. Tạo Contract Draft thiếu dữ liệu bắt buộc
        var draftResp = await _client.PostAsJsonAsync("/api/contracts", new CreateContractRequest(
            ContractNo: "HD-2026-E2E",
            CustomerId: customer.Id,
            StartDate: new DateOnly(2026, 11, 1),
            EndDate: new DateOnly(2027, 10, 31),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: null,
            AttachmentUrl: null,
            Status: null
        ));
        Assert.Equal(HttpStatusCode.Created, draftResp.StatusCode);
        var contract = (await draftResp.Content.ReadFromJsonAsync<ContractResponse>())!;
        Assert.Equal(ContractStatus.Draft, contract.Status);

        // 3. Submit khi thiếu dữ liệu -> Thất bại 400 Bad Request
        var failSubmitResp = await _client.PostAsJsonAsync($"/api/contracts/{contract.Id}/submit", new SubmitContractRequest("Thử gửi khi chưa đủ hồ sơ"));
        Assert.Equal(HttpStatusCode.BadRequest, failSubmitResp.StatusCode);

        // 4. Bổ sung dữ liệu hợp lệ -> 200 OK
        var updateResp = await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", new UpdateContractRequest(
            CustomerId: customer.Id,
            StartDate: new DateOnly(2026, 11, 1),
            EndDate: new DateOnly(2027, 10, 31),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Điều khoản dịch vụ bốc xếp và lưu kho bãi container năm 2026-2027",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-2026-E2E.pdf",
            Status: null
        ));
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

        // 5. Submit lại khi đã đủ điều kiện -> 200 OK & PendingApproval
        var okSubmitResp = await _client.PostAsJsonAsync($"/api/contracts/{contract.Id}/submit", new SubmitContractRequest("Hồ sơ hoàn chỉnh"));
        Assert.Equal(HttpStatusCode.OK, okSubmitResp.StatusCode);
        var submitData = (await okSubmitResp.Content.ReadFromJsonAsync<SubmitContractResponse>())!;
        Assert.Equal(ContractStatus.PendingApproval, submitData.Status);

        // 6. Tạo PriceList và thêm PriceItem
        var plResp = await _client.PostAsJsonAsync("/api/pricelists", new CreatePriceListRequest(
            Name: "Bảng giá E2E 2026",
            EffectiveDate: new DateOnly(2026, 11, 1),
            ExpiryDate: new DateOnly(2027, 10, 31),
            Status: PriceListStatus.Effective
        ));
        Assert.Equal(HttpStatusCode.Created, plResp.StatusCode);
        var priceList = (await plResp.Content.ReadFromJsonAsync<PriceListResponse>())!;

        var itemResp = await _client.PostAsJsonAsync($"/api/pricelists/{priceList.Id}/items", new CreatePriceItemRequest(
            ServiceCode: "SVC-E2E-40",
            ServiceName: "Bốc xếp container 40 feet",
            Unit: "Container",
            UnitPrice: 2100000m
        ));
        Assert.Equal(HttpStatusCode.Created, itemResp.StatusCode);
    }
}
