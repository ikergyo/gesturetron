using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VrLobbyPlayerListController : MonoBehaviour
{
    public static VrLobbyPlayerListController _instance = null;

    public RectTransform playerListContentTransform;

    protected VerticalLayoutGroup _layout;
    protected List<VrLobbyPlayerListElementScript> _players = new List<VrLobbyPlayerListElementScript>();

    public void OnEnable()
    {
        _instance = this;
        _layout = playerListContentTransform.GetComponent<VerticalLayoutGroup>();
    }

    void Update()
    {

        /*if (_layout)
        {
            _layout.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;

        }*/
            
    }

    public void AddPlayer(VrLobbyPlayerListElementScript player)
    {
        if (_players.Contains(player))
            return;

        _players.Add(player);
        player.transform.SetParent(playerListContentTransform,false);
        player.transform.localPosition = new Vector3(player.transform.localPosition.x, player.transform.localPosition.y, 0);

        PlayerListModified();
    }

    public void RemovePlayer(VrLobbyPlayerListElementScript player)
    {
        _players.Remove(player);
        PlayerListModified();
    }

    public void PlayerListModified()
    {
        int i = 0;
        foreach (VrLobbyPlayerListElementScript p in _players)
        {
            p.OnPlayerListChanged(i);
            ++i;
        }
    }
}
