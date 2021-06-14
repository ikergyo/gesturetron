using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VrColorChanger : MonoBehaviour
{

    VRSlider vrs;
    public VrLobbyPlayerListElementScript VRLPLE;

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
        VRLPLE.CmdColorChange();
    }
}
