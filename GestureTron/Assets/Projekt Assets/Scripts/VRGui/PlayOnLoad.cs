using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;

public class PlayOnLoad : MonoBehaviour
{

    [SerializeField] private SelectionSlider ss;
    [SerializeField] private string lobbyScene;


    private void OnEnable()
    {
        ss.OnBarFilled += HandleOnBarFilled;
        
    }

    private void OnDisable()
    {
        ss.OnBarFilled -= HandleOnBarFilled;
    }

    private void HandleOnBarFilled()
    {
        SceneManager.LoadScene(lobbyScene);
    }
}
