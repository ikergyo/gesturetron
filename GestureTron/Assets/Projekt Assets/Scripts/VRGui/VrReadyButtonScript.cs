using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VrReadyButtonScript : MonoBehaviour
{
    VRSlider vrs;
    public Slider colorButton;
    public VrLobbyPlayerListElementScript thisPlayer;

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
        this.transform.GetComponentInChildren<Text>().text = "READY";
        vrs.enabled = false;
        GetComponent<EventTrigger>().enabled = false;
        colorButton.GetComponent<VRSlider>().enabled = false;
        colorButton.GetComponent<EventTrigger>().enabled = false;
        thisPlayer.OnReadyClicked();
    }
}
