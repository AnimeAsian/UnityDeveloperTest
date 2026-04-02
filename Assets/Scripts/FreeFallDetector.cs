using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects if the player has been falling freely (no ground contact) for too long.
/// If the player is airborne beyond the threshold, it triggers a game-over.
/// 
/// Attach this to the Player GameObject alongside PlayerController.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class FreeFallDetector : MonoBehaviour
{
    [SerializeField] private float freeFallTimeout = 5f;

    [SerializeField] private float fallSpeedThreshold = 3f;

    [SerializeField] private GravityManipulator gravity;

    private PlayerController playerController;
    private Rigidbody         rb;
    private float             airborneTimer;
    private bool              isDead;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb               = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isDead) return;

        bool grounded = playerController.grounded;

        if (grounded)
        {
            airborneTimer = 0f;
            return;
        }

        Vector3 gravityDir = gravity.CurrentGravity.normalized;
        float   fallComponent = Vector3.Dot(rb.velocity, gravityDir); 

        if (fallComponent > fallSpeedThreshold)
        {
            airborneTimer += Time.deltaTime;

            if (airborneTimer >= freeFallTimeout)
            {
                isDead = true;
                GameManager.Instance.GameOver("Fell off!");
            }
        }
        else
        {
            airborneTimer = 0f;
        }
    }
}
