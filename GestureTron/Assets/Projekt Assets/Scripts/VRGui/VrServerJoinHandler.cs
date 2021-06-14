using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Utils;


public class VrServerJoinHandler : MonoBehaviour
{

    VRSlider vrs;
    public VrLanServer vls;

    private void Awake()
    {
        vrs = GetComponent<VRSlider>();
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        vrs.OnBarFilled += this.OnBarFilled;
    }

    void OnDisable()
    {
        vrs.OnBarFilled -= this.OnBarFilled;
    }

    private void OnBarFilled()
    {
        VrLobbyManagerScript.s_Singleton.networkAddress = vls.lanInfo.ipAddress;
        VrLobbyManagerScript.s_Singleton.StartClient();

    }
}
