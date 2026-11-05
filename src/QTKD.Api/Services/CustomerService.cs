using QTKD.Api.DTOs.Customers;
using QTKD.Api.Entities;
using QTKD.Api.Exceptions;
using QTKD.Api.Mappings;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public sealed class CustomerService : ICustomerService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.Ordinal)
    {
        CustomerStatus.Active,
        CustomerStatus.Suspended,
        CustomerStatus.Inactive
    };

    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<CustomerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(c => c.ToResponse()).ToList();
    }

    public async Task<CustomerResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        return customer.ToResponse();
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.CustomerCode?.Trim() ?? string.Empty;
        var name = request.Name?.Trim() ?? string.Empty;
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? CustomerStatus.Active
            : request.Status.Trim();

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(code))
        {
            errors["CustomerCode"] = ["Mã khách hàng (CustomerCode) là trường bắt buộc."];
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            errors["Name"] = ["Tên khách hàng (Name) là trường bắt buộc."];
        }
        if (!AllowedStatuses.Contains(status))
        {
            errors["Status"] = [$"Trạng thái khách hàng không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedStatuses)}."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors, "Dữ liệu tạo khách hàng không hợp lệ.");
        }

        if (await _customerRepository.ExistsByCodeAsync(code, cancellationToken))
        {
            throw new ConflictException($"Mã khách hàng '{code}' đã tồn tại trong hệ thống.");
        }

        var customer = new Customer
        {
            CustomerCode = code,
            Name = name,
            TaxCode = string.IsNullOrWhiteSpace(request.TaxCode) ? null : request.TaxCode.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim(),
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerRepository.SaveChangesAsync(cancellationToken);

        return customer.ToResponse();
    }

    public async Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Customer", id);

        var name = request.Name?.Trim() ?? string.Empty;
        var status = request.Status?.Trim() ?? string.Empty;

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(name))
        {
            errors["Name"] = ["Tên khách hàng (Name) là trường bắt buộc."];
        }
        if (!AllowedStatuses.Contains(status))
        {
            errors["Status"] = [$"Trạng thái khách hàng không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedStatuses)}."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors, "Dữ liệu cập nhật khách hàng không hợp lệ.");
        }

        customer.Name = name;
        customer.TaxCode = string.IsNullOrWhiteSpace(request.TaxCode) ? null : request.TaxCode.Trim();
        customer.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        customer.ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim();
        customer.Status = status;

        await _customerRepository.SaveChangesAsync(cancellationToken);

        return customer.ToResponse();
    }
}
