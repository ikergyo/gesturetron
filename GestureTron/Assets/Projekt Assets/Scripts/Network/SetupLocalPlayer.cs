using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Prototype.NetworkLobby;

public enum MoveMode
{
    Arcade,
    Simulator
}
public enum CameraMode
{
    Tps,
    FpsWithMouse,
    Vr
}

public enum InputModeForSteering
{
    Keyboard,
    GestureGlove,
    VrHeadset
}
public class SetupLocalPlayer : NetworkBehaviour
{
    public GameObject explosionEffect;

    public string winMessage = "You win!";
    public string looseMessage = "You loose :( !";

    float startingTime = 0f;
    public float countDownTime = 5f;

    static List<GameObject> playerList = new List<GameObject>();
    static int reachedPlayers = 0;

    public Canvas countDownCanvas;

    public CameraMode cameraMode = CameraMode.Vr;
    public MoveMode moveMode = MoveMode.Simulator;
    public InputModeForSteering inputModeForSteering = InputModeForSteering.Keyboard;

    public bool gameHasBeenStarted = false;
    public bool gameHasBeenOver = false;

    [SyncVar]
    public string pname = "player";

    [SyncVar]
    public Color playerColor = Color.red;


    [SerializeField]
    GameObject trailObject;

    [SerializeField]
    GameObject cameraObject;

    //private MotorController m_Car;
    private MotorController m_Car;

    private SteeringController sc;

    void Start () {
        if (isServer)
        {
            startingTime = countDownTime;
        }
        if (isLocalPlayer)
        {
            countDownCanvas.gameObject.SetActive(true);
            if (cameraMode == CameraMode.FpsWithMouse)
            {
                cameraObject.GetComponent<FPSCameraController>().enabled = true;
            }
            else if(cameraMode == CameraMode.Vr)
            {
                cameraObject.GetComponent<Camera>().enabled = true;
            }
            if (inputModeForSteering == InputModeForSteering.GestureGlove)
            {
                gameObject.GetComponent<SteeringController>().enabled=true;
                sc = gameObject.GetComponent<SteeringController>();
            }
            Cmd_AddPlayer(this.transform.gameObject);
        }
        
		Renderer[] rends = GetComponentsInChildren<Renderer>();
		foreach (Renderer r in rends)
		{
			r.material.color = playerColor;
		}
        trailObject.GetComponent<TronTrail>().trailColor = playerColor;
    }
    private void Update()
    {
        if (isServer && !gameHasBeenStarted && !gameHasBeenOver)
        {
            startingTime -= 1 * Time.deltaTime;
            if (startingTime >= 0)
                Rpc_CountDown(startingTime);
            else
                Rpc_gameHasBeenStarted();

            return;
        }
    }

    void FixedUpdate() {

        if (!gameHasBeenStarted)
            return;


        if (isServer)
        {
            if (playerList.Count <= 1 && reachedPlayers != 1)
            {
                Rpc_ScoreCanvasSetup();
                if (playerList.Count == 1)
                {
                    gameHasBeenStarted = false;
                    gameHasBeenOver = true;
                    Rpc_WinnerSetup();
                    Rpc_SetupOverStart();
                }
                Rpc_StartOverCountdown();
            }else if(reachedPlayers == 1)
            { 
                if (playerList.Count < 1)
                {
                    gameHasBeenStarted = false;
                    gameHasBeenOver = true;
                    Rpc_SetupOverStart();
                    Rpc_ScoreCanvasSetup();
                    Rpc_StartOverCountdown();
                }
            }
        }

        if (gameHasBeenOver)
            return;

        this.GetComponentInChildren<TextMesh> ().text = pname;
		
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");

        if (inputModeForSteering == InputModeForSteering.VrHeadset)
        {
            float cameraRotationZ = Camera.main.transform.eulerAngles.z;
            h = Mathf.Clamp(cameraRotationZ, -1,1);
        }else if(inputModeForSteering == InputModeForSteering.GestureGlove)
        {
            h = Mathf.Clamp(sc.ActualPressure, -1, 1);
            v = Mathf.Clamp(sc.GetFootActualPressure(),0,1);
        }

#if !MOBILE_INPUT
        if (isLocalPlayer)
        {
            float handbrake = CrossPlatformInputManager.GetAxis("Jump");
            m_Car.Move(h, v, v, handbrake);
        }
#else
            m_Car.Move(h, v, v, 0f);
#endif
        
    }
    [Command]
    public void Cmd_DestroyThis(GameObject destroyedObject)
    {
        playerList.Remove(destroyedObject);
        Debug.Log("playerList.Count");
        Rpc_DestroyObject(destroyedObject);

    }

