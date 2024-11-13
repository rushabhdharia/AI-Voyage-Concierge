namespace AI_Voyage_Concierge.DTO;

public class InformationDTO
{
    public string? Location { get; set; }
    public string? FreeformText { get; set; }
    public List<InformationType>? InformationTypes { get; set; }
    public string? ConversationId { get; set; }
}

public enum InformationType
{
    FunFact = 0,
    History = 1,
    LocalCustomsAndTraditions = 2,
    HiddenGems = 3,
    CulturalFactsAndSignificance = 4,
    CurrentOwnership = 5,
    PreviousOwnerships = 6
}