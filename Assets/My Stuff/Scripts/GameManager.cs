using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;
    public GameObject playerPrefab;
    [Range(0, 1)]
    public float soundBlend = 0.8f;

    public Transform Focus;
    public CinemachineVirtualCamera Camera;

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

    private void ClientConnected(ulong id)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject go = Instantiate(playerPrefab);
            go.GetComponent<NetworkObject>().SpawnWithOwnership(id);
        }
    }
}
