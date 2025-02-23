using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Encodings.Web;
using System.Text.Json;
using Ttc.DataEntities.Core;
using Ttc.Model.Matches;

namespace Ttc.DataEntities;

public enum EventType
{
    Unknown,
    PlayerStyleUpdated,
    MatchReport,
    MatchComment,
    MatchPicture,
}

/// <summary>
/// Something that happened in the club
/// </summary>
[Table("Event")]
public class EventEntity : IAudit
{
    [Key]
    public int Id { get; set; }

    public EventType Type { get; set; }
    [StringLength(1000)]
    public string? Data { get; set; }

    public int? TeamId { get; set; }
    public int? MatchId { get; set; }
    public int? PlayerId { get; set; }

    public Audit Audit { get; } = new();

    #region Event Creators
    public static EventEntity UpdatePlayerStyle(PlayerEntity player, int? currentPlayerId)
    {
        var data = new
        {
            PlayerId = player.Id,
            player.Style,
            player.BestStroke,
        };

        return new EventEntity()
        {
            Type = EventType.PlayerStyleUpdated,
            PlayerId = currentPlayerId,
            Data = JsonSerializer.Serialize(data, Options),
        };
    }

    public static EventEntity MatchReport(MatchReport report)
    {
        return new EventEntity()
        {
            Type = EventType.MatchReport,
            MatchId = report.MatchId,
            PlayerId = report.PlayerId,
        };
    }

    public static EventEntity MatchComment(MatchCommentEntity comment)
    {
        return new EventEntity()
        {
            Type = string.IsNullOrWhiteSpace(comment.ImageUrl) ? EventType.MatchComment : EventType.MatchPicture,
            MatchId = comment.MatchId,
            PlayerId = comment.PlayerId,
            Data = JsonSerializer.Serialize(new { CommentId = comment.Id }),
        };
    }

    public static EventEntity MatchPicture(int matchId)
    {
        return new EventEntity()
        {
            Type = EventType.MatchPicture,
            MatchId = matchId,
        };
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    #endregion

    public override string ToString() => $"Id={Id}, {Type}";
}
