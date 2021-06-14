using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VrLanServer : MonoBehaviour
{

    public Text serverName;
    public LanConnectionInfo lanInfo;

    // Update is called once per frame
    void Update()
    {
        
    }

    //Amikor megszűnik a szerver.
    public void OnDestroy()
    {
        VrServerList._instance.RemoveServer(this);
    }
}
