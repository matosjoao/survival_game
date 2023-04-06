using System.Collections.Generic;
using UnityEngine;

public class BuildSnapPoint : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private SnapType snapType;
    [SerializeField] private float snapOffset;

    public SnapType SnapType => snapType;
    public float SnapOffset => snapOffset;

    public bool IsAvailable { get; private set;}
    public Vector3 SnapPosition { get; private set;}
    public bool SnappedFromCenter { get; private set;}
    
    public bool IsAvailableBeta; // TODO:: Delete
    
    private BoxCollider boxCollider;

    private void Awake() 
    {
        // Get components
        boxCollider = GetComponent<BoxCollider>();

        // Set availability
        SetAvailability(true);

        // Set snap position
        SnapPosition = transform.position;

        // Reset snapFromCenter
        SnappedFromCenter = false;
    }

    private void OnTriggerEnter(Collider other) 
    {
        // Check if a snap point is merged with another snap point of the same type
        // If yes and the snap type is a wall it means that a wall was build in the center of both snap points
        // Set the position to snap n the center of the colliders
        // Set snapFromCenter to true
        if(other.TryGetComponent<BuildSnapPoint>(out BuildSnapPoint snapPoint))
        {
            // Is the same type
            if(SnapType != snapPoint.SnapType)
                return;

            // If they are collided
            if(snapType == SnapType.Wall)
            {
                SnapPosition = boxCollider.bounds.center;
                SnappedFromCenter = true;
            }
        }

        if(other.TryGetComponent<BuildSnap>(out BuildSnap buildSnap))
        {
            if(buildSnap.BuildType == SnapType)
            {
                // Desactivate this snap point
                SetAvailability(false);

                // If we built a wall ?
                if(buildSnap.BuildType == SnapType.Wall)
                {
                    buildSnap.SetSnappedFromCenter(SnappedFromCenter);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if(other.TryGetComponent<BuildSnapPoint>(out BuildSnapPoint snapPoint))
        {
            // Is the same type
            if(SnapType != snapPoint.SnapType)
                return;

            // If they are collided
            if(snapType == SnapType.Wall)
            {
                SnapPosition = transform.position;
                SnappedFromCenter = false;
            }
        }

        if(other.TryGetComponent<BuildSnap>(out BuildSnap buildSnap))
        {
            if(buildSnap.BuildType == SnapType)
            {
                // Activate this snap point
                SetAvailability(true);

                // If we built a wall ?
                if(buildSnap.BuildType == SnapType.Wall)
                {
                    buildSnap.SetSnappedFromCenter(SnappedFromCenter);
                }
            }
        }
    }

    private void SetAvailability(bool value)
    {
        IsAvailable = value;
        IsAvailableBeta = value;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = IsAvailable ? Color.green : Color.red;
        Gizmos.DrawCube(transform.position, new Vector3(0.05f, 0.05f, 0.05f));
    }
}
