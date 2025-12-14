using BotDScord.Entities;

namespace BotDScord.Infrastructure.Extensions;

public static class UserExtensions
{
    public static string GetMentionName(this User user) => $"@{user.Id}";
}