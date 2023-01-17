using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
    public static NetworkHelper Singleton;

    private void Awake()
    {
        if (Singleton != null)
        {
            Debug.LogError("sada");
            return;
        }
        Singleton = this;
    }

    public IEnumerator LobbyHeartBeat(string lobbyID)
    {
        Lobbies.Instance.SendHeartbeatPingAsync(lobbyID);
        yield return new WaitForSeconds(15);
    }
}
