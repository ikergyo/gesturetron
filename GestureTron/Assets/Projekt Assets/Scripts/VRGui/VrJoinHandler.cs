using System.Collections;
using System.Collections.Generic;
using Prototype.NetworkLobby;
using UnityEngine;
using VRStandardAssets.Utils;

public class VrJoinHandler : MonoBehaviour
{
    public VrLobbyPlayer vlp;

    [SerializeField] private SelectionSlider sl;

    private void OnEnable()
    {
        sl.OnBarFilled += HandleJoinOnBarFilled;
        
    }

    private void OnDisable()
    {
        sl.OnBarFilled -= HandleJoinOnBarFilled;
        
    }

    private void HandleJoinOnBarFilled()
    {
        if (vlp == null)
        {
            return;
        }
        vlp.vrOnReadyButton.gameObject.SetActive(true);
        vlp.vrReadyButton.gameObject.SetActive(false);
        vlp.OnReadyClicked();

    }
}
