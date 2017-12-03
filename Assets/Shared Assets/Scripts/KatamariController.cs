using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KatamariController : MonoBehaviour {

    public float Volume = 1.0f;
    public MeshCollider MeshCollider;

    Rigidbody rigid;
    Vector3 forwardVector = Vector3.forward;
    Vector3 rightVector = Vector3.right;
    List<GameObject> attachedObjects;    

    const float Speed = 0.25f;
    const float ForwardRotSpeed = 0.05f;
    const float TurningRotSpeed = 4.0f;
    const float AttachedVolumePercent = 0.5f;

    enum DistortType { DistortToOrigin, DistortToFurthestAway };

	/// <summary>
    /// Called at the start, gets all the references
    /// </summary>
	void Start ()
    {
        //Create list of game object
        attachedObjects = new List<GameObject>();

        //Get the rigidbody component
        rigid = GetComponent<Rigidbody>();
        if (rigid == null)
        {
            Debug.LogError("Katamari does not have a Rigidbody attached!");
            return;
        }

        if (MeshCollider == null)
        {
            Debug.LogError("Katamari's Collider child does not have a Mesh Collider attached!");
            return;
        }

        //TODO:
        // Get the Camera Controller Script Component from the Camera

        //Close the mesh from it's prefab to prevent local changes affecting the prefab
        MeshCollider.sharedMesh = (Mesh)Instantiate(MeshCollider.sharedMesh);
	}
	
	// Update is called once per frame
	void Update () {
        //Handle the user input
        UserInputHandler();

        //Reset velocity to prevent too much bouncing caused by the attached object
        if (rigid.velocity.y > 0)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void UserInputHandler()
    {
        //We use our own forward and right vectors to simulate rolling, allows the katamari to roll in a straight line even with
        //a lopsided mesh due to the objects attached to it

        //Move Forward
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 tempPos = transform.position;
            tempPos += forwardVector * Speed;
            transform.position = tempPos;

            transform.Rotate(rightVector, ForwardRotSpeed);
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

    void OnCollisionEnter(Collision col)
    {
        Debug.Log(col.gameObject.name);
        GameObject colObj = col.gameObject;

        //Chjeck if the object is already attached to us, or can actually be stuck to us
        if (colObj.tag == this.tag || colObj.GetComponent(typeof(StickyObjectProperties)) == null)
        {
            return;
        }
        //If it can be stuck to us, check that our volume is enough to stick it
        else if (colObj.GetComponent<StickyObjectProperties>().volume <= (this.Volume * AttachedVolumePercent) && !colObj.GetComponent<StickyObjectProperties>().isStuck)
        {
            //Create a fixed joint to attach the object to us
            FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = colObj.GetComponent<Rigidbody>();

            //Tell the object it is stuck to us and increase our volume
            colObj.GetComponent<StickyObjectProperties>().StickToObject(this.gameObject);
            this.Volume += colObj.GetComponent<StickyObjectProperties>().volume;

            //Distort our mesh
            DistortMesh(colObj);

            Debug.Log("Stuck object: " + colObj.name + " with volume: " + colObj.GetComponent<StickyObjectProperties>().volume);
        }
    }

    void DistortMesh(GameObject attachedObject, DistortType distortType = DistortType.DistortToOrigin)
    {
        //Create a local copy of the mesh first of all
        Mesh mesh = MeshCollider.sharedMesh;
        Vector3[] newVertices = mesh.vertices;

        int closestVertexIndex = 0;
        float minDistance = Mathf.Infinity;

        //Scan all vertices of our mesh to find the closest to the attached object
        for (int i = 0; i < newVertices.Length; i++)
        {
            Vector3 diff = transform.InverseTransformPoint(attachedObject.transform.position) - newVertices[i];
            float dist = diff.sqrMagnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
                closestVertexIndex = i;
            }
        }

        //Debug Code to see which vertex is chosen
        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = transform.TransformPoint(newVertices[closestVertexIndex]);

        if (distortType == DistortType.DistortToOrigin)
        {
            //Distort our mesh to the origin of the attached object
            newVertices[closestVertexIndex] = transform.InverseTransformPoint(attachedObject.transform.position);
        }
        else if (distortType == DistortType.DistortToFurthestAway)
        {
            //Distort our mesh to the furthest vertices of the attached object

            //Get the mesh of the attached object
            Mesh attachedMesh = attachedObject.GetComponent<MeshCollider>().sharedMesh;

            int furthestVertexIndex = 0;
            float maxDistance = Mathf.NegativeInfinity;

            //Scan all vertices to find the one furthest away
            for (int i = 0; i < attachedMesh.vertexCount; i++)
            {
                Vector3 diff = attachedMesh.vertices[i] - attachedObject.transform.InverseTransformPoint(transform.position);
                float dist = diff.sqrMagnitude;
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    furthestVertexIndex = i;
                }
            }

            newVertices[closestVertexIndex] = transform.InverseTransformPoint(attachedObject.transform.TransformPoint(attachedMesh.vertices[furthestVertexIndex]));
        }

        //Assign the new vertices to the old mesh
        mesh.vertices = newVertices;

        //Recalc the normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Assign the new mesh to the old mesh collider
        MeshCollider.sharedMesh = mesh;
    }

    public Vector3 GetForwardVector()
    {
        return forwardVector;
    }
}
