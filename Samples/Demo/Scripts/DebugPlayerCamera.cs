using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugPlayerCamera : MonoBehaviour
{

    private float yRotation;
    private float xRotation;

    // [SerializeField] private Transform cameraHolder;
    // [SerializeField] private Transform playerCameraPosition;
    
    // [SerializeField] private Transform pivot;
    
    [SerializeField] private float sensX = 1;
    [SerializeField] private float sensY = 1;
    
    // [SerializeField]
    // private Transform orientation;

    private Vector3 startingPosition;

    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    [SerializeField] private bool flipOffsetX;


    [SerializeField] private bool collision = false;

    private float defaultZ;

    [SerializeField] private LayerMask collisionLayerMask;

    [SerializeField] private Transform followTarget;
    [SerializeField] private Transform lookAtTarget;
    
    private void MoveCamera(Vector3 position)
    {
        transform.position = position;
    }

    private void RotateCamera(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
    
    public void SetMouseVisibleAndLocked(bool b)
    {
        Cursor.lockState = b ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !b;
    }

    private Quaternion targetCameraRotation;
    private Vector3 targetCameraHolderPosition;

    private void Update()
    {
        var offset = Vector3.zero;
        
        if (followTarget)
        {
            var distance = 4;
            var pdirection = transform.position - followTarget.position;
            pdirection = pdirection.normalized;
            
            offset = pdirection * distance;
            
            MoveCamera(followTarget.position + offset);
        }

        var pivot = transform.position;
        if (lookAtTarget)
            pivot = lookAtTarget.position;
        
        var rdirection = pivot - transform.position;
        RotateCamera(Quaternion.LookRotation(rdirection));
    }
    
    // void Update()
    // {
    //     if (Keyboard.current.cKey.wasPressedThisFrame)
    //         flipOffsetX = !flipOffsetX;
    //     
    //     var delta = Mouse.current.delta.ReadValue();
    //     float mouseX = delta.x * sensX;
    //     float mouseY = delta.y * sensY;
    //     
    //     yRotation += mouseX;
    //     xRotation -= mouseY;
    //     
    //     xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    //     
    //     targetCameraRotation = Quaternion.Euler(xRotation, yRotation, 0);
    //     targetCameraHolderPosition = playerCameraPosition.position;
    //     if (orientation)
    //         orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    // }

    private void Start()
    {
        // startingPosition = transform.localPosition;
        
        SetMouseVisibleAndLocked(true);
    }
    
    // private void LateUpdate()
    // {
    //     if (pivot)
    //         pivot.rotation = targetCameraRotation;
    //     else
    //         transform.rotation = targetCameraRotation;
    //     
    //     var offSetXAdjusted = offsetX;
    //     if (flipOffsetX)
    //         offSetXAdjusted *= -1;
    //     
    //     transform.localPosition = new Vector3(startingPosition.x + offSetXAdjusted, startingPosition.y + offsetY, targetPositionZ);
    //
    //     cameraHolder.position = targetCameraHolderPosition;
    // }
}
