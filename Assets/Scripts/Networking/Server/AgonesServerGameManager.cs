using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AgonesServerGameManager : IDisposable
{
  private string serverIP;
  private int serverPort;
  private int queryPort;
  private AgonesAllocationService agonesAllocationService;
  private const string GameSceneName = "Game";

  public NetworkServer NetworkServer { get; private set; }

  public AgonesServerGameManager(string serverIP, int serverPort, NetworkManager manager)
  {
    this.serverIP = serverIP;
    this.serverPort = serverPort;
    this.queryPort = queryPort;
    NetworkServer = new NetworkServer(manager);
    agonesAllocationService = new AgonesAllocationService();
  }
  public async Task StartGameServerAsync()
  {
    await agonesAllocationService.StartServerAsync();

    if (!NetworkServer.OpenConnection(serverIP, serverPort))
    {
      Debug.LogWarning("Network server could not be opened.");
      return;
    }
    NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
  }

  public void Dispose()
  {
    NetworkServer?.Dispose();
  }
}
