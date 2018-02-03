using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base controller containing the common mesh distortion methods
/// </summary>
public class BaseController : MonoBehaviour
{
    public float Volume = 1.0f;
    public MeshCollider MeshCollider;

    protected Rigidbody rigid;
    protected List<GameObject> attachedObjects;

    protected const float Speed = 0.35f;
    protected const float ForwardRotSpeed = 4.0f;
    protected const float TurningRotSpeed = 4.0f;
    protected const float AttachedVolumePercent = 0.5f;

    protected enum DistortType
    {
        DISTORT_TO_ORIGIN,
        DISTORT_TO_FURTHEST_AWAY
    };

    /// <summary>
    /// Called at the start, gets all the references
    /// </summary>
    protected virtual void Start()
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

    /// <summary>
    /// Called once per frame
    /// </summary>
    protected virtual void FixedUpdate()
    {
        //Reset velocity to prevent too much bouncing caused by the attached object
        //if (rigid.velocity.y > 0)
        //{
        //    rigid.velocity = Vector3.zero;
        //    rigid.angularVelocity = Vector3.zero;
        //}
    }

    /// <summary>
    /// Called when entering the collider for something else, if applicable it sticks it to ourselves
    /// </summary>
    /// <param name="col">The collision data for the object we've hit</param>
    protected virtual void OnCollisionEnter(Collision col)
    {
        GameObject colObj = col.gameObject;

        //Chjeck if the object is already attached to us, or can actually be stuck to us
        if (colObj.tag == this.tag || colObj.GetComponent(typeof(StickyObjectProperties)) == null)
        {
            return;
        }
        //If it can be stuck to us, check that our volume is enough to stick it
        else if (colObj.GetComponent<StickyObjectProperties>().Volume <= (this.Volume * AttachedVolumePercent) && !colObj.GetComponent<StickyObjectProperties>().IsStuck)
        {
            //Create a fixed joint to attach the object to us
            FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = colObj.GetComponent<Rigidbody>();

            //Tell the object it is stuck to us and increase our volume
            colObj.GetComponent<StickyObjectProperties>().StickToObject(this.gameObject);
            this.Volume += colObj.GetComponent<StickyObjectProperties>().Volume;

            //Distort our mesh
            DistortMesh(colObj);

            Debug.Log("Stuck object: " + colObj.name + " with volume: " + colObj.GetComponent<StickyObjectProperties>().Volume);
        }
    }

    /// <summary>
    /// Distorts our collision mesh to include the attached object
    /// </summary>
    /// <param name="attachedObject">The object we've attached to</param>
    /// <param name="distortType">An optional enum dictating the type of distortion to use, defaults to distort the closest point on our mesh to the origin of the object</param>
    protected virtual void DistortMesh(GameObject attachedObject, DistortType distortType = DistortType.DISTORT_TO_ORIGIN)
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

        switch (distortType)
        {
            case DistortType.DISTORT_TO_ORIGIN:
                {
                    //Distort our mesh to the origin of the attached object
                    newVertices[closestVertexIndex] = transform.InverseTransformPoint(attachedObject.transform.position);
                    break;
                }
            case DistortType.DISTORT_TO_FURTHEST_AWAY:
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
                    break;
                }
            default:
                {
                    throw new NotImplementedException();
                }
        }

        //Assign the new vertices to the old mesh
        mesh.vertices = newVertices;

        //Recalc the normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Assign the new mesh to the old mesh collider
        MeshCollider.sharedMesh = mesh;
    }

}
