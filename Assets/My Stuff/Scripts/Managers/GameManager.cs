using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;
    public GameObject playerPrefab;

    public Transform Focus;

    public PauseMenu PauseMenu;
    public CinemachineVirtualCamera Camera;
    public Canvas PlayerDeadCanvas;

    public AudioClip[] soundtracks;
    public AudioSource soundTrackSource;

    public int maxPlayers = 100;
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

        Invoke("PlayRandomSoundTrack", Random.Range(3, 10));

        PlayerController.Players = new PlayerController[maxPlayers];

        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;

        if (NetworkManager.Singleton.IsServer)
        {
            ClientConnected(NetworkManager.Singleton.LocalClientId);
        }

    }

    private void PlayRandomSoundTrack()
    {
        soundTrackSource.clip = soundtracks[Random.Range(0, soundtracks.Length)];
        soundTrackSource.Play();
        Invoke("PlayRandomSoundTrack", Random.Range(20, 60) + soundTrackSource.clip.length);
    }
    public void Respawn()
    {
        PlayerController.Players[NetworkManager.Singleton.LocalClientId].Respawn();
    }

    private void ClientConnected(ulong id)
    {
        Debug.Log("Client Connected: " + id);

        if (NetworkManager.Singleton.IsServer)
        {
            GameObject go = Instantiate(playerPrefab);
            go.transform.position = SpawnController.Singleton.GetSpawnLocation();
            go.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        }
    }
}
