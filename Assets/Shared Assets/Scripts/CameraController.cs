using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    [SerializeField]
    GameObject katamari;

    [SerializeField]
    Vector3 offset;

    KatamariController katamariController;
    Vector3 forwardVector;

	// Use this for initialization
	void Start () {
        //Check that the Katamari Object isnt null
        if (katamari == null)
        {
            Debug.LogError("The Katamari Reference in the Camera Script is null");
            return;
        }

        //Get the Katamari's Controller Script
        katamariController = katamari.GetComponent<KatamariController>();
        if (katamariController == null)
        {
            Debug.LogError("The Katamari has no KatamariController script attached");
            return;
        }

        transform.position = katamari.transform.position + offset;
	}
	
	// Update is called once per frame
	void Update () {
        //Get the forward vector from the katamari script
        forwardVector = katamariController.GetForwardVector();

        //Update the position and reposition based on the forward vector
        Vector3 newPos = katamari.transform.position + Vector3.Scale(offset, forwardVector);

        newPos.y = offset.y;
        transform.position = newPos;

        //Look at the ball
        //Use our position and the offset for the y to create a smoother camera experience
        //Using the katamari's position would cause a lot of jitter with the weird mesh shape
        transform.LookAt(new Vector3(katamari.transform.position.x, transform.position.y - offset.y, katamari.transform.position.z));
	}
}
