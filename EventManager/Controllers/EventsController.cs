using EventManager.Infrastructure;
using EventManager.Interfaces;
using EventManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{

    /// <summary>
    /// Get all events
    /// </summary>
    /// <response code="200"> Returns JSON ApiBaseResult with events data. 
    /// If there are no any - empty list with corresponding message</response>
    [ProducesResponseType(typeof(ApiResult<IReadOnlyCollection<FullEventDto>>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    [HttpGet]
    public ActionResult<ApiResult<IReadOnlyCollection<FullEventDto>>> GetAll()
    {
        var data = eventService.GetAllEvents();
        var msg = data.Count > 0 ? "Getting all events" : "There are no events";

        return Ok(new ApiResult<IReadOnlyCollection<FullEventDto>>
        {
            Data = data,
            Message = msg
        });

    }

    /// <summary>
    /// Get an event by its Guid
    /// </summary>
    /// <param name="id">Guid - id of an event to search</param>
    /// <response code="200"> Returns JSON ApiBaseResult with an event data. </response>
    [ProducesResponseType(typeof(ApiResult<FullEventDto>), StatusCodes.Status200OK)]
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
    /// <response code="201">Returns JSON ApiBaseResult with created event data</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<FullEventDto>), StatusCodes.Status201Created)]
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
    /// <response code="200">Returns JSON ApiBaseResult with successful update message</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
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
    /// <response code="200">Returns JSON ApiBaseResult with successful delete message</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status200OK)]
    [Produces("application/json")]
    public ActionResult<ApiResult> Delete(Guid id)
    {
        eventService.DeleteEvent(id);
        return Ok(new ApiResult { Message = $"Event deleted: {id}" });
    }
}
