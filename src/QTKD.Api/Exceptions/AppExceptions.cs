namespace QTKD.Api.Exceptions;

/// <summary>
/// Exception gốc của ứng dụng. Mỗi lớp con gắn với một HTTP status code,
/// GlobalExceptionHandler dựa vào đó để trả ProblemDetails thống nhất (RFC 7807).
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string title, string message) : base(message)
    {
        Title = title;
    }

    public string Title { get; }
    public abstract int StatusCode { get; }
}

/// <summary>404 – Không tìm thấy tài nguyên.</summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string resource, object key)
        : base("Resource not found", $"{resource} with id '{key}' was not found.") { }

    public override int StatusCode => StatusCodes.Status404NotFound;
}

/// <summary>400 – Dữ liệu không hợp lệ hoặc vi phạm điều kiện kiểm tra.</summary>
public sealed class ValidationException : AppException
{
    public ValidationException(string field, string message)
        : base("One or more validation errors occurred.", message)
    {
        Errors = new Dictionary<string, string[]> { [field] = new[] { message } };
    }

    public ValidationException(IDictionary<string, string[]> errors, string message = "One or more validation errors occurred.")
        : base("One or more validation errors occurred.", message)
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
    public override int StatusCode => StatusCodes.Status400BadRequest;
}

/// <summary>409 – Trạng thái hiện tại không cho phép thao tác hoặc xung đột dữ liệu.</summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base("Conflict", message) { }

    public override int StatusCode => StatusCodes.Status409Conflict;
}

/// <summary>422 – Vi phạm quy tắc nghiệp vụ (Business Rule).</summary>
public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base("Business rule violated", message) { }

    public override int StatusCode => StatusCodes.Status422UnprocessableEntity;
}

