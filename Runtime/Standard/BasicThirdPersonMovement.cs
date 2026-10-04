using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Movement_System.Runtime.Standard
{
    public class BasicThirdPersonMovement : AbstractBasicMovementController
    {

        [SerializeField]
        private float rotationSpeed;
        
        protected override Vector3 CalculateWishDirection()
        {
            if (!InputEnabled)
            {
                return Vector3.zero;
            }
            var forward = orientation.forward;
            var right = orientation.right;
            return forward * input.y + right * input.x;
        }

        protected override void CalculateVelocity(float delta)
        {
            var hasInput = input.sqrMagnitude > 0.001f;
            if (hasInput)
            {
                var wishDir = CalculateWishDirection();
                wishDir.y = 0f;

                if (wishDir != Vector3.zero)
                {
                    var targetRotation = Quaternion.LookRotation(wishDir);
                
                    // only rotate on Y axis
                    targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
                
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * delta);   
                }
            }

            base.CalculateVelocity(delta);
        }
    }
}