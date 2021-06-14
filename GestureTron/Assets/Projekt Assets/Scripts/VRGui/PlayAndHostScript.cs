using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAndHostScript : MonoBehaviour
{
    VRSlider vrs;
    public GameObject needToActivate;
    public VrLobbyManagerScript vrls;

    // Start is called before the first frame update
    void OnEnable()
    {
        vrs = GetComponent<VRSlider>();
        vrs.OnBarFilled += this.OnBarFilled;
    }

    void OnDisable()
    {
        vrs = GetComponent<VRSlider>();
        vrs.OnBarFilled -= this.OnBarFilled;
    }

    private void OnBarFilled()
    {
        if(!needToActivate.active)
            needToActivate.SetActive(true);
        vrls.StartHost();
    }
}
