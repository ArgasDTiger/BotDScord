namespace BotDScord.Entities.Abstractions;

public interface ICreatable
{
    public DateTimeOffset CreatedAtUtc { get; init; }
}