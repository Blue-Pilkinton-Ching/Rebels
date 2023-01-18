using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkOrchestrator : MonoBehaviour
{
    public TextMeshProUGUI explaination;
    public TMP_InputField LobbyCode;
    public TMP_InputField MaxPlayers;
    public TMP_InputField lobbyName;
    public TMP_InputField mapName;
    public Toggle PrivateLobby;
    //public TMP_InputField mapName;
    public bool useRandomID = true;

    InitializationOptions initializationOptions = new();
    CreateLobbyOptions lobbyOptions = new();
    string authID;

    UnityTransport transport;

    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public async void OnPlayBtn(GameObject enableAfterCompletion)
    {
        explaination.text = "Authenticating";

        if (useRandomID)
        {
            initializationOptions.SetProfile(Random.Range(int.MinValue, int.MaxValue).ToString());
        }
        else
        {
            initializationOptions.SetProfile("Primary");
        }

        try
        {
            await UnityServices.InitializeAsync(initializationOptions);
        }
        catch 
        {
            explaination.text = "Failed to Authenticate";
            throw;
        }

        explaination.text = "Signing In";

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch
        {
            explaination.text = "Failed to Sign In";
            throw;
        }

        explaination.text = "Signed In";

        enableAfterCompletion.SetActive(true);
    }

    public async void OnJoinByCodeButton()
    {
        explaination.text = "Joining Lobby";

        Lobby lobby;

        try
        {
            lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(LobbyCode.text);
        }
        catch
        {
            explaination.text = "Failed to join Lobby";
            throw;
        }

        explaination.text = "Getting Relay Joincode";

        JoinAllocation a;

        try
        {
            a = await RelayService.Instance.JoinAllocationAsync(lobby.Data["JoinCode"].Value);
        }
        catch
        {
            explaination.text = "Failed getting Relay Joincode";
            throw;
        }

        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        explaination.text = "Retrieved Relay Joincode";

        NetworkManager.Singleton.StartClient();
    }
    public async void OnQuickJoinButton()
    {
        explaination.text = "Finding Lobby";

        Lobby lobby;

        try
        {
            lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        catch 
        {
            explaination.text = "Failed to find Lobby, consider making one!";
            throw;
        }

        explaination.text = "Getting Relay Joincode";

        JoinAllocation a;

        try
        {
            a = await RelayService.Instance.JoinAllocationAsync(lobby.Data["JoinCode"].Value);
        }
        catch
        {
            explaination.text = "Failed getting Relay Joincode";
            throw;
        }

        transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        explaination.text = "Retrieved Relay Joincode";

        NetworkManager.Singleton.StartClient();
    }

    public async void OnHostButton()
    {
        explaination.text = "Creating Relay Allocation";
        Allocation a;

        try
        {
            a = await RelayService.Instance.CreateAllocationAsync(int.Parse(MaxPlayers.text));
        }
        catch 
        {
            explaination.text = "Failed to create relay allocation";
            throw;
        }

        string joinCode;

        explaination.text = "Getting Relay Allocation Join Code";

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
        }
        catch
        {
            explaination.text = "Failed to get Relay Allocation Join Code";
            throw;
        }


        Lobby lobby;

        Dictionary<string, DataObject> lobbyOptionsData = new();

        lobbyOptionsData.Add("Map", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: mapName.text));
        lobbyOptionsData.Add("JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: joinCode));
        lobbyOptionsData.Add("Location", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: a.Region));

        lobbyOptions.IsPrivate = PrivateLobby.isOn;
        lobbyOptions.Data = lobbyOptionsData;

        explaination.text = "Creating Lobby";
        try
        {
            lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName.text, int.Parse(MaxPlayers.text), lobbyOptions);
        }
        catch 
        {
            explaination.text = "Failed to create lobby";
            throw;
        }

        explaination.text = "Sucessfully created lobby: " + lobby.LobbyCode;

        NetworkHelper.Singleton.StartCoroutine(NetworkHelper.Singleton.LobbyHeartBeat(lobby.Id));

        transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        SceneManager.LoadScene("Town");

        NetworkManager.Singleton.StartHost();
    }
}
