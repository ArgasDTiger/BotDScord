namespace BotDScord.Entities.Abstractions;

public interface IDeletable
{
    public DateTimeOffset? DeletedAtUtc { get; init; }
}