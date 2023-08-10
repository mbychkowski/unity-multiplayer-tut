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
  [SerializeField] private Texture2D crosshair;

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
        userData =
          HostSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
      }
      else
      {
        userData =
          ServerSingleton.Instance.GameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
      }

      PlayerName.Value = userData.userName;
    }

    if (IsOwner)
    {
      Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
      vcam.Priority = ownerPriority;
    }
  }
}
