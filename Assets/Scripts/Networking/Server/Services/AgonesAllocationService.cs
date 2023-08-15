using System;
using System.Threading;
using System.Threading.Tasks;
using Agones;
using Agones.Dev.Sdk;
using UnityEngine;

public class AgonesAllocationService : IDisposable
{
  // private IAgonesService agonesService;
  private AgonesSDK agonesService;
  private CancellationTokenSource serverCheckCancel;
  string allocationId;

  public AgonesAllocationService()
  {
    try
    {
      agonesService = new AgonesSDK();
    }
    catch (Exception ex)
    {
      Debug.LogWarning($"Error creating Agones allocation service.\n{ex}");
    }
  }

  private async Task<string> AwaitAllocationID()
  {
    GameServer serverConfig = await agonesService.GetGameServerAsync();
    Debug.Log($"Awaiting Allocation. Server Config is:\n" +
        $"-ServerID: {serverConfig.ObjectMeta.Uid}\n" +
        $"-AllocationID: {serverConfig.Status.Address}\n" +
        $"-Address: {serverConfig.Status.Address}\n" +
        $"-Port: {serverConfig.Status.Ports}\n");

    while (string.IsNullOrEmpty(allocationId))
    {
      var config = agonesService.AllocateAsync();
      string configID = serverConfig.ObjectMeta.Uid;

      if (!string.IsNullOrEmpty(configID) && string.IsNullOrEmpty(allocationId))
      {
        Debug.Log($"Config had AllocationID: {configID}");
        allocationId = configID;
      }

      await Task.Delay(100);
    }

    return allocationId;
  }

  public async Task StartServerAsync()
  {
    if (agonesService == null) { return; }
    await agonesService.ConnectAsync();
  }

  public void Dispose()
  {
  }
}