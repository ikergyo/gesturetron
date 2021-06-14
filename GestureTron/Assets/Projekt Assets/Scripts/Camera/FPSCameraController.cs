using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCameraController : MonoBehaviour {

    private float rotationY = 0f;
    private float sensitivityY = 2f;

    private float rotationX = 0f;
    private float sensitivityX = 2f;

    public Camera playerCamera;

    private void Start()
    {
        playerCamera.enabled = true;
    }

    private void LateUpdate()
    {

        rotationY += Input.GetAxis("Mouse X") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        rotationX += Input.GetAxis("Mouse Y") * sensitivityX;
        rotationX = Mathf.Clamp(rotationX, -90, 90);


        transform.localEulerAngles = new Vector3(-rotationX, rotationY, transform.localEulerAngles.z);

        playerCamera.transform.position = this.transform.position;
        playerCamera.transform.rotation = transform.rotation;
       
    }
    
}
