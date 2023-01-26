using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
    public static NetworkHelper Singleton = null;
    private void Awake()
    {
        if (Singleton != null)
        {
            Debug.LogError("sadas");
            return;
        }
        Singleton = this;
    }

    public IEnumerator LobbyHeartBeat(string lobbyID)
    {
        while (true)
        {
            Debug.Log("Sending Lobby Heartbeat");
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyID);
            yield return new WaitForSeconds(15);
        }
    }
}
