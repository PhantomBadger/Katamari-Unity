using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class KatamariHandController : MonoBehaviour
{
    [SerializeField] private KatamariControllerVR katamariController;

    private Vector3 positionLastFrame;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
