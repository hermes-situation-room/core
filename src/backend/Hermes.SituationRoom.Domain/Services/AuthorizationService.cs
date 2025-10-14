namespace Hermes.SituationRoom.Domain.Services;

using System.Diagnostics;
using System.Security.Claims;
using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

public class AuthorizationService(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository, IActivistRepository activistRepository, IEncryptionService encryptionService) : IAuthorizationService
{

    private HttpContext HttpContext => httpContextAccessor.HttpContext;

    public async Task<Guid> LoginActivist(LoginActivistBo loginActivistBo)
    {
        var activist = await activistRepository.GetActivistBoByUsernameAsync(loginActivistBo.UserName);

        if (!encryptionService.VerifyPassword(loginActivistBo.Password, activist.PasswordHash, activist.PasswordSalt))
            throw new UnauthorizedAccessException("Invalid password or username.");

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
        var journalist = await userRepository.GetUserBoByEmailAsync(loginJournalistBo.EmailAddress);

        if (!encryptionService.VerifyPassword(loginJournalistBo.Password, journalist.PasswordHash, journalist.PasswordSalt))
            throw new UnauthorizedAccessException("Invalid password or username.");

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