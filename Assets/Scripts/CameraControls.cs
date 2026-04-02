using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private PlayerCamera _playerCamera;
    [SerializeField] private Transform _cameraFollowPoint;
    private Controlls inputActions;

    private Vector2 _lookInput;
    private float _zoomInput;

    private void Awake()
    {
        inputActions = new Controlls();
    }

     private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => _lookInput = Vector2.zero;

        inputActions.Player.Zoom.performed += ctx => _zoomInput = -ctx.ReadValue<float>();
        inputActions.Player.Zoom.canceled += ctx => _zoomInput = 0f;
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        _playerCamera.SetFollowTransform(_cameraFollowPoint);
    }

    private void LateUpdate()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        float zoomInput = -inputActions.Player.Zoom.ReadValue<float>();

        Vector3 lookVector = new Vector3(_lookInput.x, _lookInput.y, 0f);

        _playerCamera.UpdateWithInput(Time.deltaTime, _zoomInput, lookVector);
    }
}
