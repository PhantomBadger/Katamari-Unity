using UnityEngine;
using System.Collections;

public class StickyObjectProperties : MonoBehaviour {

    public string objectName;
    public float volume;
    public bool isStuck = false;

    Rigidbody rigid;
    MeshCollider meshCollider;

	// Use this for initialization
	void Start () {
        //If the name is empty give it our own temporary one
        if (objectName == null || objectName == "")
        {
            objectName = gameObject.name;
        }

        //Get the rigidbody component
        rigid = GetComponent<Rigidbody>();
        if (rigid == null)
        {
            Debug.LogError("Sticky Object '" + gameObject.name + "' does not have a rigidbody attached!");
            return;
        }

        //Get the mesh collider component
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            Debug.LogError("Sticky Object '" + gameObject.name + "' does not have a mesh collider attached!");
            return;
        }
	}

    public void StickToObject(GameObject obj)
    {
        //Make our parent the attached object so we rotate with it
        this.transform.parent = obj.transform;

        //Cant disable the rigidbody, so we remove it
        Destroy(rigid);

        //Disable the mesh collider
        meshCollider.enabled = false;

        //Have all the Physics treat us as the parent/attached object
        isStuck = true;
        this.tag = obj.tag;
        this.gameObject.layer = obj.layer;
    }
}
