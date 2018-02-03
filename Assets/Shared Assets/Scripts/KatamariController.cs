using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// A third person instance of the ball controller to allow keyboard control
/// </summary>
public class KatamariController : BaseController
{
    [SerializeField] Vector3 forwardVector = Vector3.forward;
    [SerializeField] Vector3 rightVector = Vector3.right;

    /// <summary>
    /// Called every frame, handles user input
    /// </summary>
    protected override void FixedUpdate()
    {
        //Handle the user input
        UserInputHandler();

        base.FixedUpdate();
    }

    /// <summary>
    /// Handles the user input, rolls forward or turns using our own forward/right vectors
    /// </summary>
    private void UserInputHandler()
    {
        //We use our own forward and right vectors to simulate rolling, allows the katamari to roll in a straight line even with
        //a lopsided mesh due to the objects attached to it

        //Move Forward
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 tempPos = transform.position;
            tempPos += forwardVector * Speed;
            transform.position = tempPos;

            transform.Rotate(rightVector, ForwardRotSpeed, Space.World);
        }

        //Rotate to the Right
        if (Input.GetKey(KeyCode.D))
        {
            forwardVector = Quaternion.Euler(0.0f, TurningRotSpeed, 0.0f) * forwardVector;
            rightVector = Quaternion.Euler(0.0f, TurningRotSpeed, 0.0f) * rightVector;
        }

        //Rotate to the Left
        if (Input.GetKey(KeyCode.A))
        {
            forwardVector = Quaternion.Euler(0.0f, -TurningRotSpeed, 0.0f) * forwardVector;
            rightVector = Quaternion.Euler(0.0f, -TurningRotSpeed, 0.0f) * rightVector;
        }
    }

    /// <summary>
    /// Gets the proxy forward vector of the object used for moving
    /// </summary>
    /// <returns>The proxy forward vector used for movement</returns>
    public Vector3 GetForwardVector()
    {
        return forwardVector;
    }
}
