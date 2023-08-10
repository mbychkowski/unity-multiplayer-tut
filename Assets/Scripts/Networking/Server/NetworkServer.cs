using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;

public class NetworkServer : IDisposable
{
  private NetworkManager networkManager;
  private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
  private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();
  public Action<string> OnClientLeft;
  public Action<UserData> OnUserLeft;
  public Action<UserData> OnUserJoined;


  public NetworkServer(NetworkManager networkManager)
  {
    this.networkManager = networkManager;

    networkManager.ConnectionApprovalCallback += ApprovalCheck;
    networkManager.OnServerStarted += OnNetworkReady;
  }

  public bool OpenConnection(string ip, int port)
  {
    UnityTransport transport = networkManager.gameObject.GetComponent<UnityTransport>();
    transport.SetConnectionData(ip, (ushort)port);
    return networkManager.StartServer();
  }

  private void ApprovalCheck(
    NetworkManager.ConnectionApprovalRequest req,
    NetworkManager.ConnectionApprovalResponse res)
  {
    string payload = System.Text.Encoding.UTF8.GetString(req.Payload);
    UserData userData = JsonUtility.FromJson<UserData>(payload);

    clientIdToAuth[req.ClientNetworkId] = userData.userAuthId;
    authIdToUserData[userData.userAuthId] = userData;
    OnUserJoined?.Invoke(userData);

    res.Approved = true;
    res.CreatePlayerObject = true;
  }

  private void OnNetworkReady()
  {
    networkManager.OnClientDisconnectCallback += OnClientDisconnect;
  }

  private void OnClientDisconnect(ulong clientId)
  {
    if (clientIdToAuth.TryGetValue(clientId, out string authId))
    {
      clientIdToAuth.Remove(clientId);
      OnUserLeft?.Invoke(authIdToUserData[authId]);
      authIdToUserData.Remove(authId);
      OnClientLeft?.Invoke(authId);
    }
  }

  public UserData GetUserDataByClientId(ulong clientId)
  {
    if (clientIdToAuth.TryGetValue(clientId, out string authId))
    {
      if (authIdToUserData.TryGetValue(authId, out UserData data))
      {
        return data;
      }

      return null;
    }

    return null;
  }

  public void Dispose()
  {
    if (networkManager == null) { return;  }

    networkManager.ConnectionApprovalCallback -= ApprovalCheck;
    networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
    networkManager.OnServerStarted -= OnNetworkReady;

    if (networkManager.IsListening)
    {
      networkManager.Shutdown();
    }
  }
}
