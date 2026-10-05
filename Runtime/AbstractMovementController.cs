using MelodySuite.Core.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MelodySuite.Movement.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class AbstractMovementController : PlayerBehaviour
    {
        [SerializeField] private LayerMask whatIsGround;
        
        private CharacterController characterController;
        
        protected Vector3 velocity = Vector3.zero;

        public bool grounded { get; protected set; }

        public float slopeAngle { get; protected set; }

        public Vector3 slopeNormal { get; protected set; }
        
        protected float slopeLimit => 30;
        
        public Vector3 Velocity => velocity;

        protected virtual void Awake()
        {
            if (!MovementUtility.ValidateRigidBodyOrDisable(this))
                return;
            MovementUtility.ValidateCharacterControllerOrDisable(this, out characterController);
        }
        
        protected abstract void CalculateVelocity(float delta);
        
        private void Update()
        {
            var bodyRadius = characterController.radius;
            var bodyHalfHeight = characterController.height * 0.5f;
            
            var sphereCheckPosition = transform.position - transform.up * (bodyHalfHeight - bodyRadius);
            
            if (Physics.Raycast(sphereCheckPosition, Vector3.down, out var hit, bodyRadius + 0.1f, whatIsGround))
            {
                slopeNormal = hit.normal;
                slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                grounded = slopeAngle < slopeLimit;
            }
            else
            {
                grounded = false;
                slopeNormal = Vector3.up;
                slopeAngle = 0f;
            }
            
            CalculateVelocity(Time.deltaTime);
            
            if (characterController)
                characterController.Move(velocity * Time.deltaTime);
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            var bodyRadius = characterController.radius;
            var bodyHalfHeight = characterController.height * 0.5f;

            var origin = transform.position;
            var sphereCheckPosition = origin - transform.up * (bodyHalfHeight - bodyRadius);
            
            // Gizmos.color = Color.green;
            // Gizmos.DrawWireSphere(sphereCheckPosition, bodyRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(sphereCheckPosition, sphereCheckPosition + Vector3.down * (bodyRadius + 0.1f));
        }

        public virtual void Sprint(InputAction.CallbackContext context)
        {
        }
        
        public virtual void Crouch(InputAction.CallbackContext context)
        {
        }

        public virtual void SlowWalk(InputAction.CallbackContext context)
        {
        }
        
        public virtual void Move(InputAction.CallbackContext context)
        {
        }
        
        public virtual void Jump(InputAction.CallbackContext context)
        {
        }
    }
}