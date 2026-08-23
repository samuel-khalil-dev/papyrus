namespace Papyrus.Contracts.Content;

public sealed record PageResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int Total,
    int TotalPages);

public sealed record ApiValidationError(string Code, string Message);
