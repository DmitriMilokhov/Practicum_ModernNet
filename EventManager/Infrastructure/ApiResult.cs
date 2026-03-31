using System.Net;

namespace EventManager.Infrastructure;

/// <summary>
///  Base class with result params
/// </summary>
public class ApiBaseResult
{
    /// <summary>
    /// Response Date & Time
    /// </summary>
    public DateTime DateTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional information
    /// </summary>
    public required string Message { get; set; }
}

public class ApiResult : ApiBaseResult { }

/// <summary>
///  Typed Result 
/// </summary>
public class ApiResult<T> : ApiBaseResult
{
    /// <summary>
    /// Data of necessary type
    /// </summary>
    public required T Data { get; set; }
}
