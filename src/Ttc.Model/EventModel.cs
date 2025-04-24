namespace Ttc.Model;

public enum EventType
{
    Unknown,
    PlayerStyleUpdated,
    MatchReport,
    MatchComment,
    MatchPicture,
    PlayerPicture,
}

public class EventModel
{
    public int Id { get; set; }

    public EventType Type { get; set; }
    public string? Data { get; set; }

    public int? TeamId { get; set; }
    public int? MatchId { get; set; }
    public int? PlayerId { get; set; }

    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = "";

    public override string ToString() => $"Id={Id}, {Type}";
}
