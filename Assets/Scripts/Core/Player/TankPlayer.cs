using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class TankPlayer : NetworkBehaviour
{
  [Header("References")]
  [SerializeField] private CinemachineVirtualCamera vcam;

  [Header("Settings")]
  [SerializeField] private int ownerPriority = 15;

  public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
  public override void OnNetworkSpawn()
  {
    if (IsServer)
    {
      UserData userData = null;
      if (IsHost)
      {
        userData = HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
      }
      else
      {
        userData = ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
      }

      PlayerName.Value = userData.userName;
    }

    if (IsOwner)
    {
      vcam.Priority = ownerPriority;
    }
  }
}
