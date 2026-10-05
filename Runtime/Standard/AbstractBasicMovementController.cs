using UnityEngine;
using UnityEngine.InputSystem;

namespace MelodySuite.Movement.Runtime
{
    public abstract class AbstractBasicMovementController : AbstractMovementController
    {
        
        [SerializeField]
        private float baseMoveSpeed = 5;
        [SerializeField]
        protected Transform orientation;
        [SerializeField]
        private float jumpBufferTime = 0.15f;
        [SerializeField]
        private float jumpForce = 9f;
        [SerializeField]
        private float gravity = 28.6f;
        [SerializeField] 
        private float sprintSpeedMultiplier = 1.5f;

        public bool InputEnabled { get; set; } = true;

        protected Vector2 input;
        private float _jumpBufferCounter;
        private bool _jumpPressed;

        public float MoveSpeed { get; set; }

        public bool Sprinting { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            MoveSpeed = baseMoveSpeed;
        }

        protected abstract Vector3 CalculateWishDirection();
        
        protected override void CalculateVelocity(float delta)
        {
            if (_jumpPressed)
            {
                _jumpPressed = false;
                _jumpBufferCounter = jumpBufferTime;
            }
            
            
            ApplyGravity(delta);
        
            var wishDir = CalculateWishDirection();
            wishDir.y = 0f;
        
            if (wishDir.sqrMagnitude > 0f)
                wishDir.Normalize();
            var targetSpeed = Sprinting
                ? MoveSpeed * sprintSpeedMultiplier
                : MoveSpeed;
            
            velocity.x = wishDir.x * targetSpeed;
            velocity.z = wishDir.z * targetSpeed;
        }
        
        public override void Move(InputAction.CallbackContext context)
        {
            input = context.ReadValue<Vector2>();
        }
        
        public override void Jump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _jumpPressed = true;
            }
        }

        public override void Sprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Sprinting = true;
            }
            else if (context.canceled)
            {
                Sprinting = false;
            }
        }

        private void ApplyGravity(float delta)
        {
            _jumpBufferCounter -= delta;

            var jump = grounded && _jumpBufferCounter > 0f;
            
            if (jump)
            {
                velocity.y = jumpForce;
            
                _jumpBufferCounter = 0f;
            }
            else
            {
                if (grounded)
                {
                    velocity.y = -1f;
                }
                else
                {
                    velocity.y -= gravity * delta;
                } 
            }
        }
    }
}