using System.Security.Claims;
using db_final_proj.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace db_final_proj.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _protectedLocalStore;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
    private ClaimsPrincipal? _cachedUser;

    public CustomAuthStateProvider(ProtectedLocalStorage protectedLocalStore)
    {
        _protectedLocalStore = protectedLocalStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cachedUser != null)
        {
            return new AuthenticationState(_cachedUser);
        }

        try
        {
            var result = await _protectedLocalStore.GetAsync<User>("UserSession");
            if (result.Success && result.Value != null)
            {
                _cachedUser = CreatePrincipalFromUser(result.Value);
                return new AuthenticationState(_cachedUser);
            }
        }
        catch
        {
            // JS Interop is not available during prerendering or if the circuit is disconnected
        }

        return new AuthenticationState(_anonymous);
    }

    public void MarkUserAsAuthenticated(User user)
    {
        _cachedUser = CreatePrincipalFromUser(user);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_cachedUser)));
    }

    public void MarkUserAsLoggedOut()
    {
        _cachedUser = _anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private ClaimsPrincipal CreatePrincipalFromUser(User user)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.UserEmail),
        }, "CustomAuth");

        return new ClaimsPrincipal(identity);
    }
}
