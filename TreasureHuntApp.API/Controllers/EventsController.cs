using Microsoft.AspNetCore.Mvc;
using TreasureHuntApp.API.Attributes;
using TreasureHuntApp.Infrastructure.Services;
using TreasureHuntApp.Shared.DTOs.Events;
using TreasureHuntApp.Shared.DTOs.Locations;
using TreasureHuntApp.Shared.DTOs.Teams;

namespace TreasureHuntApp.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[AdminAuthorize]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EventResponse>>> GetEvents()
    {
        var events = await eventService.GetEventsAsync();
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDetailResponse>> GetEvent(int id)
    {
        var eventDetail = await eventService.GetEventDetailAsync(id);

        return eventDetail is null ? NotFound($"Event with ID {id} not found") : Ok(eventDetail);
    }

    [HttpPost]
    public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.StartTime >= request.EndTime)
        {
            return BadRequest("Start time must be before end time");
        }

        if (request.StartTime < DateTime.UtcNow.AddMinutes(-5))
        {
            return BadRequest("Start time cannot be in the past");
        }

        try
        {
            var eventResponse = await eventService.CreateEventAsync(request);
            return CreatedAtAction(nameof(GetEvent), new { id = eventResponse.Id }, eventResponse);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create event: {ex.Message}");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventResponse>> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.StartTime >= request.EndTime)
        {
            return BadRequest("Start time must be before end time");
        }

        var updatedEvent = await eventService.UpdateEventAsync(id, request);

        return updatedEvent is null ? NotFound($"Event with ID {id} not found or cannot be updated") : Ok(updatedEvent);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteEvent(int id)
    {
        var success = await eventService.DeleteEventAsync(id);

        return !success ? NotFound($"Event with ID {id} not found or cannot be deleted") : NoContent();
    }

    [HttpPost("{id:int}/start")]
    public async Task<ActionResult> StartEvent(int id)
    {
        var success = await eventService.StartEventAsync(id);

        return !success ? BadRequest($"Event with ID {id} cannot be started") : Ok(new { message = "Event started successfully" });
    }

    [HttpPost("{id:int}/end")]
    public async Task<ActionResult> EndEvent(int id)
    {
        var success = await eventService.EndEventAsync(id);

        return !success ? BadRequest($"Event with ID {id} cannot be ended") : Ok(new { message = "Event ended successfully" });
    }

    [HttpGet("stats")]
    public async Task<ActionResult<EventStatsResponse>> GetEventStats()
    {
        var stats = await eventService.GetEventStatsAsync();
        return Ok(stats);
    }

    [HttpGet("{id:int}/teams")]
    public async Task<ActionResult<List<TeamResponse>>> GetEventTeams(int id)
    {
        var teams = await eventService.GetEventTeamsAsync(id);
        return Ok(teams);
    }

    [HttpGet("{id:int}/teams/progress")]
    public async Task<ActionResult<List<TeamResponse>>> GetEventTeamProgress(int id)
    {
        var teams = await eventService.GetEventTeamProgressAsync(id);
        return Ok(teams);
    }

    [HttpPost("{id:int}/teams")]
    public async Task<ActionResult<TeamResponse>> CreateTeam(int id, [FromBody] CreateTeamRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        request.EventId = id;

        try
        {
            var team = await eventService.CreateTeamAsync(request);
            return CreatedAtAction(nameof(GetEvent), new { id = team.Id }, team);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create team: {ex.Message}");
        }
    }

    [HttpDelete("teams/{teamId:int}")]
    public async Task<ActionResult> DeleteTeam(int teamId)
    {
        var success = await eventService.DeleteTeamAsync(teamId);

        return !success ? NotFound($"Team with ID {teamId} not found") : NoContent();
    }

    [HttpGet("{id:int}/locations")]
    public async Task<ActionResult<List<LocationResponse>>> GetEventLocations(int id)
    {
        var locations = await eventService.GetEventLocationsAsync(id);
        return Ok(locations);
    }

    [HttpPost("{id:int}/locations")]
    public async Task<ActionResult<LocationResponse>> AddLocationToEvent(int id, [FromBody] CreateLocationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var location = await eventService.AddLocationToEventAsync(id, request);
            return Ok(location);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to add location: {ex.Message}");
        }
    }

    [HttpPut("locations/{locationId:int}")]
    public async Task<ActionResult<LocationResponse>> UpdateLocation(int locationId, [FromBody] CreateLocationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedLocation = await eventService.UpdateLocationAsync(locationId, request);

        return updatedLocation is null ? NotFound($"Location with ID {locationId} not found") : Ok(updatedLocation);
    }

    [HttpDelete("locations/{locationId:int}")]
    public async Task<ActionResult> DeleteLocation(int locationId)
    {
        var success = await eventService.DeleteLocationAsync(locationId);

        return !success ? NotFound($"Location with ID {locationId} not found") : NoContent();
    }
}
