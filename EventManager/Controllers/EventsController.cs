using EventManager.Infrastructure;
using EventManager.Interfaces;
using EventManager.Models;
using EventManager.Models.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{

    /// <summary>
    /// Get events
    /// </summary>
    /// <param name="filter">
    /// Optional query filters:
    /// - Title: filters by title (case-insensitive, contains)
    /// - From: start date (inclusive)
    /// - To: end date (inclusive)
    /// </param>
    /// <response code="200"> Returns JSON ApiResult with events data. 
    /// If there are no any - empty list with corresponding message</response>
    [ProducesResponseType(typeof(ApiResult<PagedResponse<FullEventDto>>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    [HttpGet]
    public ActionResult<ApiResult<PagedResponse<FullEventDto>>> GetAll([FromQuery]EventFilter filter)
    {
        var data = eventService.GetEvents(filter);
        var msg = data.TotalItems > 0 ? "Getting events" : "There are no events";

        return Ok(new ApiResult<PagedResponse<FullEventDto>>
        {
            Data = data,
            Message = msg
        });

    }

    /// <summary>
    /// Get an event by its Guid
    /// </summary>
    /// <param name="id">Guid - id of an event to search</param>
    /// <response code="200"> Returns JSON ApiResult with an event data. </response>
    /// <response code="404">Returns JSON ApiErrorResult with corresponding message if event not found</response>
    [ProducesResponseType(typeof(ApiResult<FullEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpGet("{id}")]
    public ActionResult<ApiResult<FullEventDto>> Get(Guid id)
    {
        var eventDto = eventService.GetEvent(id);
        return Ok(new ApiResult<FullEventDto>
        {
            Data = eventDto,
            Message = $"Getting data of the event: {id}"
        });
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    /// <param name="eventDto">Data required to create an event</param>
    /// <response code="201">Returns JSON ApiResult with created event data</response>
    /// <response code="400">Returns JSON ApiErrorResult with corresponding message if there are validation errors</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<FullEventDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public ActionResult<ApiResult<FullEventDto>> Create([FromBody] EventDto eventDto)
    {
        var createdEvent = eventService.AddEvent(eventDto);
        return CreatedAtAction(nameof(Get), new { id = createdEvent.Id }, new ApiResult<FullEventDto>
        {
            Data = createdEvent,
            Message = $"Event created: {createdEvent.Id}"
        });
    }


    /// <summary>
    /// Update an existing event by its Guid
    /// </summary>
    /// <param name="id">Guid - id of an event to update</param>
    /// <param name="eventDto">Updated event data</param>
    /// <response code="200">Returns JSON ApiResult with successful update message</response>
    /// <response code="404">Returns JSON ApiErrorResult with corresponding message if event not found</response>
    /// <response code="400">Returns JSON ApiErrorResult with corresponding message if there are validation errors</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public ActionResult<ApiResult> Update(Guid id, [FromBody] EventDto eventDto)
    {
        eventService.UpdateEvent(id, eventDto);
        return Ok(new ApiResult { Message = $"Event updated: {id}" });
    }

    /// <summary>
    /// Delete an event by its Guid
    /// </summary>
    /// <param name="id">Guid - id of an event to delete</param>
    /// <response code="200">Returns JSON ApiResult with successful delete message</response>
    /// <response code="404">Returns JSON ApiErrorResult with corresponding message if event not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public ActionResult<ApiResult> Delete(Guid id)
    {
        eventService.DeleteEvent(id);
        return Ok(new ApiResult { Message = $"Event deleted: {id}" });
    }
}
