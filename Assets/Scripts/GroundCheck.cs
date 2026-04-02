using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerController playerController;
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == playerController.gameObject)
            return;

        playerController.SetGrounded(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == playerController.gameObject)
            return;

        playerController.SetGrounded(false);
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject == playerController.gameObject)
            return;

        playerController.SetGrounded(true);
    }
}
