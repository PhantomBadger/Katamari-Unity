﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// VR instance of the ball controller to allow linear movement
/// </summary>
public class KatamariControllerVR : BaseController
{
    private Vector3 lastDirection;

    /// <summary>
    /// Rolls toward a given direction, moving a set amount forward in a linear translation
    /// </summary>
    /// <param name="direction">The direction to roll towards</param>
    public void RollTowards(Vector3 direction)
    {
        //Update the position
        Vector3 tempPos = transform.position;
        tempPos += direction * Speed;
        transform.position = tempPos;

        //Find the axis of rotation, if the given direction is forward, then its 90° to the right
        Vector3 axisVector = Quaternion.Euler(0.0f, 90.0f, 0.0f) * direction;

        //Rotate along the axis by a set amount
        transform.Rotate(axisVector, ForwardRotSpeed, Space.World);
        lastDirection = direction;
    }
}
