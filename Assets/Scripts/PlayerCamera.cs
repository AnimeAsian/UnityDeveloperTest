using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Distance Settings")]
    [SerializeField] private float _defaultDistance = 6f,
        _minDistance = 3f,
        _maxDistance = 10f,
        _distanceMovenentSpeed = 5f,
        _distanceMovementSharpness = 10f;

        [Header("Rotation Settings")]
        [SerializeField] private float _rotationSpeed = 10f,
        _rotationSharpness = 10000f;
        [Header("Follow Settings")]
        [SerializeField] private float _followSharpness = 10000f;
        [Header("Vertical Angle Limits")]
        [SerializeField] private float _minVerticalAngle = -90f,
        _maxVerticalAngle = 90f,
        _defaultVerticalAngle = 20f;

    private Transform _followTransform;
    private Vector3 _currentFollowPosition, _planarDirection;
    private float _targetVerticalAngle;
    
    private float _currentDistance, _targetDistance;

    private bool _lockCamera = false;

    private void Awake()
    {
        _currentDistance = _defaultDistance;
        _targetDistance = _currentDistance;
        _planarDirection = Vector3.forward;
    }

    private void OnValidate()
    {
        _defaultDistance = Mathf.Clamp(_defaultDistance, _minDistance, _maxDistance);
        _defaultVerticalAngle = Mathf.Clamp(_defaultVerticalAngle, _minVerticalAngle, _maxVerticalAngle);
    }

    public void SetFollowTransform(Transform t)
    {
        _followTransform = t;
        if (_followTransform == null) return;

        _currentFollowPosition = t.position;
        _planarDirection = t.forward;
    }

    public void SetCameraLock(bool state)
    {
        _lockCamera = state;
    }

    public void ResetPlanarDirection()
    {
        if (!_followTransform) return;

        _planarDirection = Vector3.ProjectOnPlane(_followTransform.forward, _followTransform.up).normalized;
    }

    public void ResetVerticalAngle()
    {
        _targetVerticalAngle = 0f;
    }

    public void UpdateWithInput(float deltaTime, float zoomInput, Vector3 rotationInput)
    {
        if (!_followTransform) return;
        if (!_lockCamera)
        {
            HandleRotationInput(deltaTime, rotationInput, out Quaternion targetRotation);
            HandlePosition(deltaTime, zoomInput, targetRotation);
        }
        else
        {
            // Follow position only, no rotation update
            _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, _followTransform.position, 1f - Mathf.Exp(-_followSharpness * deltaTime));

            Vector3 targetPosition = _currentFollowPosition - ((transform.rotation * Vector3.forward) * _currentDistance);

            transform.position = targetPosition;
        }     
    }

    private void HandleRotationInput(float deltaTime, Vector3 rotationInput, out Quaternion targetRotation)
    {
        Quaternion rotationFromInput = Quaternion.AngleAxis(rotationInput.x * _rotationSpeed, transform.up);
        _planarDirection = rotationFromInput * _planarDirection;

        _targetVerticalAngle -= (rotationInput.y * _rotationSpeed);
        _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, _minVerticalAngle, _maxVerticalAngle);

        Vector3 up = _followTransform.up;
        Vector3 right = Vector3.Cross(up, _planarDirection).normalized;

        Quaternion pitchRot = Quaternion.AngleAxis(_targetVerticalAngle, right);
        Quaternion planarRot = Quaternion.LookRotation(_planarDirection, up);

        // Combine rotations
        Quaternion finalRotation = pitchRot * planarRot;

        targetRotation = Quaternion.Slerp(transform.rotation, finalRotation, _rotationSharpness * deltaTime);

        transform.rotation = targetRotation;
    }

    private void HandlePosition(float deltaTime, float zoomInput, Quaternion targetRotation)
    {
        _targetDistance += zoomInput * _distanceMovenentSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);

        _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, _followTransform.position, 1f - Mathf.Exp(-_followSharpness * deltaTime));
        Vector3 targetPosition = _currentFollowPosition - ((targetRotation * Vector3.forward) * _currentDistance);

        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, 1 - Mathf.Exp(-_distanceMovementSharpness * deltaTime));
        transform.position = targetPosition;
    }
}
