namespace Hermes.SituationRoom.Domain.Services;

using System.Security.Claims;
using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

public class AuthorizationService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository, IActivistRepository activistRepository, IJournalistRepository journalistRepository) : IAuthorizationService
{

    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    public async Task<Guid> LoginActivist(LoginActivistBo loginActivistBo)
    {
        var activist = await activistRepository.GetActivistBoByUsernameAsync(loginActivistBo.UserName);

        if (activist.Password != loginActivistBo.Password)
            throw new UnauthorizedAccessException("Invalid password or username.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, activist.UserName),
            new Claim(ClaimTypes.NameIdentifier, activist.Uid.ToString()),
            new Claim(ClaimTypes.Role, "Activist")
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24),
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow,
        };

        if (HttpContext == null)
            throw new InvalidOperationException("No active HttpContext available.");

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return activist.Uid;
    }

    public async Task<Guid> LoginJournalist(LoginJournalistBo loginJournalistBo)
    {
        var journalist = await userRepository.GetUserBoByEmailAsync(loginJournalistBo.EmailAddress);

        if (journalist.Password != loginJournalistBo.Password)
            throw new UnauthorizedAccessException("Invalid password.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, journalist.EmailAddress),
            new Claim(ClaimTypes.NameIdentifier, journalist.Uid.ToString()),
            new Claim(ClaimTypes.Role, "Journalist")
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24),
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow,
        };

        if (HttpContext == null)
            throw new InvalidOperationException("No active HttpContext available.");

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return journalist.Uid;
    }

    public async Task Logout()
    {
        if (HttpContext == null)
            throw new InvalidOperationException("No active HttpContext available.");

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<CurrentUserBo?> GetCurrentUser()
    {
        if (HttpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = HttpContext.User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || roleClaim == null)
            return null;

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return null;

        var userType = roleClaim.Value.ToLower();

        // Fetch full user data based on type
        if (userType == "activist")
        {
            var activist = await activistRepository.GetActivistBoAsync(userId);
            return new CurrentUserBo
            {
                UserId = userId,
                UserType = "activist",
                UserData = activist
            };
        }
        else if (userType == "journalist")
        {
            var journalist = await journalistRepository.GetJournalistBoAsync(userId);
            return new CurrentUserBo
            {
                UserId = userId,
                UserType = "journalist",
                UserData = journalist
            };
        }

        return null;
    }
}