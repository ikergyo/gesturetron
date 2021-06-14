using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuLoadScript : MonoBehaviour {

    public Button startButton;
    public Button optionsButton;
    public Button exitButton;
    public Canvas quitMenu;
    public Canvas optionsMenu;
    public Button muteButton;
    public Button soundButton;


    // Use this for initialization
    void Start () {
        startButton = startButton.GetComponent<Button>();
        optionsButton = optionsButton.GetComponent<Button>();
        exitButton = exitButton.GetComponent<Button>();
        optionsMenu = optionsMenu.GetComponent<Canvas>();
        quitMenu = quitMenu.GetComponent<Canvas>();
        optionsMenu.enabled = false;
        quitMenu.enabled = false;

        muteButton = muteButton.GetComponent<Button>();
        soundButton = soundButton.GetComponent<Button>();
        HideMuteButton();
    }

    public void StartLevel()
    {
        SceneManager.LoadScene("lobbyMenuScene");
    }

    public void Options() {
        optionsMenu.enabled = true;
        startButton.enabled = false;
        optionsButton.enabled = false;
        exitButton.enabled = false;
    }

    public void OptionsSubmit() {
        optionsMenu.enabled = false;
        startButton.enabled = true;
        optionsButton.enabled = true;
        exitButton.enabled = true;
        //mute
    }

    public void Mute() {
        AudioListener.volume = 1- AudioListener.volume;
        HideMuteButton();
    }

    private void HideMuteButton() {
        if (AudioListener.volume == 1)
        {
            soundButton.enabled = false;
            soundButton.transform.localScale = new Vector3(0, 0, 0);

            muteButton.enabled = true;
            muteButton.transform.localScale = new Vector3(1, 1, 1);
        }
        else {
            muteButton.enabled = true;
            muteButton.transform.localScale = new Vector3(0, 0, 0);

            soundButton.enabled = true;
            soundButton.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void QuitGame() {
        quitMenu.enabled = true;
        startButton.enabled = false;
        optionsButton.enabled = false;
        exitButton.enabled = false;
    }

    public void NoPress() {
        quitMenu.enabled = false;
        startButton.enabled = true;
        optionsButton.enabled = true;
        exitButton.enabled = true;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
