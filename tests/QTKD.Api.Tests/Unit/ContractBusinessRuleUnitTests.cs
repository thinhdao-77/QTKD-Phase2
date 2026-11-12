using QTKD.Api.DTOs.Contracts;
using QTKD.Api.DTOs.Customers;
using QTKD.Api.Entities;
using QTKD.Api.Exceptions;
using QTKD.Api.Services;
using QTKD.Api.Tests.Unit.Fakes;

namespace QTKD.Api.Tests.Unit;

/// <summary>
/// Unit Test kiểm thử các quy tắc nghiệp vụ ở tầng Service (Bước 14 & Bước 15):
/// - CTR-01: Chỉ cho sửa hợp đồng ở Draft hoặc RevisionRequested (có cả case đúng & case sai).
/// - CTR-02: Chỉ gửi khi Customer, ngày hiệu lực, nội dung/tệp bắt buộc hợp lệ (có cả case đúng & case sai).
/// - CTR-03: Không được chuyển Draft thẳng sang Approved; Submit thành công chỉ chuyển sang PendingApproval (có cả case đúng & case sai).
/// - TC01..TC08 ở mức Unit Test với Fake Repository.
/// </summary>
public sealed class ContractBusinessRuleUnitTests
{
    private static (ContractService ContractService, CustomerService CustomerService, FakeCustomerRepository CustomerRepo, FakeContractRepository ContractRepo) CreateServices()
    {
        var activeCustomer = new Customer
        {
            Id = 1,
            CustomerCode = "KH-001",
            Name = "Công ty TNHH Logistics Minh Phát",
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var suspendedCustomer = new Customer
        {
            Id = 2,
            CustomerCode = "KH-002",
            Name = "Công ty Tạm Ngưng",
            Status = CustomerStatus.Suspended,
            CreatedAt = DateTime.UtcNow
        };

        var draftContractComplete = new Contract
        {
            Id = 1,
            ContractNo = "HD-2026-018",
            CustomerId = 1,
            Customer = activeCustomer,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2027, 9, 30),
            ServiceGroup = "Dịch vụ cảng",
            ContentSummary = "Điều khoản bốc xếp và lưu kho bãi container",
            AttachmentUrl = "https://storage.qtkd.local/contracts/HD-2026-018.pdf",
            Status = ContractStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var draftContractIncomplete = new Contract
        {
            Id = 2,
            ContractNo = "HD-2026-019",
            CustomerId = 1,
            Customer = activeCustomer,
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2027, 10, 31),
            ServiceGroup = "Vận chuyển nội địa",
            ContentSummary = null,
            AttachmentUrl = null,
            Status = ContractStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var pendingContract = new Contract
        {
            Id = 3,
            ContractNo = "HD-2026-020",
            CustomerId = 1,
            Customer = activeCustomer,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2027, 9, 30),
            ServiceGroup = "Dịch vụ cảng",
            ContentSummary = "Nội dung gốc trước khi trình duyệt",
            AttachmentUrl = "https://storage.qtkd.local/contracts/HD-2026-020.pdf",
            Status = ContractStatus.PendingApproval,
            SubmittedAt = new DateTime(2026, 10, 15, 8, 0, 0, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };

        var revisionContract = new Contract
        {
            Id = 4,
            ContractNo = "HD-2026-021",
            CustomerId = 1,
            Customer = activeCustomer,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2027, 9, 30),
            ServiceGroup = "Dịch vụ cảng",
            ContentSummary = "Nội dung cần chỉnh sửa",
            AttachmentUrl = "https://storage.qtkd.local/contracts/HD-2026-021.pdf",
            Status = ContractStatus.RevisionRequested,
            CreatedAt = DateTime.UtcNow
        };

        var customerRepo = new FakeCustomerRepository([activeCustomer, suspendedCustomer]);
        var contractRepo = new FakeContractRepository([draftContractComplete, draftContractIncomplete, pendingContract, revisionContract]);

        var customerService = new CustomerService(customerRepo);
        var contractService = new ContractService(contractRepo, customerRepo);

        return (contractService, customerService, customerRepo, contractRepo);
    }

    // =========================================================================
    // 1. UNIT TESTS CHO CUSTOMER (TC01 & TC02)
    // =========================================================================

