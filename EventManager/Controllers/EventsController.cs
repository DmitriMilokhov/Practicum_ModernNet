using EventManager.Infrastructure;
using EventManager.Interfaces;
using EventManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        var data = eventService.GetAllEvents();
        var msg = data.Count > 0 ? "Getting all events" : "There are no events";

        return Ok(new ApiResult<IReadOnlyCollection<FullEventDto>>
        {
            Data = data,
            Message = msg
        });

    }

    [HttpGet("{id}")]
    public IActionResult Get(Guid id)
    {
        try
        {
            var eventDto = eventService.GetEvent(id);
            return Ok(new ApiResult<FullEventDto>
            {
                Data = eventDto,
                Message = $"Getting data of the event: {id}"
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResult { Message = ex.Message });
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] EventDto eventDto)
    {
        var createdEvent = eventService.AddEvent(eventDto);
        return CreatedAtAction(nameof(Get), new { id = createdEvent.Id }, new ApiResult<FullEventDto>
        {
            Data = createdEvent,
            Message = $"Event created: {createdEvent.Id}"
        });
    }


    [HttpPut("{id}")]
    public IActionResult Update(Guid id, [FromBody] FullEventDto eventDto)
    {
        try
        {
            eventService.UpdateEvent(id, eventDto);
            return Ok(new ApiResult { Message = $"Event updated: {id}" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResult { Message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResult { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        try
        {
            eventService.DeleteEvent(id);
            return Ok(new ApiResult { Message = $"Event deleted: {id}" });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ApiResult { Message = ex.Message });
        }
    }
}
