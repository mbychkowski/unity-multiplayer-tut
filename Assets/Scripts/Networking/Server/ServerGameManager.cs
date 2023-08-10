using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerGameManager : IDisposable
{
  private string serverIP;
  private int serverPort;
  private int queryPort;
  private MultiplayAllocationService multiplayAllocationService;
  private MatchplayBackfiller backfiller;
  private const string GameSceneName = "Game";

  public NetworkServer NetworkServer { get; private set; }

  public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager)
  {
    this.serverIP = serverIP;
    this.serverPort = serverPort;
    this.queryPort = queryPort;
    NetworkServer = new NetworkServer(manager);
    multiplayAllocationService = new MultiplayAllocationService();
  }
  public async Task StartGameServerAsync()
  {
    await multiplayAllocationService.BeginServerCheck();

    try
    {
      MatchmakingResults matchmakerPayload = await GetMatchmakerPayloadAsync();
      if (matchmakerPayload != null)
      {
        await StartBackfill(matchmakerPayload);
        NetworkServer.OnUserJoined += UserJoined;
        NetworkServer.OnUserLeft += UserLeft;
      }
      else
      {
        Debug.LogWarning("Matchmaker payload time out");
      }
    }
    catch (Exception e)
    {
      Debug.LogWarning(e);
    }

    if (!NetworkServer.OpenConnection(serverIP, serverPort))
    {
      Debug.LogWarning("Network server could not be opened.");
      return;
    }
    NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
  }

  private async Task<MatchmakingResults> GetMatchmakerPayloadAsync()
  {
    Task<MatchmakingResults> matchmakerPayloadTask =
      multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

    if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask )
    {
      return matchmakerPayloadTask.Result;
    }

    return null;
  }

  private async Task StartBackfill(MatchmakingResults payload)
  {
    backfiller = new MatchplayBackfiller($"{serverIP}:{serverPort}",
      payload.QueueName,
      payload.MatchProperties,
      20);

    if (backfiller.NeedsPlayers())
    {
      await backfiller.BeginBackfilling();
    }
  }


  private void UserJoined(UserData user)
  {
    backfiller.AddPlayerToMatch(user);
    multiplayAllocationService.AddPlayer();
    if (!backfiller.NeedsPlayers() && backfiller.IsBackfilling)
    {
      _ = backfiller.StopBackfill();
    }
  }

  private void UserLeft(UserData user)
  {
    int playerClount = backfiller.RemovePlayerFromMatch(user.userAuthId);
    multiplayAllocationService.RemovePlayer();

    if (playerClount <= 0)
    {
      CloseServer();
      return;
    }

    if (backfiller.NeedsPlayers() && !backfiller.IsBackfilling)
    {
      _ = backfiller.BeginBackfilling();
    }
  }

  private async void CloseServer()
  {
    await backfiller.StopBackfill();
    Dispose();
    Application.Quit();
  }

  public void Dispose()
  {
    NetworkServer.OnUserJoined -= UserJoined;
    NetworkServer.OnUserLeft -= UserLeft;

    backfiller?.Dispose();
    multiplayAllocationService?.Dispose();
    NetworkServer?.Dispose();
  }
}
