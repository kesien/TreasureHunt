using Microsoft.AspNetCore.Mvc;
using TreasureHuntApp.API.Attributes;
using TreasureHuntApp.Infrastructure.Services;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[AdminAuthorize] // All endpoints require admin authentication
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    // GET: api/events
    [HttpGet]
    public async Task<ActionResult<List<EventResponse>>> GetEvents()
    {
        var events = await _eventService.GetEventsAsync();
        return Ok(events);
    }

    // GET: api/events/5
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDetailResponse>> GetEvent(int id)
    {
        var eventDetail = await _eventService.GetEventDetailAsync(id);

        return eventDetail == null ? (ActionResult<EventDetailResponse>)NotFound($"Event with ID {id} not found") : (ActionResult<EventDetailResponse>)Ok(eventDetail);
    }

    // POST: api/events
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

        if (request.StartTime < DateTime.UtcNow.AddMinutes(-5)) // Allow 5 minute grace period
        {
            return BadRequest("Start time cannot be in the past");
        }

        try
        {
            var eventResponse = await _eventService.CreateEventAsync(request);
            return CreatedAtAction(nameof(GetEvent), new { id = eventResponse.Id }, eventResponse);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create event: {ex.Message}");
        }
    }

    // PUT: api/events/5
    [HttpPut("{id}")]
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

        var updatedEvent = await _eventService.UpdateEventAsync(id, request);

        return updatedEvent == null ? (ActionResult<EventResponse>)NotFound($"Event with ID {id} not found or cannot be updated") : (ActionResult<EventResponse>)Ok(updatedEvent);
    }

    // DELETE: api/events/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEvent(int id)
    {
        var success = await _eventService.DeleteEventAsync(id);

        return !success ? NotFound($"Event with ID {id} not found or cannot be deleted") : NoContent();
    }

    // POST: api/events/5/start
    [HttpPost("{id}/start")]
    public async Task<ActionResult> StartEvent(int id)
    {
        var success = await _eventService.StartEventAsync(id);

        return !success ? BadRequest($"Event with ID {id} cannot be started") : Ok(new { message = "Event started successfully" });
    }

    // POST: api/events/5/end
    [HttpPost("{id}/end")]
    public async Task<ActionResult> EndEvent(int id)
    {
        var success = await _eventService.EndEventAsync(id);

        return !success ? BadRequest($"Event with ID {id} cannot be ended") : Ok(new { message = "Event ended successfully" });
    }

    // GET: api/events/stats
    [HttpGet("stats")]
    public async Task<ActionResult<EventStatsResponse>> GetEventStats()
    {
        var stats = await _eventService.GetEventStatsAsync();
        return Ok(stats);
    }

    // GET: api/events/5/teams
    [HttpGet("{id}/teams")]
    public async Task<ActionResult<List<TeamResponse>>> GetEventTeams(int id)
    {
        var teams = await _eventService.GetEventTeamsAsync(id);
        return Ok(teams);
    }

    // GET: api/events/5/teams/progress
    [HttpGet("{id}/teams/progress")]
    public async Task<ActionResult<List<TeamResponse>>> GetEventTeamProgress(int id)
    {
        var teams = await _eventService.GetEventTeamProgressAsync(id);
        return Ok(teams);
    }

    // POST: api/events/5/teams
    [HttpPost("{id}/teams")]
    public async Task<ActionResult<TeamResponse>> CreateTeam(int id, [FromBody] CreateTeamRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Ensure the team is created for the correct event
        request.EventId = id;

        try
        {
            var team = await _eventService.CreateTeamAsync(request);
            return CreatedAtAction(nameof(GetEvent), new { id = team.Id }, team);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to create team: {ex.Message}");
        }
    }

    // DELETE: api/events/teams/5
    [HttpDelete("teams/{teamId}")]
    public async Task<ActionResult> DeleteTeam(int teamId)
    {
        var success = await _eventService.DeleteTeamAsync(teamId);

        return !success ? NotFound($"Team with ID {teamId} not found") : NoContent();
    }

    // GET: api/events/5/locations
    [HttpGet("{id}/locations")]
    public async Task<ActionResult<List<LocationResponse>>> GetEventLocations(int id)
    {
        var locations = await _eventService.GetEventLocationsAsync(id);
        return Ok(locations);
    }

    // POST: api/events/5/locations
    [HttpPost("{id}/locations")]
    public async Task<ActionResult<LocationResponse>> AddLocationToEvent(int id, [FromBody] CreateLocationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var location = await _eventService.AddLocationToEventAsync(id, request);
            return Ok(location);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to add location: {ex.Message}");
        }
    }

    // PUT: api/events/locations/5
    [HttpPut("locations/{locationId}")]
    public async Task<ActionResult<LocationResponse>> UpdateLocation(int locationId, [FromBody] CreateLocationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedLocation = await _eventService.UpdateLocationAsync(locationId, request);

        return updatedLocation == null ? (ActionResult<LocationResponse>)NotFound($"Location with ID {locationId} not found") : (ActionResult<LocationResponse>)Ok(updatedLocation);
    }

    // DELETE: api/events/locations/5
    [HttpDelete("locations/{locationId}")]
    public async Task<ActionResult> DeleteLocation(int locationId)
    {
        var success = await _eventService.DeleteLocationAsync(locationId);

        return !success ? NotFound($"Location with ID {locationId} not found") : NoContent();
    }
}
