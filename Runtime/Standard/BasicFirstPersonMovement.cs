using UnityEngine;

namespace MelodySuite.Movement.Runtime
{
    public class BasicFirstPersonMovement : AbstractBasicMovementController
    {
        [SerializeField] private Camera cam;
        protected override Vector3 CalculateWishDirection()
        {
            if (!inputEnabled)
            {
                return Vector3.zero;
            }
            return orientation.forward * input.y + orientation.right * input.x;
        }

        protected override void CalculateVelocity(float delta)
        {
            if (cam)
                transform.rotation = Quaternion.Euler(new Vector3(0, cam.transform.eulerAngles.y, 0));
            base.CalculateVelocity(delta);
        }
    }
}
