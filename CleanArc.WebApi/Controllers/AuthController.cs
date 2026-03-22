using CleanArc.WebApi.Contracts.Auth;
using CleanArc.WebApi.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CleanArc.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtOptions _jwt;
    private readonly JwtTokenService _tokenService;

    public AuthController(
        UserManager<IdentityUser> userManager,
        IOptions<JwtOptions> jwt,
        JwtTokenService tokenService)
    {
        _userManager = userManager;
        _jwt = jwt.Value;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            return Unauthorized(new { error = "Invalid credentials." });
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            return Unauthorized(new { error = "Invalid credentials." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Dispatcher";

        var token = _tokenService.CreateToken(user.UserName ?? request.Username, role);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes);

        return Ok(new LoginResponse(token, role, expires));
    }
}
