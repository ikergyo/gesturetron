using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionCheck : MonoBehaviour
{
    public SetupLocalPlayer networkObject;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            Debug.Log("Collide with the wall, idiot");
            networkObject.Cmd_DestroyThis(this.gameObject);
        }

    }

}
