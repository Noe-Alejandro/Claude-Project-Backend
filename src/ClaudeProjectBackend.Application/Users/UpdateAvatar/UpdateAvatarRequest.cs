using System.ComponentModel.DataAnnotations;

namespace ClaudeProjectBackend.Application.Users.UpdateAvatar;

public sealed record UpdateAvatarRequest(
    [Url] string? AvatarUrl);
