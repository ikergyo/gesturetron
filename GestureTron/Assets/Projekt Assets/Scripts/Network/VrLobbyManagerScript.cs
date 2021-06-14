using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class VrLobbyManagerScript : NetworkLobbyManager
{

    static public VrLobbyManagerScript s_Singleton;

    public RectTransform ServerListTransform;
    public RectTransform RoomPlayerListTransform;

    protected LobbyHook _lobbyHooks;
    protected RectTransform currentPanel;

    public LobbyCountdownPanel countdownPanel;

    [HideInInspector]
    public int _playerNumber = 0;

    //used to disconnect a client properly when exiting the matchmaker
    [HideInInspector]
    public bool _isMatchmaking = false;

    protected bool _disconnectServer = false;

    protected ulong _currentMatchID;

    public NetworkServerDiscovery nsDiscovery;
    public GameListController glc;

    [Tooltip("Time in second between all players ready & match start")]
    public float prematchCountdown = 5.0f;

    void Start()
    {
        s_Singleton = this;
        _lobbyHooks = GetComponent<Prototype.NetworkLobby.LobbyHook>();
        

        nsDiscovery.glc = this.glc;
        
        GetComponent<Canvas>().enabled = true;

        DontDestroyOnLoad(gameObject);

    }
    public override void OnLobbyClientSceneChanged(NetworkConnection conn)
    {
        ServerListTransform.gameObject.SetActive(false);
        RoomPlayerListTransform.gameObject.SetActive(false);


    }
    public void ChangeTo(RectTransform newPanel)
    {
        if (currentPanel != null)
        {
            currentPanel.gameObject.SetActive(false);
        }

        if (newPanel != null)
        {
            newPanel.gameObject.SetActive(true);
        }

        currentPanel = newPanel;

    }
    public override void OnStartHost()
    {
        base.OnStartHost();
        nsDiscovery.StartBroadcast();
    }
    public void AddLocalPlayer()
    {
        TryToAddPlayer();
    }

    public void RemovePlayer(VrLobbyPlayerListElementScript player)
    {
        player.RemovePlayer();
    }
    public void StopHostClbk()
    {
        if (_isMatchmaking)
        {
            matchMaker.DestroyMatch((NetworkID)_currentMatchID, 0, OnDestroyMatch);
            _disconnectServer = true;
        }
        else
        {
            StopHost();
        }


        ChangeTo(null);
    }

    public void StopClientClbk()
    {
        StopClient();

        if (_isMatchmaking)
        {
            StopMatchMaker();
        }

        ChangeTo(null);
    }

    public void StopServerClbk()
    {
        StopServer();
        ChangeTo(null);
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        _currentMatchID = (System.UInt64)matchInfo.networkId;
    }

    public override void OnDestroyMatch(bool success, string extendedInfo)
    {
        base.OnDestroyMatch(success, extendedInfo);
        if (_disconnectServer)
        {
            StopMatchMaker();
            StopHost();
        }
    }

   

    public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;

        VrLobbyPlayerListElementScript newPlayer = obj.GetComponent<VrLobbyPlayerListElementScript>();

        //newPlayer.GetComponent<SelectionSlider>().
        newPlayer.ToggleJoinButton(numPlayers + 1 >= minPlayers);


        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            VrLobbyPlayerListElementScript p = lobbySlots[i] as VrLobbyPlayerListElementScript;

            if (p != null)
            {
                p.RpcUpdateRemoveButton();
                p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            }
        }

        return obj;
    }
    public override void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
    {
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            VrLobbyPlayerListElementScript p = lobbySlots[i] as VrLobbyPlayerListElementScript;

            if (p != null)
            {
                p.RpcUpdateRemoveButton();
                p.ToggleJoinButton(numPlayers + 1 >= minPlayers);
            }
        }
    }

    public override void OnLobbyServerDisconnect(NetworkConnection conn)
    {
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            VrLobbyPlayerListElementScript p = lobbySlots[i] as VrLobbyPlayerListElementScript;

            if (p != null)
            {
                p.RpcUpdateRemoveButton();
                p.ToggleJoinButton(numPlayers >= minPlayers);
            }
        }

    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        //This hook allows you to apply state data from the lobby-player to the game-player
        //just subclass "LobbyHook" and add it to the lobby object.

        if (_lobbyHooks)
            _lobbyHooks.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);

        return true;
    }
    public override void OnLobbyServerPlayersReady()
    {
        bool allready = true;
        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
                allready &= lobbySlots[i].readyToBegin;
        }

        if (allready)
            StartCoroutine(ServerCountdownCoroutine());
    }
    public IEnumerator ServerCountdownCoroutine()
    {
        float remainingTime = prematchCountdown;
        int floorTime = Mathf.FloorToInt(remainingTime);

        while (remainingTime > 0)
        {
            yield return null;

            remainingTime -= Time.deltaTime;
            int newFloorTime = Mathf.FloorToInt(remainingTime);

            if (newFloorTime != floorTime)
            {//to avoid flooding the network of message, we only send a notice to client when the number of plain seconds change.
                floorTime = newFloorTime;

                for (int i = 0; i < lobbySlots.Length; ++i)
                {
                    if (lobbySlots[i] != null)
                    {//there is maxPlayer slots, so some could be == null, need to test it before accessing!
                        (lobbySlots[i] as VrLobbyPlayerListElementScript).RpcUpdateCountdown(floorTime);
                    }
                }
            }
        }

        for (int i = 0; i < lobbySlots.Length; ++i)
        {
            if (lobbySlots[i] != null)
            {
                (lobbySlots[i] as VrLobbyPlayerListElementScript).RpcUpdateCountdown(0);
            }
        }

        ServerChangeScene(playScene);
    }
    public void OnPlayersNumberModified(int count)
    {
        _playerNumber += count;

        int localPlayerCount = 0;
        foreach (UnityEngine.Networking.PlayerController p in ClientScene.localPlayers)
            localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

        //addPlayerButton.SetActive(localPlayerCount < maxPlayersPerConnection && _playerNumber < maxPlayers);
    }
}
