using QTKD.Api.DTOs.Contracts;
using QTKD.Api.Entities;
using QTKD.Api.Exceptions;
using QTKD.Api.Mappings;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public sealed class ContractService : IContractService
{
    private readonly IContractRepository _contractRepository;
    private readonly ICustomerRepository _customerRepository;

    public ContractService(IContractRepository contractRepository, ICustomerRepository customerRepository)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;
    }

    public async Task<List<ContractResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var contracts = await _contractRepository.GetAllAsync(cancellationToken);
        return contracts.Select(c => c.ToResponse()).ToList();
    }

    public async Task<ContractResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contract", id);

        return contract.ToResponse();
    }

    public async Task<ContractResponse> CreateAsync(CreateContractRequest request, CancellationToken cancellationToken = default)
    {
        var contractNo = request.ContractNo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(contractNo))
        {
            throw new ValidationException("ContractNo", "Số hợp đồng (ContractNo) là trường bắt buộc.");
        }

        // CTR-03: Không cho phép khởi tạo trực tiếp hợp đồng ở trạng thái Approved hoặc trạng thái khác ngoài Draft
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            !string.Equals(request.Status.Trim(), ContractStatus.Draft, StringComparison.Ordinal))
        {
            throw new BusinessRuleException(
                $"[CTR-03] Không được tạo trực tiếp hợp đồng ở trạng thái '{request.Status.Trim()}'. Hợp đồng mới tạo phải ở trạng thái '{ContractStatus.Draft}'.");
        }

        if (request.StartDate > request.EndDate)
        {
            throw new ValidationException("EndDate", "Thời gian hiệu lực không hợp lệ: StartDate không được lớn hơn EndDate.");
        }

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new ValidationException("CustomerId", $"Khách hàng với Id '{request.CustomerId}' không tồn tại.");

        if (customer.Status != CustomerStatus.Active)
        {
            throw new BusinessRuleException(
                $"Khách hàng '{customer.Name}' đang ở trạng thái '{customer.Status}', chỉ khách hàng 'Active' mới được lập hợp đồng.");
        }

        if (await _contractRepository.ExistsByContractNoAsync(contractNo, cancellationToken))
        {
            throw new ConflictException($"Số hợp đồng '{contractNo}' đã tồn tại trong hệ thống.");
        }

        var contract = new Contract
        {
            ContractNo = contractNo,
            CustomerId = customer.Id,
            Customer = customer,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            ServiceGroup = string.IsNullOrWhiteSpace(request.ServiceGroup) ? null : request.ServiceGroup.Trim(),
            ContentSummary = string.IsNullOrWhiteSpace(request.ContentSummary) ? null : request.ContentSummary.Trim(),
            AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim(),
            Status = ContractStatus.Draft,
            SubmittedAt = null,
            CreatedAt = DateTime.UtcNow
        };

        await _contractRepository.AddAsync(contract, cancellationToken);
        await _contractRepository.SaveChangesAsync(cancellationToken);

        return contract.ToResponse();
    }

    public async Task<ContractResponse> UpdateAsync(int id, UpdateContractRequest request, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contract", id);

        // CTR-01: Chỉ được chỉnh sửa hợp đồng ở trạng thái Draft hoặc RevisionRequested
        if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.RevisionRequested)
        {
            throw new BusinessRuleException(
                $"[CTR-01] Chỉ được chỉnh sửa hợp đồng khi ở trạng thái '{ContractStatus.Draft}' hoặc '{ContractStatus.RevisionRequested}'. Trạng thái hiện tại: '{contract.Status}'.");
        }

        // CTR-03: Không được chuyển trực tiếp từ Draft sang Approved (hoặc tự ý đổi trạng thái qua API sửa thông tin)
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var requestedStatus = request.Status.Trim();
            if (string.Equals(requestedStatus, ContractStatus.Approved, StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRuleException(
                    $"[CTR-03] Không được chuyển trực tiếp hợp đồng từ '{contract.Status}' sang '{ContractStatus.Approved}'. Hợp đồng phải qua quy trình trình duyệt (POST /api/contracts/{{id}}/submit).");
            }

            if (!string.Equals(requestedStatus, contract.Status, StringComparison.Ordinal))
            {
                throw new BusinessRuleException(
                    $"[CTR-03] Không được phép thay đổi trạng thái từ '{contract.Status}' sang '{requestedStatus}' qua thao tác cập nhật thông tin.");
            }
        }

        if (request.StartDate > request.EndDate)
        {
            throw new ValidationException("EndDate", "Thời gian hiệu lực không hợp lệ: StartDate không được lớn hơn EndDate.");
        }

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new ValidationException("CustomerId", $"Khách hàng với Id '{request.CustomerId}' không tồn tại.");

        if (customer.Status != CustomerStatus.Active)
        {
            throw new BusinessRuleException(
                $"Khách hàng '{customer.Name}' đang ở trạng thái '{customer.Status}', không hợp lệ để gán vào hợp đồng.");
        }

        contract.CustomerId = customer.Id;
        contract.Customer = customer;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.ServiceGroup = string.IsNullOrWhiteSpace(request.ServiceGroup) ? null : request.ServiceGroup.Trim();
        contract.ContentSummary = string.IsNullOrWhiteSpace(request.ContentSummary) ? null : request.ContentSummary.Trim();
        contract.AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim();

        await _contractRepository.SaveChangesAsync(cancellationToken);

        return contract.ToResponse();
    }

    public async Task<SubmitContractResponse> SubmitAsync(int id, SubmitContractRequest? request = null, CancellationToken cancellationToken = default)
    {
        var contract = await _contractRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Contract", id);

        // AC05 / TC08: Kiểm tra gửi lặp cùng một hợp đồng đã ở trạng thái PendingApproval
        if (contract.Status == ContractStatus.PendingApproval)
        {
            throw new ConflictException(
                $"[US03/AC05] Hợp đồng '{contract.ContractNo}' đã được gửi trình duyệt trước đó và đang ở trạng thái '{ContractStatus.PendingApproval}'. Không tạo thêm lượt xử lý phê duyệt trùng lặp.");
        }

        // AC02: Kiểm tra hợp đồng có ở trạng thái cho phép gửi (Draft hoặc RevisionRequested) hay không
        if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.RevisionRequested)
        {
            throw new ConflictException(
                $"[US03/AC02] Hợp đồng đang ở trạng thái '{contract.Status}', không được phép gửi trình duyệt. Chỉ hợp đồng ở trạng thái '{ContractStatus.Draft}' hoặc '{ContractStatus.RevisionRequested}' mới được gửi.");
        }

        // CTR-02 / AC03: Kiểm tra đầy đủ điều kiện gửi hợp đồng:
        // 1. Customer hợp lệ (tồn tại và đang Active)
        // 2. Thời gian hiệu lực hợp lệ (StartDate <= EndDate)
        // 3. Nội dung hợp đồng bắt buộc (ContentSummary không rỗng)
        // 4. Tệp đính kèm bắt buộc (AttachmentUrl không rỗng)
        var ctr02Errors = new Dictionary<string, string[]>();

        var customer = contract.Customer ?? await _customerRepository.GetByIdAsync(contract.CustomerId, cancellationToken);
        if (customer is null || customer.Status != CustomerStatus.Active)
        {
            ctr02Errors["Customer"] = ["[CTR-02] Khách hàng của hợp đồng không tồn tại hoặc không ở trạng thái 'Active'."];
        }

        if (contract.StartDate > contract.EndDate)
        {
            ctr02Errors["EffectiveDates"] = ["[CTR-02] Thời gian hiệu lực hợp đồng không hợp lệ (StartDate lớn hơn EndDate)."];
        }

        if (string.IsNullOrWhiteSpace(contract.ContentSummary))
        {
            ctr02Errors["ContentSummary"] = ["[CTR-02] Thiếu nội dung hợp đồng bắt buộc (ContentSummary)."];
        }

        if (string.IsNullOrWhiteSpace(contract.AttachmentUrl))
        {
            ctr02Errors["AttachmentUrl"] = ["[CTR-02] Thiếu tệp đính kèm bắt buộc (AttachmentUrl)."];
        }

        if (ctr02Errors.Count > 0)
        {
            throw new ValidationException(
                ctr02Errors,
                "[CTR-02] Hợp đồng chưa đáp ứng đủ điều kiện bắt buộc để gửi trình duyệt.");
        }

        // CTR-03 / AC04: Chỉ chuyển từ Draft/RevisionRequested sang PendingApproval (không bao giờ chuyển thẳng sang Approved)
        var previousStatus = contract.Status;
        contract.Status = ContractStatus.PendingApproval;
        contract.SubmittedAt = DateTime.UtcNow;

        await _contractRepository.SaveChangesAsync(cancellationToken);

        var approvalId = $"APR-{contract.ContractNo}";
        var comment = string.IsNullOrWhiteSpace(request?.Comment) ? null : request.Comment.Trim();

        return new SubmitContractResponse(
            ContractId: contract.Id,
            ContractNo: contract.ContractNo,
            PreviousStatus: previousStatus,
            Status: contract.Status,
            ApprovalId: approvalId,
            SubmittedAt: contract.SubmittedAt.Value,
            Comment: comment,
            Message: $"Hợp đồng '{contract.ContractNo}' đã được gửi trình duyệt thành công và chuyển sang trạng thái '{ContractStatus.PendingApproval}'."
        );
    }
}
