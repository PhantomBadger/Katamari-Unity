using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class KatamariHandController : MonoBehaviour
{
    [SerializeField] private Transform headPosition;
    [SerializeField] private KatamariControllerVR katamariController;
    [SerializeField] private bool showDebugMessages = false;

    private Vector3 headToPrevPos;
    private SteamVR_TrackedObject steamVRComponent;
    private SteamVR_Controller.Device steamVRController;

    private const float MinimumMovementThreshold = 0.01f;
    private const float AngularVelocityScale = 20.0f;
    private const float KatamariRadius = 2.0f;

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
        else if (steamVRController.GetHairTriggerUp())
        {
            //If the hair trigger was released this frame, apply the angular velocity impulse
            Vector3 impulseAngularVelocity = CalculateImposedAngularVelocity();
            Vector3 impulseVelocity = CalculateImposedVelocity(impulseAngularVelocity);
            ApplyAngularVelocityImpulse(impulseVelocity, impulseAngularVelocity);
        }
    }

    /// <summary>
    /// Translate the controllers movement into ball movement
    /// </summary>
    private void TranslateControllerMovement()
    {
        Vector3 velocity = CalculateImposedVelocity();

        //Project onto the horizontal plane to remove vertical components
        Vector3 projectedDirection = Vector3.ProjectOnPlane(velocity, Vector3.up);

        if (showDebugMessages)
        {
            Debug.Log("TranslateControllerMovement()" +
                "\nProjected Direction: " + projectedDirection +
                "\nVelocity: " + velocity);
        }

        katamariController.RollTowards(projectedDirection.normalized);
    }

    /// <summary>
    /// In the case that the player has stopped actively interacting with the controller
    /// This method will impart an angular velocity impulse to give the feeling of 'throwing' the ball
    /// </summary>
    private void ApplyAngularVelocityImpulse(Vector3 velocity, Vector3 angularVelocity)
    {
        //Project onto the horizontal plane to remove vertical components
        Vector3 projectedDirection = Vector3.ProjectOnPlane(velocity, Vector3.up);

        Rigidbody katamariRigidbody = katamariController.GetComponent<Rigidbody>();

        //Calculate the new angular velocity, and then clamp it in the current direction
        //This should allow us to affect existing movements but not exceed our limit
        Vector3 scaledAngularVelocity = angularVelocity * AngularVelocityScale;
        Vector3 newAngularVelocity = katamariRigidbody.angularVelocity + scaledAngularVelocity;

        if (showDebugMessages)
        {
            Debug.Log("ApplyAngularVelocityImpulse(Vector3 velocity, Vector3 angularVelocity)" +
                        "\nAngular Velocity Impulse: " + scaledAngularVelocity +
                       "\nNew Total: " + newAngularVelocity +
                       "\nCurrent Angular: " + katamariRigidbody.angularVelocity);
        }

        katamariRigidbody.angularVelocity = new Vector3(
            katamariRigidbody.angularVelocity.x < 0 ? Mathf.Max(scaledAngularVelocity.x, newAngularVelocity.x) : Mathf.Min(scaledAngularVelocity.x, newAngularVelocity.x),
            katamariRigidbody.angularVelocity.y < 0 ? Mathf.Max(scaledAngularVelocity.y, newAngularVelocity.y) : Mathf.Min(scaledAngularVelocity.y, newAngularVelocity.y),
            katamariRigidbody.angularVelocity.z < 0 ? Mathf.Max(scaledAngularVelocity.z, newAngularVelocity.z) : Mathf.Min(scaledAngularVelocity.z, newAngularVelocity.z));

    }

    /// <summary>
    /// Calculates the angular velocity imposed this frame from the difference between the controller's delta position
    /// </summary>
    /// <returns>The angular velocity of the ball</returns>
    private Vector3 CalculateImposedAngularVelocity()
    {
        //Get the head to position vectors
        Vector3 headToOldPos = headToPrevPos;
        Vector3 headToNewPos = transform.position - headPosition.position;

        //Get the distance between the two positions, if it's too small ignore it
        Vector3 deltaPosition = headToNewPos - headToOldPos;
        if (deltaPosition.magnitude < MinimumMovementThreshold)
        {
            return Vector3.zero;
        }

        //Get the cross product of thw two vectors in order to get an axis of rotation
        Vector3 axis = Vector3.Cross(headToNewPos, headToOldPos);
        float signedAngleBetween = Vector3.SignedAngle(headToOldPos, headToNewPos, axis);

        Vector3 angularVelocity = signedAngleBetween * axis;

        if (showDebugMessages)
        {
            Debug.Log("CalculateImposedAngularVelocity()" +
                "\nAngular Velocity: " + angularVelocity +
                "\nAxis: " + axis +
                "\nSigned Angle Between: " + signedAngleBetween +
                "\nDelta Position: " + deltaPosition +
                "\nHead to Old Pos: " + headToOldPos +
                "\nHead to New Pos: " + headToNewPos +
                "\nCur Pos: " + transform.position);
        }
        return angularVelocity;
    }

    /// <summary>
    /// Calculates the velocity imposed this frame from the difference between the controller's delta position
    /// </summary>
    /// <returns>The velocity of the ball</returns>
    private Vector3 CalculateImposedVelocity()
    {
        //Get the position of the ball
        Vector3 position = katamariController.transform.position;

        //The point of rotation is the ball's contact point with the ground
        Vector3 pointOfRotation = katamariController.transform.position + new Vector3(0, -KatamariRadius, 0);

        //Calculate angular velocity
        Vector3 angularVelocity = CalculateImposedAngularVelocity();

        //Linear Velocity = Angular Velocity Cross The difference between the position of the ball and the point of rotation
        Vector3 linearVelocity = Vector3.Cross(angularVelocity, position - pointOfRotation);

        Debug.Log("CalculateImposedVelocity()" +
            "\nLinear Velocity: " + linearVelocity +
            "\nAngular Velocity: " + angularVelocity);

        return linearVelocity;

        //V = rW (Velocity = Radius * Angular Velocity)
        //return CalculateImposedAngularVelocity() * KatamariRadius;
        //return Vector3.Cross(CalculateImposedAngularVelocity(), (transform.position - (transform.position + new Vector3(0, -KatamariRadius, 0))));
    }

    /// <summary>
    /// Calculates the velocity imposed this frame from the difference between the controller's delta position
    /// </summary>
    /// <param name="angularVelocity">The angular velocity of the ball calculated from the controller's delta position</param>
    /// <returns>The velocity of the ball</returns>
    private Vector3 CalculateImposedVelocity(Vector3 angularVelocity)
    {
        //Get the position of the ball
        Vector3 position = katamariController.transform.position;

        //The point of rotation is the ball's contact point with the ground
        Vector3 pointOfRotation = katamariController.transform.position + new Vector3(0, -KatamariRadius, 0);

        Vector3 linearVelocity = Vector3.Cross(angularVelocity, position - pointOfRotation);

        Debug.Log("CalculateImposedVelocity(Vector3 angularVelocity)" +
            "\nLinear Velocity: " + linearVelocity +
            "\nAngular Velocity: " + angularVelocity);

        return linearVelocity;

        //V = rW (Velocity = Radius * Angular Velocity)
        //return Vector3.Cross(angularVelocity, (transform.position - (transform.position + new Vector3(0, -KatamariRadius, 0))));
    }
}
