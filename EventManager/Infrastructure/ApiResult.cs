using Microsoft.AspNetCore.Mvc;

namespace EventManager.Infrastructure;

/// <summary>
///  Base class with result params
/// </summary>
public class ApiBaseResult
{
    /// <summary>
    /// Response Creation DateTime
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

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

/// <summary>
///  Error Result 
/// </summary>
public class ApiErrorResult : ApiBaseResult
{
    /// <summary>
    /// Error Data
    /// </summary>
    public ProblemDetails ErrorDeatails { get; set; }
}
