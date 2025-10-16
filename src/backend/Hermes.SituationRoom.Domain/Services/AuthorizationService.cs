namespace Hermes.SituationRoom.Domain.Services;

using System.Security.Claims;
using AutoMapper;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects;
using Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

public class AuthorizationService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository, IActivistRepository activistRepository, IJournalistRepository journalistRepository, IEncryptionService encryptionService, IMapper mapper) : IAuthorizationService
{

    private HttpContext HttpContext => httpContextAccessor.HttpContext;

    public async Task<Guid> LoginActivist(LoginActivistRequestDto loginActivistDto)
    {
        var loginActivistBo = mapper.Map<LoginActivistBo>(loginActivistDto);
        var activist = await activistRepository.GetActivistBoByUsernameAsync(loginActivistBo.UserName);

        if (!encryptionService.VerifyPassword(loginActivistBo.Password, activist.PasswordHash, activist.PasswordSalt))
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

    public async Task<Guid> LoginJournalist(LoginJournalistRequestDto loginJournalistDto)
    {
        var loginJournalistBo = mapper.Map<LoginJournalistBo>(loginJournalistDto);
        var journalist = await userRepository.GetUserBoByEmailAsync(loginJournalistBo.EmailAddress);

        if (!encryptionService.VerifyPassword(loginJournalistBo.Password, journalist.PasswordHash, journalist.PasswordSalt))
            throw new UnauthorizedAccessException("Invalid password or username.");

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

    public Task<CurrentUserDto?> GetCurrentUser()
    {
        if (HttpContext?.User?.Identity?.IsAuthenticated != true)
            return Task.FromResult<CurrentUserDto?>(null);

        var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = HttpContext.User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || roleClaim == null)
            return Task.FromResult<CurrentUserDto?>(null);

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return Task.FromResult<CurrentUserDto?>(null);

        var userType = roleClaim.Value.ToLower();

        var currentUserBo = new CurrentUserBo
        {
            UserId = userId,
            UserType = userType
        };

        var currentUserDto = mapper.Map<CurrentUserDto>(currentUserBo);
        return Task.FromResult(currentUserDto);
    }
}
