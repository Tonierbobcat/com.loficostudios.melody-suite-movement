using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockMouse : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
