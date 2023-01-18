using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;
    public GameObject playerPrefab;
    [Range(0, 1)]
    public float soundBlend = 0.8f;

    public Transform Focus;
    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Debug.LogError("sada");
        }

        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
        ClientConnected(NetworkManager.Singleton.LocalClientId);
    }

    private void ClientConnected(ulong id)
    {
        Debug.Log("Client: " + id + " Connected");
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject go = Instantiate(playerPrefab);
            go.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        }
    }
}
