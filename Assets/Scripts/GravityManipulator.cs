using UnityEngine;
using UnityEngine.InputSystem;

public class GravityManipulator : MonoBehaviour
{
    public enum GravityDir { Forward, Backward, Left, Right }

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerCamera playerCamera;

    [Header("Gravity")]
    [SerializeField] private float gravityForce = 20f;

    [Header("Rotation")]
    [SerializeField] private float rotationDuration = 0.25f;

    [Header("Hologram")]
    [SerializeField] private GameObject hologramInstance;
    [SerializeField] private Transform player;
    [SerializeField] private float hologramOffset = 1.5f;

    private bool isRotating = false;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private float rotationTime = 0f;

    private GravityDir selectedGravity;
    private bool previewActive;

    private Vector3 currentGravityVector = Vector3.down;

    public Vector3 CurrentGravity => currentGravityVector;
    public bool IsRotating => isRotating;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        currentGravityVector = Vector3.down;
    }

    // ---------------- INPUT ----------------

    public void OnGravitySelect(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 input = context.ReadValue<Vector2>();

        if (input.y > 0.5f) selectedGravity = GravityDir.Forward;
        else if (input.y < -0.5f) selectedGravity = GravityDir.Backward;
        else if (input.x < -0.5f) selectedGravity = GravityDir.Left;
        else if (input.x > 0.5f) selectedGravity = GravityDir.Right;

        previewActive = true;

        Vector3 previewDir = GetPreviewGravity(selectedGravity);
        ShowHologram(previewDir);
    }

    public void OnGravityApply(InputAction.CallbackContext context)
    {
        if (!context.performed || !previewActive || isRotating) return;

        StartRotation(selectedGravity);
        previewActive = false;

        HideHologram();

        if (playerCamera)
            playerCamera.SetCameraLock(true);
    }

    // ---------------- ROTATION ----------------

    private void StartRotation(GravityDir dir)
    {
        Quaternion rotation = Quaternion.identity;

        switch (dir)
        {
            case GravityDir.Forward:
                rotation = Quaternion.AngleAxis(-90f, transform.right);
                break;

            case GravityDir.Backward:
                rotation = Quaternion.AngleAxis(90f, transform.right);
                break;

            case GravityDir.Left:
                rotation = Quaternion.AngleAxis(-90f, transform.forward);
                break;

            case GravityDir.Right:
                rotation = Quaternion.AngleAxis(90f, transform.forward);
                break;
        }

        startRotation = transform.rotation;
        targetRotation = rotation * transform.rotation;

        rotationTime = 0f;
        isRotating = true;
    }

    private Vector3 GetPreviewGravity(GravityDir dir)
    {
        switch (dir)
        {
            case GravityDir.Forward:
                return transform.forward;

            case GravityDir.Backward:
                return -transform.forward;

            case GravityDir.Left:
                return -transform.right;

            case GravityDir.Right:
                return transform.right;

            default:
                return -transform.up;
        }
    }

    private void UpdateRotation()
    {
        if (!isRotating) return;

        rotationTime += Time.deltaTime;
        float t = rotationTime / rotationDuration;

        transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

        if (t >= 1f)
        {
            isRotating = false;

            SnapToAxis();

            currentGravityVector = -transform.up;

            if (playerCamera)
            {
                playerCamera.SetCameraLock(false);
                playerCamera.ResetPlanarDirection();
                playerCamera.ResetVerticalAngle();
            }
        }
    }

    private void ShowHologram(Vector3 gravityDir)
    {
        if (hologramInstance == null) return;

        // Position
        hologramInstance.transform.position =
            player.transform.position + gravityDir * hologramOffset;

        // 🔥 Correct up direction (opposite of gravity)
        Vector3 up = -gravityDir;

        // 🔥 Keep player forward projected onto new surface
        Vector3 forward = Vector3.ProjectOnPlane(player.transform.forward, up);

        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.ProjectOnPlane(player.transform.up, up);

        forward.Normalize();

        // 🔥 Stable rotation
        hologramInstance.transform.rotation = Quaternion.LookRotation(forward, up);

        hologramInstance.SetActive(true);
    }

    private void HideHologram()
    {
        if (hologramInstance != null)
            hologramInstance.SetActive(false);
    }

    private void SnapToAxis()
    {
        Vector3 euler = transform.eulerAngles;

        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;

        transform.eulerAngles = euler;
    }

    // ---------------- UPDATE ----------------

    private void FixedUpdate()
    {
        rb.AddForce(currentGravityVector * gravityForce, ForceMode.Acceleration);
    }

    private void Update()
    {
        UpdateRotation();
    }
}