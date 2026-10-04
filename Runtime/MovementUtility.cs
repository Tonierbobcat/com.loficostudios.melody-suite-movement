using UnityEngine;

namespace Movement_System.Runtime
{
    public class MovementUtility
    {

        public static bool ValidateRigidBodyOrDisable(MonoBehaviour behaviour)
        {
            var rb = behaviour.GetComponent<Rigidbody>();

            if (rb == null || rb.isKinematic)
                return true;
            
            Debug.LogError("RigidBody must be kinematic");
            behaviour.enabled = false;
            return false;
        }

        public static bool ValidateCharacterControllerOrDisable(MonoBehaviour behaviour, out CharacterController controller)
        {
            if (behaviour.TryGetComponent(out controller)) 
                return true;
            controller = null;
            Debug.LogError("GameObject must have a CharacterController");
            return false;

        }
        
        public static void EvaluateOnGround(Transform transform, CharacterController controller, LayerMask whatIsGround, ref bool grounded)
        {
            var bodyRadius = controller.radius;
            var bodyHalfHeight = controller.height * 0.5f;

            var origin = transform.position;
            var sphereCheckPosition = origin - (transform.up * bodyHalfHeight - transform.up * (bodyRadius * 0.5f));
            
            grounded = Physics.CheckSphere(sphereCheckPosition, bodyRadius, whatIsGround.value);
        }
    }
}