    [Fact]
    public async Task TC01_Unit_CreateCustomer_WithValidData_CreatesAndReadsBackCustomer()
    {
        var (_, customerService, customerRepo, _) = CreateServices();

        var request = new CreateCustomerRequest(
            CustomerCode: "KH-003",
            Name: "Công ty TNHH Tiếp Vận Sài Gòn",
            TaxCode: "0315558899",
            Address: "88 Nguyễn Huệ, Quận 1, TP.HCM",
            ContactEmail: "ops@saigonlogistics.vn",
            Status: CustomerStatus.Active
        );

        var created = await customerService.CreateAsync(request);

        Assert.Equal("KH-003", created.CustomerCode);
        Assert.Equal("Công ty TNHH Tiếp Vận Sài Gòn", created.Name);
        Assert.Equal(CustomerStatus.Active, created.Status);

        var fromDb = await customerRepo.GetByIdAsync(created.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("Công ty TNHH Tiếp Vận Sài Gòn", fromDb!.Name);
    }

    [Theory]
    [InlineData("", "Công ty ABC", "CustomerCode")]
    [InlineData("   ", "Công ty ABC", "CustomerCode")]
    [InlineData("KH-004", "", "Name")]
    [InlineData("KH-004", "   ", "Name")]
    public async Task TC02_Unit_CreateCustomer_MissingRequiredFields_ThrowsValidationException(
        string code, string name, string expectedErrorKey)
    {
        var (_, customerService, _, _) = CreateServices();

        var request = new CreateCustomerRequest(
            CustomerCode: code,
            Name: name,
            TaxCode: null,
            Address: null,
            ContactEmail: null,
            Status: CustomerStatus.Active
        );

        var ex = await Assert.ThrowsAsync<ValidationException>(() => customerService.CreateAsync(request));
        Assert.True(ex.Errors.ContainsKey(expectedErrorKey));
    }

    // =========================================================================
    // 2. UNIT TESTS CHO QUY TẮC CTR-01 (TC03 & TC04)
    //    "Chỉ cho sửa hợp đồng ở Draft hoặc RevisionRequested"
    // =========================================================================

    [Fact]
    public async Task CTR01_Valid_TC03_UpdateContract_WhenInDraft_UpdatesSuccessfully()
    {
        var (contractService, _, _, contractRepo) = CreateServices();

        var updateRequest = new UpdateContractRequest(
            CustomerId: 1,
            StartDate: new DateOnly(2026, 11, 1),
            EndDate: new DateOnly(2027, 10, 31),
            ServiceGroup: "Dịch vụ cảng quốc tế",
            ContentSummary: "Nội dung đã cập nhật ở trạng thái Draft",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-2026-019-v2.pdf",
            Status: null
        );

        var updated = await contractService.UpdateAsync(2, updateRequest);

        Assert.Equal(ContractStatus.Draft, updated.Status);
        Assert.Equal("Nội dung đã cập nhật ở trạng thái Draft", updated.ContentSummary);
        Assert.Equal("https://storage.qtkd.local/contracts/HD-2026-019-v2.pdf", updated.AttachmentUrl);

        var stored = await contractRepo.GetByIdAsync(2);
        Assert.Equal("Nội dung đã cập nhật ở trạng thái Draft", stored!.ContentSummary);
    }

    [Fact]
    public async Task CTR01_Valid_UpdateContract_WhenInRevisionRequested_UpdatesSuccessfully()
    {
        var (contractService, _, _, _) = CreateServices();

        var updateRequest = new UpdateContractRequest(
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Nội dung đã hiệu chỉnh theo yêu cầu",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-2026-021-revised.pdf",
            Status: null
        );

        var updated = await contractService.UpdateAsync(4, updateRequest);

        Assert.Equal(ContractStatus.RevisionRequested, updated.Status);
        Assert.Equal("Nội dung đã hiệu chỉnh theo yêu cầu", updated.ContentSummary);
    }

    [Fact]
    public async Task CTR01_Invalid_TC04_UpdateContract_WhenInPendingApproval_ThrowsBusinessRuleExceptionAndPreservesData()
    {
        var (contractService, _, _, contractRepo) = CreateServices();

        var updateRequest = new UpdateContractRequest(
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2028, 9, 30),
            ServiceGroup: "Dịch vụ bị sửa trái phép",
            ContentSummary: "Nội dung bị thay đổi trái phép",
            AttachmentUrl: "https://storage.qtkd.local/contracts/hacked.pdf",
            Status: null
        );

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => contractService.UpdateAsync(3, updateRequest));
        Assert.Contains("CTR-01", ex.Message);

