using QTKD.Api.DTOs.PriceLists;
using QTKD.Api.Entities;
using QTKD.Api.Exceptions;
using QTKD.Api.Mappings;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public sealed class PriceListService : IPriceListService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.Ordinal)
    {
        PriceListStatus.Draft,
        PriceListStatus.Effective,
        PriceListStatus.Expired
    };

    private readonly IPriceListRepository _priceListRepository;

    public PriceListService(IPriceListRepository priceListRepository)
    {
        _priceListRepository = priceListRepository;
    }

    public async Task<List<PriceListResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var priceLists = await _priceListRepository.GetAllAsync(cancellationToken);
        return priceLists.Select(p => p.ToResponse()).ToList();
    }

    public async Task<PriceListResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var priceList = await _priceListRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("PriceList", id);

        return priceList.ToResponse();
    }

    public async Task<PriceListResponse> CreateAsync(CreatePriceListRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? PriceListStatus.Draft
            : request.Status.Trim();

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(name))
        {
            errors["Name"] = ["Tên bảng giá (Name) là trường bắt buộc."];
        }
        if (request.ExpiryDate.HasValue && request.EffectiveDate > request.ExpiryDate.Value)
        {
            errors["ExpiryDate"] = ["[PRC-02] Ngày bắt đầu hiệu lực (EffectiveDate) không được lớn hơn ngày hết hiệu lực (ExpiryDate)."];
        }
        if (!AllowedStatuses.Contains(status))
        {
            errors["Status"] = [$"Trạng thái bảng giá không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedStatuses)}."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors, "Dữ liệu tạo bảng giá không hợp lệ.");
        }

        var priceList = new PriceList
        {
            Name = name,
            EffectiveDate = request.EffectiveDate,
            ExpiryDate = request.ExpiryDate,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await _priceListRepository.AddPriceListAsync(priceList, cancellationToken);
        await _priceListRepository.SaveChangesAsync(cancellationToken);

        return priceList.ToResponse();
    }

    public async Task<List<PriceItemResponse>> GetItemsAsync(int priceListId, CancellationToken cancellationToken = default)
    {
        _ = await _priceListRepository.GetByIdAsync(priceListId, cancellationToken)
            ?? throw new NotFoundException("PriceList", priceListId);

        var items = await _priceListRepository.GetItemsByPriceListIdAsync(priceListId, cancellationToken);
        return items.Select(i => i.ToResponse()).ToList();
    }

    public async Task<PriceItemResponse> AddItemAsync(int priceListId, CreatePriceItemRequest request, CancellationToken cancellationToken = default)
    {
        _ = await _priceListRepository.GetByIdAsync(priceListId, cancellationToken)
            ?? throw new NotFoundException("PriceList", priceListId);

        var serviceCode = request.ServiceCode?.Trim() ?? string.Empty;
        var serviceName = request.ServiceName?.Trim() ?? string.Empty;
        var unit = request.Unit?.Trim() ?? string.Empty;

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(serviceCode))
        {
            errors["ServiceCode"] = ["Mã dịch vụ (ServiceCode) là trường bắt buộc."];
        }
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            errors["ServiceName"] = ["Tên dịch vụ (ServiceName) là trường bắt buộc."];
        }
        if (string.IsNullOrWhiteSpace(unit))
        {
            errors["Unit"] = ["Đơn vị tính (Unit) là trường bắt buộc."];
        }
        if (request.UnitPrice <= 0)
        {
            errors["UnitPrice"] = ["Đơn giá (UnitPrice) phải lớn hơn 0."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors, "Dữ liệu thêm dòng đơn giá không hợp lệ.");
        }

        if (await _priceListRepository.ExistsItemByServiceCodeAsync(priceListId, serviceCode, cancellationToken))
        {
            throw new ConflictException($"Mã dịch vụ '{serviceCode}' đã tồn tại trong bảng giá có Id '{priceListId}'.");
        }

        var item = new PriceItem
        {
            PriceListId = priceListId,
            ServiceCode = serviceCode,
            ServiceName = serviceName,
            Unit = unit,
            UnitPrice = request.UnitPrice
        };

        await _priceListRepository.AddPriceItemAsync(item, cancellationToken);
        await _priceListRepository.SaveChangesAsync(cancellationToken);

        return item.ToResponse();
    }
}
