using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class LookSettings
{
    public bool m_invertCamY;
    public bool m_invertCamX;

    public float m_sensX = 10f;
    public float m_sensY = 10f;
    public bool m_combinedSens = true;

    public bool m_smooth = true;
    public float m_smoothTime = 0.05f;
}

[Serializable]
public class ThirdPersonCameraSettings
{
    public LookSettings m_lookSettings = new();

    public float m_fov = 60f;
    public float m_radius = 5f;

    [Header("Zoom")]
    public float m_zoomMultiplier = 1f;
    public float m_minZoomMultiplier = 0.4f;
    public float m_maxZoomMultiplier = 3f;
    public float m_zoomSmoothTime = 0.1f;

    public bool m_enableCollisions;
    public float m_cameraCollisionRadius = 0.1f;
    public LayerMask m_collisionMask;
    
    // [TagField]
    // public string ignoreTag;
}

[Serializable]
public class FirstPersonCameraSettings
{
    public LookSettings m_lookSettings = new();

    public float m_fov = 60f;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private FirstPersonCameraSettings m_firstPersonCameraSettings = new();
    [SerializeField]
    private ThirdPersonCameraSettings m_thirdPersonCameraSettings = new();
    
    [SerializeField]
    private Transform m_cameraPivot;

    [SerializeField] private bool m_inputEnabled = true;
    
    [SerializeField] private CameraType m_cameraType = CameraType.FirstPerson;
    
    [SerializeField] private InputActionReference look;
    [SerializeField] private InputActionReference zoom;
    
    private CinemachineCamera _thirdPersonCamera;
        
    private CinemachineOrbitalFollow _orbitalFollow;
    
    [SerializeField]
    private bool m_autoEnableInputs = true;
    [SerializeField]
    private bool m_autoChangeCursor = true;
    
    private void CameraZoom(InputAction.CallbackContext context)
    {
        // Debug.Log("Camera Zoom Triggered: " + context.ReadValue<Vector2>().y);
        var delta = context.ReadValue<Vector2>().y;
        var sens = 8;
        m_thirdPersonCameraSettings.m_zoomMultiplier -= delta * 0.01f * sens; 
        m_thirdPersonCameraSettings.m_zoomMultiplier = Mathf.Clamp(
            m_thirdPersonCameraSettings.m_zoomMultiplier, 
            m_thirdPersonCameraSettings.m_minZoomMultiplier, 
            m_thirdPersonCameraSettings.m_maxZoomMultiplier);
    }
    
    private void Awake()
    {
        if (m_autoEnableInputs)
            look.action.Enable();
        
        if (m_cameraType == CameraType.ThirdPerson)
        {
            InitializeThirdPersonCamera();
        }
        else
        {
            InitializeFirstPersonCamera();
        }
        
        lastCameraType = m_cameraType;
    }

    private CinemachineCamera _firstPersonCamera;
    
    private void InitializeFirstPersonCamera()
    {
        var obj = new GameObject("FirstPersonCamera");
        obj.transform.parent = transform;
        _firstPersonCamera = obj.AddComponent<CinemachineCamera>();
        _firstPersonCamera.Target = new CameraTarget()
        {
            TrackingTarget = m_cameraPivot
        };

        var panTilt = obj.AddComponent<CinemachinePanTilt>();
        var hardLock = obj.AddComponent<CinemachineHardLockToTarget>();
        var axisController = obj.AddComponent<CinemachineInputAxisController>();
        InitController(axisController, m_firstPersonCameraSettings.m_lookSettings, 0);
        InitController(axisController, m_firstPersonCameraSettings.m_lookSettings, 1);
    }

    private void InitializeThirdPersonCamera()
    {
        if (zoom != null)
        {
            if (m_autoEnableInputs)
                zoom.action.Enable();
            
            zoom.action.performed += CameraZoom;
        }
        
        var obj = new GameObject("ThirdPersonCamera");
        obj.transform.parent = transform;
        _thirdPersonCamera = obj.AddComponent<CinemachineCamera>();
        _thirdPersonCamera.Target = new CameraTarget()
        {
            TrackingTarget = m_cameraPivot
        };

        _thirdPersonCamera.Lens.FieldOfView = m_thirdPersonCameraSettings.m_fov;
            
        _orbitalFollow = obj.AddComponent<CinemachineOrbitalFollow>();
        _orbitalFollow.TrackerSettings.PositionDamping = Vector3.zero;
        _orbitalFollow.Radius = m_thirdPersonCameraSettings.m_radius;
        _orbitalFollow.HorizontalAxis.Center = 0;
        _orbitalFollow.HorizontalAxis.Range = new Vector2(-180, 180);
        _orbitalFollow.VerticalAxis.Center = 0;
        _orbitalFollow.VerticalAxis.Range = new Vector2(-60, 89);

        var rotationComposer = obj.AddComponent<CinemachineRotationComposer>();
        rotationComposer.Composition.ScreenPosition = Vector2.zero;
        rotationComposer.Composition.DeadZone = default;
        rotationComposer.Composition.HardLimits = default;
            
        rotationComposer.TargetOffset = Vector3.zero;
        rotationComposer.Damping = Vector3.zero;
        rotationComposer.Lookahead = default;
            
        var axisController = obj.AddComponent<CinemachineInputAxisController>();
        axisController.AutoEnableInputs = false;

        InitController(axisController, m_thirdPersonCameraSettings.m_lookSettings, 0);
        InitController(axisController, m_thirdPersonCameraSettings.m_lookSettings, 1);

        var deoccluder = obj.AddComponent<CinemachineDeoccluder>();
        deoccluder.CollideAgainst = m_thirdPersonCameraSettings.m_collisionMask;
        deoccluder.AvoidObstacles.CameraRadius = 0.4f;
        deoccluder.AvoidObstacles.Enabled = true;
        deoccluder.AvoidObstacles.CameraRadius = m_thirdPersonCameraSettings.m_cameraCollisionRadius;
        deoccluder.enabled = m_thirdPersonCameraSettings.m_enableCollisions;
        // deoccluder.IgnoreTag = m_thirdPersonCameraSettings.ignoreTag;
    }
    
