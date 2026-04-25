using EventManager.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Features.Events.Model;

public class EventFilter
{
    /// <summary>Filter by title (case-insensitive, substring match)</summary>
    public string? Title { get; init; }

    /// <summary>Start date (inclusive). Format: yyyy-MM-dd</summary>
    public DateTime? From { get; init; }

    /// <summary>End date (inclusive). Format: yyyy-MM-dd </summary>
    public DateTime? To { get; init; }

    /// <summary>Page number (starting from 1)</summary>
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.PageMustBeAboveOrEqualOne)]
    public int Page { get; init; } = 1;

    /// <summary>Items per page (10 by default)</summary>
    [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.PageSizeMustBeAboveOrEqualOne)]
    public int PageSize { get; init; } = 10;
}
