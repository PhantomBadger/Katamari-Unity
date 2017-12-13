using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonKatamariHead : MonoBehaviour
{
    public GameObject Katamari;
    public Vector3 Offset = new Vector3(0, -5, 0);
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = Katamari.transform.position + Offset;
	}
}
