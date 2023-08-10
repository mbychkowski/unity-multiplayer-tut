using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEngine;

public class ClientGameManager : IDisposable
{
  private const string MenuSceneName = "Menu";
  private JoinAllocation joinAllocation;
  private NetworkClient networkClient;
  private MatchplayMatchmaker matchmaker;
  private UserData userData;
  public async Task<bool> InitAsync()
  {
    await UnityServices.InitializeAsync();

    networkClient = new NetworkClient(NetworkManager.Singleton);

    matchmaker = new MatchplayMatchmaker();

    AuthState authState = await AuthenticationWrapper.DoAuth();

    if (authState == AuthState.Authenticated)
    {
      userData = new UserData
      {
        userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
        userAuthId = AuthenticationService.Instance.PlayerId,
      };
      return true;
    }

    return false;
  }

  public void GoToMenu()
  {
    SceneManager.LoadScene(MenuSceneName);
  }

  public void StartClient(string ip, int port)
  {
    UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    transport.SetConnectionData(ip, (ushort)port);
    ConnectClient();
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

    ConnectClient();
  }

  private void ConnectClient()
  {
    string payload = JsonUtility.ToJson(userData);
    byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

    NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

    NetworkManager.Singleton.StartClient();
  }

  public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
  {
    if (matchmaker.IsMatchmaking)
    {
      return;
    }

    MatchmakerPollingResult matchResult = await GetMatchAsync();
    onMatchmakeResponse?.Invoke(matchResult);
  }

  public async Task CancelMatchmaking()
  {
    await matchmaker.CancelMatchmaking();
  }

  private async Task<MatchmakerPollingResult> GetMatchAsync()
  {
    MatchmakingResult matchmakingResult = await matchmaker.Matchmake(userData);

    if (matchmakingResult.result == MatchmakerPollingResult.Success)
    {
      StartClient(matchmakingResult.ip, matchmakingResult.port);
    }

    return matchmakingResult.result;
  }

  public void Dispose()
  {
    networkClient?.Dispose();
  }
}
