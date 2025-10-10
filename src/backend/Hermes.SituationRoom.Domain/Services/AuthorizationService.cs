namespace Hermes.SituationRoom.Domain.Services;

using System.Security.Claims;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

public class AuthorizationService : IAuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    private readonly IActivistRepository _activistRepository;

    public AuthorizationService(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository,
        IActivistRepository activistRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _activistRepository = activistRepository;
    }

    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public async Task<Guid> LoginActivist(LoginActivistBo loginActivistBo)
    {
        var activist = await _activistRepository.GetActivistBoByUsernameAsync(loginActivistBo.UserName);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, activist.UserName),
            new Claim(ClaimTypes.NameIdentifier, activist.Uid.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
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
        var journalist = await _userRepository.GetUserBoByEmailAsync(loginJournalistBo.EmailAddress);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, journalist.EmailAddress),
            new Claim(ClaimTypes.NameIdentifier, journalist.Uid.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
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
}