namespace AI_Voyage_Concierge.DTO;

public class ItineraryDto
{
    public string[]? Locations { get; set; }
    public int? NumberOfDays { get; set; }
    public string? FreeformText { get; set; }
    public string? ConversationId { get; set; }
}