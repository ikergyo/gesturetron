using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinScript : MonoBehaviour
{
    VRSlider vrs;
    public GameObject needToActivate;
    public VrLobbyManagerScript vrls;

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
        if (!needToActivate.active)
            needToActivate.SetActive(true);
        
    }
}
