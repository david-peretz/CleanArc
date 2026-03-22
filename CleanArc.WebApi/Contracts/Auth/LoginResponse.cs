namespace CleanArc.WebApi.Contracts.Auth;

public sealed record LoginResponse(string AccessToken, string Role, DateTime ExpiresAtUtc);
