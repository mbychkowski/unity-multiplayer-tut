using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

  [SerializeField] private TMP_Text queueStatusText;
  [SerializeField] private TMP_Text queueTimerText;
  [SerializeField] private TMP_Text queueFindMatchButtonText;
  [SerializeField] private TMP_InputField joinCodeField;

  private bool isMatchMaking;
  private bool isCancelling;

  private void Start()
  {
    if (ClientSingleton.Instance == null) { return; }

    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

    queueStatusText.text = string.Empty;
    queueTimerText.text = string.Empty;
  }

  public async void HandleFindMatch()
  {
    if (isCancelling) { return; }

    if (isMatchMaking)
    {
      queueStatusText.text = "Cancelling...";
      isCancelling = true;
      await ClientSingleton.Instance.GameManager.CancelMatchmaking();
      isCancelling = false;
      isMatchMaking = false;
      queueFindMatchButtonText.text = "Find Match";
      queueStatusText.text = string.Empty;
      return;
    }
    ClientSingleton.Instance.GameManager.MatchmakeAsync(OnMatchMade);
    queueFindMatchButtonText.text = "Cancel";
    queueStatusText.text = "Searching...";
    isMatchMaking = true;
  }

  private void OnMatchMade(MatchmakerPollingResult result)
  {
    switch(result)
    {
      case MatchmakerPollingResult.Success:
        queueStatusText.text = "Connecting...";
        break;
      case MatchmakerPollingResult.TicketCreationError:
        queueStatusText.text = "TicketCreationError";
        break;
      case MatchmakerPollingResult.TicketCancellationError:
        queueStatusText.text = "TicketCancellationError";
        break;
      case MatchmakerPollingResult.TicketRetrievalError:
        queueStatusText.text = "TicketRetrievalError";
        break;
      case MatchmakerPollingResult.MatchAssignmentError:
        queueStatusText.text = "MatchAssignmentError";
        break;
    };
  }

  public async void StartHost()
  {
    await HostSingleton.Instance.GameManager.StartHostAsync();
  }

  public async void StartClient()
    {
      await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
    }
}
