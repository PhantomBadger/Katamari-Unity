using UnityEngine;
using System.Collections;

/// <summary>
/// Contains the data needed to represent a sticky object
/// </summary>
public class StickyObjectProperties : MonoBehaviour
{
    public string ObjectName;
    public float Volume;
    public bool IsStuck = false;

    private Rigidbody rigid;
    private MeshCollider meshCollider;

	/// <summary>
    /// Called at the start, used for initialisation
    /// </summary>
	void Start ()
    {
        //If the name is empty give it our own temporary one
        if (ObjectName == null || ObjectName == "")
        {
            ObjectName = gameObject.name;
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

    /// <summary>
    /// Sticks this object to another
    /// </summary>
    /// <param name="objectToStickTo">The object to stick to</param>
    public void StickToObject(GameObject objectToStickTo)
    {
        //Make our parent the attached object so we rotate with it
        this.transform.parent = objectToStickTo.transform;

        //Cant disable the rigidbody, so we remove it
        Destroy(rigid);

        //Disable the mesh collider
        meshCollider.enabled = false;

        //Have all the Physics treat us as the parent/attached object
        IsStuck = true;
        this.tag = objectToStickTo.tag;
        this.gameObject.layer = objectToStickTo.layer;
    }
}
