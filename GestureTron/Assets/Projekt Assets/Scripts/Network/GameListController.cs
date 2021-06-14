using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameListController : MonoBehaviour
{
    public VrLobbyManagerScript VLM;
    public RectTransform serverListContentTransform;
    public GameObject serverListPrefab;

    protected VerticalLayoutGroup _layout;
    // Start is called before the first frame update
    void Start()
    {
        _layout = serverListContentTransform.GetComponent<VerticalLayoutGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (_layout)
            //_layout.childAlignment = Time.frameCount % 2 == 0 ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
    }

    public void AddServer(LanConnectionInfo info)
    {
        GameObject obj = Instantiate(serverListPrefab);
        obj.GetComponent<VrLanServer>().serverName.text = info.ipAddress;
        obj.GetComponent<VrLanServer>().lanInfo = info;
        obj.transform.SetParent(serverListContentTransform, false);
    }
}
