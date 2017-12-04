using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class VRRemoteController : MonoBehaviour
{
    [SerializeField] private GameObject cursorPrefab;
    [SerializeField] private KatamariControllerVR katamariController;
    [SerializeField] private GameObject ballRemote;
    [SerializeField] private Vector3 cursorPositionOffset;

    [Header("Line Info")]
    [SerializeField] private float lineWidth;
    [SerializeField] private Color lineColour;

    private GameObject cursorInstance = null;
    private SteamVR_TrackedObject steamVRComponent;
    private SteamVR_Controller.Device steamVRController;
    private LineRenderer cursorLine;
    private bool cursorActive = false;

	/// <summary>
    /// Called at the start, initialises the needed variables
    /// </summary>
	private void Start ()
    {
		if (cursorPrefab == null)
        {
            Debug.LogError("ERROR: Cursor Prefab is null, please provide a prefab instance");
        }

        if (ballRemote == null)
        {
            Debug.LogError("ERROR: Ball Remote is null, please provide the ball remote instance");
        }

        steamVRComponent = GetComponent<SteamVR_TrackedObject>();

        //Get the Line Renderer, or make one if we dont have one
        cursorLine = GetComponent<LineRenderer>();
        if (cursorLine == null)
        {
            cursorLine = this.gameObject.AddComponent<LineRenderer>();
            cursorLine.enabled = false;
        }
        cursorLine.startWidth = cursorLine.endWidth = lineWidth;
        cursorLine.startColor = cursorLine.endColor = lineColour;
        cursorLine.material = new Material(Shader.Find("Sprites/Default"));

        steamVRController = SteamVR_Controller.Input((int)steamVRComponent.index);
    }
	
	/// <summary>
    /// Called every frame
    /// </summary>
	private void FixedUpdate()
    {
        HandleCursorState();
        MoveKatamari();
	}

    /// <summary>
    /// Determines whether the cursor is active or not
    /// </summary>
    private void HandleCursorState()
    {
        if (steamVRController.GetHairTriggerDown() && !cursorActive)
        {
            cursorInstance = Instantiate(cursorPrefab, transform);
            cursorInstance.transform.localPosition = cursorPositionOffset;
            cursorActive = true;
            cursorLine.enabled = true;
        }
        else if (steamVRController.GetHairTriggerUp() && cursorActive)
        {
            Destroy(cursorInstance);
            cursorActive = false;
            cursorLine.enabled = false;
        }
    }

    private void MoveKatamari()
    {
        if (!cursorActive)
        {
            return;
        }

        //Get the direction to move
        Vector3 moveDirection = cursorInstance.transform.position - ballRemote.transform.position;
        Vector3 projectedMoveDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up);
        projectedMoveDirection = projectedMoveDirection.normalized;

        //Draw the cursor line
        cursorLine.SetPositions(new Vector3[2] { cursorInstance.transform.position, ballRemote.transform.position });

        //Tell the katamari to roll
        katamariController.RollTowards(projectedMoveDirection);
    }
}
