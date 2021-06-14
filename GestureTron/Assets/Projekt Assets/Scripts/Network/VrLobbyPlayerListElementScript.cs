using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class VrLobbyPlayerListElementScript : NetworkLobbyPlayer
{
    static Color[] Colors = new Color[] { Color.magenta, Color.red, Color.cyan, Color.blue, Color.green, Color.yellow };
    static string[] NameList = new string[] {"Kevin Flynn", "Clu", "Alan Bradley", "Tron", "Lora Baines", "Yori", "Walter Gibbs", "Dumont", "Ed Dillinger", "Sark", "Roy Kleinberg", "Ram", "Crom", "Bit", "Jet Bradley", "Mercury", "Ma3a", "Thorne", "The Kernel", "Byte", "Data Wraiths", "Sam Flynn", "Quorra", "Clu 2", "Jarvis", "Edward Dillinger Jr." ,"Anon", "Abraxas", "Beck", "Tesler" };
    //used on server to avoid assigning the same color to two player

    static List<int> _colorInUse = new List<int>();
    static List<int> _nameInUse = new List<int>();

    public VRSlider colorButton;
    public GUIText nameInput;
    public GameObject vrReadyButton;
    public GameObject vrOnReadyButton;



    //OnMyName function will be invoked on clients when server change the value of playerName
    [SyncVar(hook = "OnMyName")]
    public string playerName = "";
    [SyncVar(hook = "OnMyColor")]
    public Color playerColor = Color.white;

    static Color JoinColor = new Color(255.0f / 255.0f, 0.0f, 101.0f / 255.0f, 1.0f);
    static Color NotReadyColor = new Color(34.0f / 255.0f, 44 / 255.0f, 55.0f / 255.0f, 1.0f);
    static Color ReadyColor = new Color(0.0f, 204.0f / 255.0f, 204.0f / 255.0f, 1.0f);
    static Color TransparentColor = new Color(0, 0, 0, 0);
    public Color defaultJoinButtonColor;
    public Color defaultOtherJoinButtonColor;


    public override void OnClientEnterLobby()
    {

        base.OnClientEnterLobby();

        if (VrLobbyManagerScript.s_Singleton != null)
        {
            VrLobbyManagerScript.s_Singleton.OnPlayersNumberModified(1);
            VrLobbyManagerScript.s_Singleton.ServerListTransform.gameObject.SetActive(false);
            VrLobbyManagerScript.s_Singleton.RoomPlayerListTransform.gameObject.SetActive(true);
        }
        VrLobbyPlayerListController._instance.AddPlayer(this);
        
        if (this.isLocalPlayer)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupOtherPlayer();
        }

        OnMyName(playerName);
        OnMyColor(playerColor);
    }


    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        SetupLocalPlayer();
    }

    void ChangeReadyButtonColor(Color c)
    {

    }

    void SetupOtherPlayer()
    {

        
        vrReadyButton.transform.GetComponentInChildren<Text>().text = "...";
        
        vrReadyButton.GetComponent<VRSlider>().enabled = false;
        vrReadyButton.GetComponent<EventTrigger>().enabled = false;
        vrReadyButton.GetComponentInParent<GraphicRaycaster>().enabled = false;
        
        vrReadyButton.transform.GetChild(0).GetComponent<Image>().color = new Color(98, 119, 120, 230);
        
        colorButton.GetComponent<VRSlider>().enabled = false;
        colorButton.GetComponent<EventTrigger>().enabled = false;



        OnClientReady(false);
    }

    void SetupLocalPlayer()
    {

        if (playerColor == Color.white)
            CmdColorChange();

        ChangeReadyButtonColor(JoinColor);

        vrReadyButton.transform.GetComponentInChildren<Text>().text = "JOIN";
        vrReadyButton.GetComponent<VRSlider>().enabled = true;
        vrReadyButton.GetComponent<EventTrigger>().enabled = true;
        vrReadyButton.GetComponentInParent<GraphicRaycaster>().enabled = true;
        vrReadyButton.transform.GetChild(0).GetComponent<Image>().color = new Color(21, 179, 190, 230);

        colorButton.GetComponent<VRSlider>().enabled = true;
        colorButton.GetComponent<EventTrigger>().enabled = true;


        if (playerName == "")
        {
            CmdNameChanged("Player" + (VrLobbyPlayerListController._instance.playerListContentTransform.childCount - 1));
        }

        if (VrLobbyManagerScript.s_Singleton != null) VrLobbyManagerScript.s_Singleton.OnPlayersNumberModified(0);
    }

    //This enable/disable the remove button depending on if that is the only local player or not
    public void CheckRemoveButton()
    {
        if (!isLocalPlayer)
            return;

        int localPlayerCount = 0;
        foreach (UnityEngine.Networking.PlayerController p in ClientScene.localPlayers)
            localPlayerCount += (p == null || p.playerControllerId == -1) ? 0 : 1;

        //removePlayerButton.interactable = localPlayerCount > 1;
    }

    public override void OnClientReady(bool readyState)
    {
        if (readyState)
        {
            ChangeReadyButtonColor(TransparentColor);

        }
        else
        {
            ChangeReadyButtonColor(isLocalPlayer ? JoinColor : NotReadyColor);

        }
    }

    public void OnPlayerListChanged(int idx)
    {
        
    }


    public void OnMyName(string newName)
    {
        System.Random rand = new System.Random();
        playerName = NameList[rand.Next(0,NameList.Length-1)];
        this.transform.GetChild(2).GetComponent<Text>().text = playerName;
        //nameInput.text = playerName;
    }

    public void OnMyColor(Color newColor)
    {
        playerColor = newColor;
        colorButton.transform.GetChild(0).GetComponent<Image>().color = newColor;
    }
    //Ha betöltöttt a color akkor ezt kell meghívni
    public void OnColorClicked()
    {
        CmdColorChange();
    }

    public void OnReadyClicked()
    {
        SendReadyToBeginMessage();
    }

    public void OnNameChanged(string str)
    {
        CmdNameChanged(str);
    }

    public void OnRemovePlayerClick()
    {
        if (isLocalPlayer)
        {
            RemovePlayer();
        }
        else if (isServer)
        {

        }
            //VrLobbyManagerScript.s_Singleton.KickPlayer(connectionToClient);

    }

    public void ToggleJoinButton(bool enabled)
    {
        //readyButton.gameObject.SetActive(enabled);
        //waitingPlayerButton.gameObject.SetActive(!enabled);
    }

    [ClientRpc]
    public void RpcUpdateCountdown(int countdown)
    {
        VrLobbyManagerScript.s_Singleton.countdownPanel.UIText.text = "Match Starting in " + countdown;
        VrLobbyManagerScript.s_Singleton.countdownPanel.gameObject.SetActive(countdown != 0);
    }

    [ClientRpc]
    public void RpcUpdateRemoveButton()
    {
        CheckRemoveButton();
    }

    //====== Server Command

    [Command]
    public void CmdColorChange()
    {
        int idx = System.Array.IndexOf(Colors, playerColor);

        int inUseIdx = _colorInUse.IndexOf(idx);

        if (idx < 0) idx = 0;

        idx = (idx + 1) % Colors.Length;

        bool alreadyInUse = false;

        do
        {
            alreadyInUse = false;
            for (int i = 0; i < _colorInUse.Count; ++i)
            {
                if (_colorInUse[i] == idx)
                {//that color is already in use
                    alreadyInUse = true;
                    idx = (idx + 1) % Colors.Length;
                }
            }
        }
        while (alreadyInUse);

        if (inUseIdx >= 0)
        {//if we already add an entry in the colorTabs, we change it
            _colorInUse[inUseIdx] = idx;
        }
        else
        {//else we add it
            _colorInUse.Add(idx);
        }

        playerColor = Colors[idx];
    }

    [Command]
    public void CmdNameChanged(string name)
    {
        playerName = name;
    }

    //Cleanup thing when get destroy (which happen when client kick or disconnect)
    public void OnDestroy()
    {
        VrLobbyPlayerListController._instance.RemovePlayer(this);
        if (VrLobbyManagerScript.s_Singleton != null) VrLobbyManagerScript.s_Singleton.OnPlayersNumberModified(-1);

        int idx = System.Array.IndexOf(Colors, playerColor);

        if (idx < 0)
            return;

        for (int i = 0; i < _colorInUse.Count; ++i)
        {
            if (_colorInUse[i] == idx)
            {//that color is already in use
                _colorInUse.RemoveAt(i);
                break;
            }
        }
    }
}
