namespace SobEvents.Application.DTOs;

public record PagedRequestDto
{
    private const int MaxPageSize = 50;
    public int Page {get;init;} = 1;
    private int _pageSize = 20;
    public int Pagesize
    {
        get => _pageSize;
        init => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
    public string? Search {get;init;}
}

public record PagedResponseDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize

){
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}