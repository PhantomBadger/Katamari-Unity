using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class KatamariHandController : MonoBehaviour
{
    [SerializeField] private Transform headPosition;
    [SerializeField] private KatamariControllerVR katamariController;

    private Vector3 headToPrevPos;
    private SteamVR_TrackedObject steamVRComponent;
    private SteamVR_Controller.Device steamVRController;

    private const float MinimumMovementThreshold = 0.01f;
    private const float AngularVelocityScale = 5.0f;

    /// <summary>
    /// Called at the start, gets controller component references
    /// </summary>
    private void Start ()
    {
        steamVRComponent = GetComponent<SteamVR_TrackedObject>();
        steamVRController = SteamVR_Controller.Input((int)steamVRComponent.index);
    }
	
	/// <summary>
    /// Called once per frame, reads controller button inputs
    /// </summary>
	private void Update ()
    {
		if (steamVRController.GetHairTrigger())
        {
            //If the hair trigger was put down this frame populate the prev pos values
            if (steamVRController.GetHairTriggerDown())
            {
                headToPrevPos = transform.position - headPosition.position;
            }

            //Translate the controller movement into ball movement
            TranslateControllerMovement();

            //Update the previous frame values
            headToPrevPos = transform.position - headPosition.position;
        }
    }

    /// <summary>
    /// Translate the controllers movement into ball movement
    /// </summary>
    private void TranslateControllerMovement()
    {
        //Get the head to position vectors
        Vector3 headToOldPos = headToPrevPos;
        Vector3 headToNewPos = transform.position - headPosition.position;

        //Get the distance between the two positions, if it's too small ignore it
        Vector3 deltaPosition = headToNewPos - headToOldPos;
        if (deltaPosition.magnitude < MinimumMovementThreshold)
        {
            return;
        }

        //Get the cross product of thw two vectors in order to get an axis of rotation
        Vector3 axis = Vector3.Cross(headToNewPos, headToOldPos);
        float signedAngleBetween = Vector3.SignedAngle(headToOldPos, headToNewPos, axis);

        Vector3 direction = Vector3.Cross(axis, Vector3.up);

        Vector3 angularVelocity = signedAngleBetween * axis;

        //V = rW (Velocity = Radius * Angular Velocity)
        Vector3 velocity = angularVelocity * 2;
        //velocity = Quaternion.Euler(0, -90, 0) * velocity;

        //Project onto the horizontal plane to remove vertical components
        Vector3 projectedDirection = Vector3.ProjectOnPlane(velocity, Vector3.up);

        Debug.Log("Projected Direction: " + projectedDirection +
            "\nRaw Direction: " + velocity +
            "\nAngular Velocity: " + angularVelocity);

        Rigidbody katamariRigidbody = katamariController.GetComponent<Rigidbody>();

        //Calculate the new angular velocity, and then clamp it in the current direction
        //This should allow us to affect existing movements but not exceed our limit
        Vector3 scaledAngularVelocity = angularVelocity * AngularVelocityScale;
        Vector3 newAngularVelocity = katamariRigidbody.angularVelocity + scaledAngularVelocity;

        katamariRigidbody.angularVelocity = new Vector3(
            katamariRigidbody.angularVelocity.x < 0 ? Mathf.Max(scaledAngularVelocity.x, newAngularVelocity.x) : Mathf.Min(scaledAngularVelocity.x, newAngularVelocity.x),
            katamariRigidbody.angularVelocity.y < 0 ? Mathf.Max(scaledAngularVelocity.y, newAngularVelocity.y) : Mathf.Min(scaledAngularVelocity.y, newAngularVelocity.y),
            katamariRigidbody.angularVelocity.z < 0 ? Mathf.Max(scaledAngularVelocity.z, newAngularVelocity.z) : Mathf.Min(scaledAngularVelocity.z, newAngularVelocity.z));


        //katamariRigidbody.velocity += projectedDirection;

        //TODO: Experiment with a combination of this and the linear movement,
        // - linear movement when held down then standard angular for a frame???

        //katamariController.RollTowards(projectedDirection.normalized);
    }
}
