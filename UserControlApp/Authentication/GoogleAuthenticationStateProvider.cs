using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace UserControlApp.Authentication;

public class GoogleAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly UserService _userService;

    public User? CurrentUser { get; set; } = new();

    public GoogleAuthenticationStateProvider(UserService userService)
    {
        _userService = userService;
        AuthenticationStateChanged += OnAuthenticationStateChangedAsync;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = new ClaimsPrincipal();
        var user = _userService.FetchUserFromBrowser();

        if (user is not null)
        {
            var authenticatedUser = await _userService.SendAuthenticateRequestAsync(user.Username, user.Password);

            if (authenticatedUser is not null)
            {
                principal = authenticatedUser.ToClaimsPrincipal();
            }

            CurrentUser = authenticatedUser;
        }

        return new(principal);
    }

    public async Task LoginAsync(string username, string password)
    {
        var principal = new ClaimsPrincipal();
        var user = await _userService.SendAuthenticateRequestAsync(username, password);

        if (user is not null)
        {
            principal = user.ToClaimsPrincipal();
            CurrentUser = user;
        }


        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private async void OnAuthenticationStateChangedAsync(Task<AuthenticationState> task)
    {
        var authenticationState = await task;

        if (authenticationState is not null)
        {
            CurrentUser = User.FromClaimsPrincipal(authenticationState.User);
        }
    }

    public void Logout()
    {
        _userService.ClearBrowserUserData();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new())));
    }

    public void Dispose() => AuthenticationStateChanged -= OnAuthenticationStateChangedAsync;

    //[JSInvokable]
    //public void GoogleLogin(GoogleResponse googleResponse)
    //{
    //    var principal = new ClaimsPrincipal();
    //    var user = User.FromGoogleJwt(googleResponse.Credential);
    //    CurrentUser = user;

    //    if (user is not null)
    //    {
    //        principal = user.ToClaimsPrincipal();
    //    }

    //    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    //}
}