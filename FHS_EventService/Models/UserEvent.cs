using System.Text.Json;

namespace FHS_EventService.Models
{
    /// <summary>User activity event stored and returned by the API </summary>
    public record UserEvent(string UserId, string Type, DateTime Timestamp, JsonElement? Data);

    /// <summary>Incoming request body to POST events </summary>
    public record UserEventRequest(string? UserId, string? Type, DateTime? Timestamp, JsonElement? Data);
}

