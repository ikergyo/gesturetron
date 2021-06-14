using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VrServerList : MonoBehaviour
{
    public static VrServerList _instance = null;

    public RectTransform serverListContentTransform;

    protected VerticalLayoutGroup _layout;
    protected List<VrLanServer> serverList = new List<VrLanServer>();
    public void OnEnable()
    {
        _instance = this;
        _layout = serverListContentTransform.GetComponent<VerticalLayoutGroup>();
    }

    public void AddServer(VrLanServer newServer)
    {
        serverList.Add(newServer);
        newServer.transform.SetParent(serverListContentTransform, false);
    }

    public void RemoveServer(VrLanServer server)
    {
        serverList.Remove(server);
        
    }

}
