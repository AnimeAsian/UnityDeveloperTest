using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody rb;
    public float speed, sensitivity, maxForce, jumpForce;

    [Header("State")]
    public bool grounded;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float runThreshold = 0.1f;
    private bool wasGrounded;

    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private GravityManipulator gravity;

     [Header("Gravity")]
     [SerializeField] float extraGravity = 20f;

     private Vector2 moveInput;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (playerCamera)
        {
            playerCamera.SetFollowTransform(transform);
        }
    }

    void FixedUpdate()
    {
        Move();
        wasGrounded = grounded;
        
        if (!grounded)
        {
            rb.AddForce(gravity.CurrentGravity * extraGravity, ForceMode.Acceleration);
        }
    }

    void Update()
    {
        UpdateAnimator();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
       if (context.performed)
        Jump();
    }

    void Move()
    {
        Vector3 gravityDir = gravity.CurrentGravity;
        Vector3 upAxis = -gravityDir;

        Vector3 currentVelocity = rb.velocity;

        Vector3 velocityOnPlane = Vector3.ProjectOnPlane(currentVelocity, gravityDir);

        Debug.Log("MoveInput: " + moveInput);

        // STOP logic
        if (moveInput.sqrMagnitude < 0.01f)
        {
            Vector3 newVelocity = Vector3.Lerp(velocityOnPlane, Vector3.zero, 10f * Time.fixedDeltaTime);

            rb.velocity = newVelocity + Vector3.Project(currentVelocity, gravityDir);
            return;
        }

        // Camera relative movement
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, gravityDir);

        if (camForward.sqrMagnitude < 0.01f)
        {
            camForward = Vector3.ProjectOnPlane(cameraTransform.up, gravityDir);
        }

        camForward.Normalize();
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, gravityDir).normalized;

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        Vector3 targetVelocity = moveDirection * speed;

        Vector3 velocityChange = targetVelocity - velocityOnPlane;
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        Debug.Log("MoveDir: " + moveDirection);

        RotatePlayer(moveDirection, upAxis);
    }

    void Jump()
    {
        if (!grounded) return;
        Vector3 jumpDir = -gravity.CurrentGravity;

        rb.AddForce(jumpDir * jumpForce, ForceMode.VelocityChange);
        grounded = false;
    }

    void RotatePlayer(Vector3 moveDirection, Vector3 upAxis)
    {
        if (moveDirection.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, upAxis);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        if (!animator) return;

        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed < 0.1f) speed = 0f;

        Debug.Log("Velocity Speed: " + speed);

        animator.SetFloat("Speed", speed);

        animator.SetBool("Grounded", grounded);
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }

}
