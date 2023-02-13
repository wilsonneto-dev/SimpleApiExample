namespace Identity.Dtos;

public record UserLoginOutput(
    bool Success,
    string? AccessToken = null,
    string? RefreshToken = null,
    string? UserId = null,
    string? Username = null);
