using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ClientGameManager
{
  private const string MenuSceneName = "Menu";
  private JoinAllocation joinAllocation;
  public async Task<bool> InitAsync()
  {
    await UnityServices.InitializeAsync();

    AuthState authState = await AuthenticationWrapper.DoAuth();

    if (authState == AuthState.Authenticated)
    {
      return true;
    }

    return false;
  }

  public void GoToMenu()
  {
    SceneManager.LoadScene(MenuSceneName);
  }

  public async Task StartClientAsync(string joinCode)
  {
    try
    {
      joinAllocation = await Relay.Instance.JoinAllocationAsync(joinCode);
    }
    catch (Exception e)
    {
      Debug.Log(e);
      return;
    }

    UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

    RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp");
    transport.SetRelayServerData(relayServerData);

    NetworkManager.Singleton.StartClient();
  }
}
