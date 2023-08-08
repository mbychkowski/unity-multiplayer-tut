using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class AuthenticationWrapper
{
  public static AuthState AuthState { get; private set; } = AuthState.Unauthenticated;

  public static async Task<AuthState> DoAuth(int maxRetries = 5)
  {
    if (AuthState == AuthState.Authenticated)
    {
      return AuthState;
    }

    if (AuthState == AuthState.Authenticating)
    {
      Debug.LogWarning("Already authenticating.");
      await Authenticating();
      return AuthState;
    }

    await SignInAnonymouslyAsync(maxRetries);

    return AuthState;
  }

  private static async Task<AuthState> Authenticating()
  {
    while (AuthState == AuthState.Authenticating || AuthState == AuthState.Unauthenticated)
    {
      await Task.Delay(200);
    }

    return AuthState;
  }

  private static async Task SignInAnonymouslyAsync(int maxRetries = 5)
  {
    AuthState = AuthState.Authenticating;
    int retries = 0;
    while (AuthState == AuthState.Authenticating && retries < maxRetries)
    {
      try
      {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
        {
          AuthState = AuthState.Authenticated;
          break;
        }
      }
      catch (AuthenticationException authException)
      {
        Debug.LogError(authException);
        AuthState = AuthState.Error;
      }
      catch (RequestFailedException reqException)
      {
        Debug.LogError(reqException);
        AuthState = AuthState.Error;
      }

      retries++;
      await Task.Delay(1000);
    }
    if (AuthState != AuthState.Authenticated)
    {
      Debug.LogWarning($"Player did not authenticate after {maxRetries} retries.");
      AuthState = AuthState.Timeout;
    }
  }
}

public enum AuthState
{
  Unauthenticated,
  Authenticating,
  Authenticated,
  Error,
  Timeout
}