        // Kiểm tra dữ liệu gốc trong repository vẫn được giữ nguyên 100%
        var stored = await contractRepo.GetByIdAsync(3);
        Assert.Equal(ContractStatus.PendingApproval, stored!.Status);
        Assert.Equal("Nội dung gốc trước khi trình duyệt", stored.ContentSummary);
        Assert.Equal("Dịch vụ cảng", stored.ServiceGroup);
    }

    // =========================================================================
    // 3. UNIT TESTS CHO QUY TẮC CTR-02 (TC05 & TC06)
    //    "Chỉ gửi khi Customer, ngày hiệu lực, nội dung/tệp bắt buộc hợp lệ"
    // =========================================================================

    [Fact]
    public async Task CTR02_Invalid_TC05_SubmitContract_WhenMissingContentAndAttachment_ThrowsValidationExceptionAndRemainsDraft()
    {
        var (contractService, _, _, contractRepo) = CreateServices();

        // Contract Id = 2 đang thiếu cả ContentSummary và AttachmentUrl
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            contractService.SubmitAsync(2, new SubmitContractRequest("Gửi khi thiếu dữ liệu")));

        Assert.Contains("CTR-02", ex.Message);
        Assert.True(ex.Errors.ContainsKey("ContentSummary"));
        Assert.True(ex.Errors.ContainsKey("AttachmentUrl"));

        // Kiểm tra trạng thái hợp đồng vẫn là Draft và SubmittedAt vẫn là null
        var stored = await contractRepo.GetByIdAsync(2);
        Assert.Equal(ContractStatus.Draft, stored!.Status);
        Assert.Null(stored.SubmittedAt);
    }

    [Fact]
    public async Task CTR02_Invalid_SubmitContract_WhenCustomerIsNotActive_ThrowsValidationExceptionAndRemainsDraft()
    {
        var (contractService, _, customerRepo, contractRepo) = CreateServices();

        // Chuyển khách hàng Id = 1 sang trạng thái Suspended
        var customer = await customerRepo.GetByIdAsync(1);
        customer!.Status = CustomerStatus.Suspended;

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            contractService.SubmitAsync(1, new SubmitContractRequest("Gửi khi khách hàng bị tạm ngưng")));

        Assert.True(ex.Errors.ContainsKey("Customer"));

        var stored = await contractRepo.GetByIdAsync(1);
        Assert.Equal(ContractStatus.Draft, stored!.Status);
    }

    [Fact]
    public async Task CTR02_Valid_TC06_SubmitContract_WhenAllConditionsMet_TransitionsToPendingApproval()
    {
        var (contractService, _, _, contractRepo) = CreateServices();

        var result = await contractService.SubmitAsync(1, new SubmitContractRequest("Hồ sơ hợp lệ, kính trình duyệt"));

        Assert.Equal(1, result.ContractId);
        Assert.Equal("HD-2026-018", result.ContractNo);
        Assert.Equal(ContractStatus.Draft, result.PreviousStatus);
        Assert.Equal(ContractStatus.PendingApproval, result.Status);
        Assert.Equal("APR-HD-2026-018", result.ApprovalId);

        var stored = await contractRepo.GetByIdAsync(1);
        Assert.Equal(ContractStatus.PendingApproval, stored!.Status);
        Assert.NotNull(stored.SubmittedAt);
    }

    // =========================================================================
    // 4. UNIT TESTS CHO QUY TẮC CTR-03 (TC06 & TC07)
    //    "Không được chuyển Draft thẳng sang Approved"
    // =========================================================================

    [Fact]
    public async Task CTR03_Invalid_TC07_UpdateContract_TryTransitionDraftDirectlyToApproved_ThrowsBusinessRuleException()
    {
        var (contractService, _, _, contractRepo) = CreateServices();

        var updateRequest = new UpdateContractRequest(
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Đầy đủ nội dung",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-2026-018.pdf",
            Status: ContractStatus.Approved
        );

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => contractService.UpdateAsync(1, updateRequest));
        Assert.Contains("CTR-03", ex.Message);

        var stored = await contractRepo.GetByIdAsync(1);
        Assert.Equal(ContractStatus.Draft, stored!.Status);
    }

    [Fact]
    public async Task CTR03_Invalid_CreateContract_WithApprovedStatus_ThrowsBusinessRuleException()
    {
        var (contractService, _, _, _) = CreateServices();

        var createRequest = new CreateContractRequest(
            ContractNo: "HD-2026-099",
            CustomerId: 1,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2027, 9, 30),
            ServiceGroup: "Dịch vụ cảng",
            ContentSummary: "Nội dung",
            AttachmentUrl: "https://storage.qtkd.local/contracts/HD-2026-099.pdf",
            Status: ContractStatus.Approved
        );

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => contractService.CreateAsync(createRequest));
        Assert.Contains("CTR-03", ex.Message);
    }

    [Fact]
    public async Task CTR03_Valid_SubmitContract_OnlyTransitionsToPendingApproval_NeverApproved()
    {
        var (contractService, _, _, contractRepo) = CreateServices();

        var response = await contractService.SubmitAsync(1);

        Assert.Equal(ContractStatus.PendingApproval, response.Status);
        Assert.NotEqual(ContractStatus.Approved, response.Status);

        var stored = await contractRepo.GetByIdAsync(1);
        Assert.Equal(ContractStatus.PendingApproval, stored!.Status);
    }

    // =========================================================================
    // 5. UNIT TEST CHO CHỐNG SUBMIT LẶP (TC08 / AC05)
    // =========================================================================

    [Fact]
    public async Task TC08_Unit_SubmitContractTwice_SecondCallThrowsConflictAndDoesNotDuplicateProcessing()
    {
        var (contractService, _, _, contractRepo) = CreateServices();

        var firstSubmit = await contractService.SubmitAsync(1, new SubmitContractRequest("Gửi lần 1"));
        var originalSubmittedAt = firstSubmit.SubmittedAt;

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            contractService.SubmitAsync(1, new SubmitContractRequest("Gửi lặp lần 2")));

        Assert.Contains("AC05", ex.Message);

        var stored = await contractRepo.GetByIdAsync(1);
        Assert.Equal(ContractStatus.PendingApproval, stored!.Status);
        Assert.Equal(originalSubmittedAt, stored.SubmittedAt);
    }
}
