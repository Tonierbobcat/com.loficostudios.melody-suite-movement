using Movement_System.Runtime;
using TMPro;
using UnityEngine;

public class DebugMovementInfo : MonoBehaviour
{
    [SerializeField]
    private AbstractMovementController  movementController;
    
    private void Update()
    {
        if (movementController == null)
            return;
        var vel =  movementController.Velocity;
        GetComponent<TextMeshProUGUI>().text = $"velocity: {new Vector3(vel.x, 0, vel.z).magnitude}\n" +
                                               $"gravity: {vel.y}\n" +
                                               $"onGround: {movementController.grounded}\n" +
                                               // $"sliding: {movementController.surfing}\n" +
                                               $"slope: {movementController.slopeAngle}";
    }
}
