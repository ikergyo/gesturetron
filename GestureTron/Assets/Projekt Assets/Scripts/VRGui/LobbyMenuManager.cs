using System;
using System.Collections;
using System.Collections.Generic;
using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;

enum LobbyStatus
{
    None,
    ServerList,
    Host,
}

public class LobbyMenuManager : MonoBehaviour
{
    [SerializeField] private SelectionSlider selectionJoin;
    [SerializeField] private SelectionSlider selectionHost;

    public RectTransform VrMainMenuPanel;
    public RectTransform VrHostMenuPanel;
    public RectTransform VrServerListPanel;

    private LobbyStatus ls = LobbyStatus.None;

    public VrLobbyManager lobbyManager;

    private void OnEnable()
    {
        selectionHost.OnBarFilled += HandleOnBarFilledHost;
        selectionJoin.OnBarFilled += HandleOnBarFilledJoin;
    }

    private void OnDisable()
    {
        selectionHost.OnBarFilled -= HandleOnBarFilledHost;
        selectionJoin.OnBarFilled -= HandleOnBarFilledJoin;
    }

    private void HandleOnBarFilledJoin()
    {
        if (ls == LobbyStatus.None)
        {
            VrServerListPanel.gameObject.SetActive(true);
            ls = LobbyStatus.ServerList;

        }
        /*lobbyManager.networkAddress = "127.0.0.1";
        lobbyManager.StartClient();
        if (ls == LobbyStatus.None)
        {
            VrMainMenuPanel.gameObject.SetActive(true);
            ls = LobbyStatus.Host;

        }
        VrHostMenuPanel.gameObject.SetActive(true);
        */
        //lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
        //SceneManager.LoadScene("lobbyMenuScene");
    }
    private void HandleOnBarFilledHost()
    {
        lobbyManager.StartHost();
        if (ls == LobbyStatus.None)
        {
            VrMainMenuPanel.gameObject.SetActive(true);
            ls = LobbyStatus.Host;
            
        }
        VrHostMenuPanel.gameObject.SetActive(true);
        //VrJoinMenuPanel.gameObject.SetActive(false);
        
        
    }
}
