using UnityEngine;
using Prototype.NetworkLobby;
using System.Collections;
using UnityEngine.Networking;

    public class VrNWHook : LobbyHook
    {

        public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
        {
            VrLobbyPlayerListElementScript lobby = lobbyPlayer.GetComponent<VrLobbyPlayerListElementScript>();
            SetupLocalPlayer localPlayer = gamePlayer.GetComponent<SetupLocalPlayer>();

            localPlayer.pname = lobby.playerName;
            localPlayer.playerColor = lobby.playerColor;
        }
    }