    private void ApplySensitivity(CinemachineInputAxisController axisController, LookSettings lookSettings, int controllerIndex)
    {
        var sens = controllerIndex == 0
            ? lookSettings.m_sensX
            : (lookSettings.m_combinedSens ? -lookSettings.m_sensX : lookSettings.m_sensY);
        
        var invert = controllerIndex == 0
            ? lookSettings.m_invertCamX
            : lookSettings.m_invertCamY;
        
        var controller = axisController.Controllers[controllerIndex];
        controller.Driver.AccelTime = lookSettings.m_smooth ? lookSettings.m_smoothTime : 0;
        controller.Driver.DecelTime = lookSettings.m_smooth ? lookSettings.m_smoothTime : 0;
        controller.Input.Gain = invert ? -sens : sens;
    }
    
    private void InitController(CinemachineInputAxisController axisController, LookSettings lookSettings, int controllerIndex)
    {
        var controller = axisController.Controllers[controllerIndex];
        
        controller.Input.InputAction = look;
        ApplySensitivity(axisController, lookSettings, controllerIndex);
    }

    public bool InputEnabled
    {
        get => m_inputEnabled;
        set => m_inputEnabled = value;
    }

    private void HandleFirstPersonUpdate()
    {
        
        _firstPersonCamera.Lens.FieldOfView = m_firstPersonCameraSettings.m_fov;
        var axisController = _firstPersonCamera.GetComponent<CinemachineInputAxisController>();
        axisController.enabled = m_inputEnabled;
        ApplySensitivity(axisController, m_firstPersonCameraSettings.m_lookSettings, 0);
        ApplySensitivity(axisController, m_firstPersonCameraSettings.m_lookSettings, 1);
    }
    private float _zoomVelocity;
    private void HandleThirdPersonUpdate()
    {
       _thirdPersonCamera.GetComponent<CinemachineDeoccluder>().enabled = m_thirdPersonCameraSettings.m_enableCollisions;
       
        _thirdPersonCamera.Lens.FieldOfView = m_thirdPersonCameraSettings.m_fov;
        var axisController = _thirdPersonCamera.GetComponent<CinemachineInputAxisController>();
        axisController.enabled = m_inputEnabled;
        ApplySensitivity(axisController, m_thirdPersonCameraSettings.m_lookSettings, 0);
        ApplySensitivity(axisController, m_thirdPersonCameraSettings.m_lookSettings, 1);
        
        var targetRadius = Mathf.Clamp(m_thirdPersonCameraSettings.m_radius * m_thirdPersonCameraSettings.m_zoomMultiplier, 
            m_thirdPersonCameraSettings.m_radius * m_thirdPersonCameraSettings.m_minZoomMultiplier, 
            m_thirdPersonCameraSettings.m_radius * m_thirdPersonCameraSettings.m_maxZoomMultiplier);
            
        _orbitalFollow.Radius = Mathf.SmoothDamp(
            _orbitalFollow.Radius,
            targetRadius,
            ref _zoomVelocity,
            m_thirdPersonCameraSettings.m_zoomSmoothTime
        );
    }

    private void HandleFirstPersonLateUpdate()
    {
    }

    private void HandleThirdPersonLateUpdate()
    { }

    private CameraType lastCameraType;
    
    void Update()
    {
        if (m_autoChangeCursor)
        {
            if (m_inputEnabled)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
        if (lastCameraType != m_cameraType)
        {
            switch (m_cameraType)
            {
                case CameraType.ThirdPerson:
                    if (_firstPersonCamera)
                        _firstPersonCamera.enabled = false;
                    
                    if (_thirdPersonCamera == null)
                    {
                        InitializeThirdPersonCamera();
                    }
                    
                    _thirdPersonCamera.enabled = true;
                    break;
                case CameraType.FirstPerson:
                    if (_thirdPersonCamera)
                        _thirdPersonCamera.enabled = false;

                    if (_firstPersonCamera == null)
                    {
                        InitializeFirstPersonCamera();
                    }
                    
                    _firstPersonCamera.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        lastCameraType = m_cameraType;
        
        switch (m_cameraType)
        {
            case CameraType.ThirdPerson:
                HandleThirdPersonUpdate();
                break;
            case CameraType.FirstPerson:
                HandleFirstPersonUpdate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public enum CameraType
{
    ThirdPerson,
    FirstPerson
}