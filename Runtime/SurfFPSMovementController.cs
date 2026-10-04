using UnityEngine;
using UnityEngine.InputSystem;

namespace Movement_System.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public class SurfFPSMovementController : AbstractMovementController
    {
        [SerializeField]
        private float gravity = 28.6f;
        [SerializeField]
        private float jumpForce = 9.64f;
        [SerializeField]
        private float friction = 4.0f;
        [SerializeField]
        private float stopSpeed = 3.57f;
        [SerializeField]
        private float accelerate = 5.5f;
        [SerializeField]
        private float airAccelerate = 12f;
        [SerializeField]
        private Transform orientation;
  
        [SerializeField]
        private float jumpBufferTime = 0.15f; // 150ms buffer
        [SerializeField]
        private bool allowHoldJump = false;

        [SerializeField] private bool allowJump = true;
    
        // private Vector3 m_WishMoveDirection;
    
        private float m_JumpBufferCounter;
    
        private Vector2 m_Input;
    
        private bool m_JumpPressed;
        private bool m_JumpHeld;
    
        public bool surfing => !grounded && slopeAngle >= slopeLimit;
    
        private static readonly Vector3 k_XZPlane = new Vector3(1.0f, 0.0f, 1.0f);
    
        private void ApplyGravity(float deltaTime)
        {
            if (grounded)
            {
                velocity.y = -1f;
            }
            else
            {
                velocity.y -= gravity * deltaTime;
            }
        }
    
        public override void Move(InputAction.CallbackContext context)
        {
            m_Input = context.ReadValue<Vector2>();
        }
    
        public override void Jump(InputAction.CallbackContext context)
        {
            if (allowHoldJump)
            {
                if (context.performed)
                {
                    m_JumpHeld = true;
                }
                else if (context.canceled)
                {
                    m_JumpHeld = false;
                }
            }
            else if (context.performed)
            {
                m_JumpPressed = true;
            }
        }
    
        private void HandleInput()
        {
            // m_WishMoveDirection = (orientation.forward * m_Input.y + orientation.right * m_Input.x) * moveSpeed;
        
            if (m_JumpHeld)
                m_JumpPressed = true;
            if (m_JumpPressed)
            {
                m_JumpPressed = false;
                m_JumpBufferCounter = jumpBufferTime;
            }
        }

        protected override void CalculateVelocity(float delta)
        {
            HandleInput();
        
            m_JumpBufferCounter -= delta;
        
            if (allowJump && grounded && m_JumpBufferCounter > 0f)
            {
                grounded = false;
                velocity.y = jumpForce;
            
                m_JumpBufferCounter = 0f;
            }
        
            ApplyGravity(delta);
        
            if (grounded)
            {
                Friction(delta);
            
                var input = m_Input;
            
                Vector3 wishVel = orientation.forward * input.y + orientation.right * input.x;
                float wishSpeed = wishVel.magnitude * 250f;
                Vector3 wishDir = wishVel.normalized;

                GroundAccelerate(delta, wishDir, wishSpeed, accelerate);
            }
            else
            {
                Vector3 wishVel = orientation.forward * m_Input.y + orientation.right * m_Input.x;
                Vector3 wishDir = wishVel.normalized;
                float wishSpeed = wishVel.magnitude;
            
                AirAccelerate(delta, wishDir, wishSpeed, airAccelerate);
            }
        }
    
        private void GroundAccelerate(float deltaTime, Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
        
            if (addSpeed <= 0)
                return;

            float accelSpeed = accel * wishSpeed * deltaTime * 5.2f;

            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            velocity += wishDir * accelSpeed;
        }

        private void AirAccelerate(float deltaTime, Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
        
            if (addSpeed <= 0)
                return;
        
            float accelSpeed = wishSpeed * accel * deltaTime * 1;
        
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;
        
            velocity += wishDir * accelSpeed;
        }

        private void Friction(float deltaTime)
        {
            float speed = Vector3.Scale(k_XZPlane, velocity).magnitude;
            if (speed < 0.01f)
            {
                velocity = Vector3.Scale(velocity, Vector3.up);
                return;
            }

            float drop = 0.0f;
        
            float control = speed < stopSpeed ? stopSpeed : speed;
            drop += control * friction * deltaTime;

            float newSpeed = Mathf.Max(speed - drop, 0.0f) / speed;
            velocity *= newSpeed;
        }
    

    
    }
}