    [Command]
    public void Cmd_AddPlayer(GameObject newPlayer)
    {
        if (isServer)
        {
            playerList.Add(newPlayer);
            reachedPlayers++;
            string name = newPlayer.GetComponent<SetupLocalPlayer>().pname;
        }
    }
    [ClientRpc]
    public void Rpc_CountDown(float time)
    {
        if(isLocalPlayer)
            countDownCanvas.transform.GetChild(0).GetComponent<Text>().text = time.ToString("0");
    }
    [ClientRpc]
    public void Rpc_gameHasBeenStarted()
    {
        //Csak a serveren fut le ez is
        /*string name = this.gameObject.GetComponent<SetupLocalPlayer>().pname;
        Debug.Log(name + ": " + this.gameObject.GetComponent<NetworkIdentity>().netId);*/
        
        countDownCanvas.gameObject.SetActive(false);
        gameHasBeenStarted = true;

        if (moveMode == MoveMode.Simulator)
        {
            GetComponent<SimulatorMotorController>().enabled = true;
            m_Car = GetComponent<SimulatorMotorController>();
        }
        else if (moveMode == MoveMode.Arcade)
        {
            GetComponent<ArcadeMotorController>().enabled = true;
            m_Car = GetComponent<ArcadeMotorController>();
        }
        
    }
    [ClientRpc]
    public void Rpc_ScoreCanvasSetup()
    {
        
        string name = this.gameObject.GetComponent<SetupLocalPlayer>().pname;
        Debug.Log("LoserSetup: " + name + ": " + this.gameObject.GetComponent<NetworkIdentity>().netId);
        Camera.main.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = looseMessage;
        Camera.main.transform.GetChild(0).gameObject.SetActive(true);
    }

    [ClientRpc]
    public void Rpc_WinnerSetup()
    {

        string name = this.gameObject.GetComponent<SetupLocalPlayer>().pname;
        Debug.Log("WinnerSetup: " + name + ": " + this.gameObject.GetComponent<NetworkIdentity>().netId);

        if (this.transform.childCount > 0)
        {
            this.transform.GetChild(7).GetChild(0).GetComponent<Text>().text = winMessage;
            this.transform.GetChild(7).gameObject.SetActive(true);
        }
        /*try
        {
            m_Car.Init();
        }
        catch (Exception e)
        {
            Debug.Log(m_Car);
            Debug.Log(this.GetComponent<NetworkIdentity>().netId);
        }*/
        if (m_Car != null)
        {
            m_Car.Init();
        }
        GetComponent<ArcadeMotorController>().enabled = false;
        GetComponent<SimulatorMotorController>().enabled = false;
        this.m_Car = null;
        gameHasBeenStarted = false;
        gameHasBeenOver = true;
        //Debug.Log("Végigment: " + gameObject.GetComponent<NetworkIdentity>().netId);


    }
    [ClientRpc]
    public void Rpc_DestroyObject(GameObject destroyedObject)
    {
        Instantiate(explosionEffect, destroyedObject.transform.position, destroyedObject.transform.rotation);
        //destroyedObject.GetComponent<SetupLocalPlayer>().gameHasBeenStarted = false;
        //destroyedObject.GetComponent<SetupLocalPlayer>().gameHasBeenOver = true;
        foreach (Transform child in destroyedObject.transform)
        {
            GameObject.Destroy(child.gameObject);

        }

    }
    [ClientRpc]
    public void Rpc_StartOverCountdown()
    {
        StartCoroutine(OverCountdown());
    }
    [ClientRpc]
    public void Rpc_SetupOverStart()
    {
        gameHasBeenStarted = false;
        gameHasBeenOver = true;
    }
    private IEnumerator OverCountdown()
    {
        float duration = countDownTime; // 3 seconds you can change this to
        float totalTime = 0;
        while (totalTime <= duration)
        {

            totalTime += Time.deltaTime;
            yield return null;
        }

        if (VrLobbyManagerScript.s_Singleton != null)
        {
            VrLobbyManagerScript.s_Singleton.SendReturnToLobby();
            /*DestroyObject(VrLobbyManagerScript.s_Singleton.gameObject);
            string sceneName = VrLobbyManagerScript.s_Singleton.lobbyScene;
            SceneManager.LoadScene(sceneName);*/
        }
        else if(LobbyManager.s_Singleton != null)
        {

            LobbyManager.s_Singleton.SendReturnToLobby();            
            /*DestroyObject(LobbyManager.s_Singleton.gameObject);
            string sceneName = LobbyManager.s_Singleton.lobbyScene;
            SceneManager.LoadScene(sceneName);*/
        }
        
    }
    
}
