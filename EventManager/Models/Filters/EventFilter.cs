using System.ComponentModel.DataAnnotations;

namespace EventManager.Models.Filters;

public class EventFilter
{
    /// <summary>Filter by title (case-insensitive, substring match)</summary>
    public string? Title { get; init; }

    /// <summary>Start date (inclusive). Format: yyyy-MM-dd</summary>
    public DateTime? From { get; init; }

    /// <summary>End date (inclusive). Format: yyyy-MM-dd </summary>
    public DateTime? To { get; init; }

    /// <summary>Page number (starting from 1)</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1")]
    public int Page { get; init; } = 1;

    /// <summary>Items per page (10 by default)</summary>
    [Range(1, int.MaxValue, ErrorMessage = "PageSize must be greater than or equal to 1")]
    public int PageSize { get; init; } = 10;
}
