using System.Collections.Generic;
using UnityEngine;

public class BuildSnapPoint : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private SnapType snapType;
    [SerializeField] private SnapType parentObjectType;

    public bool IsAvailable { get; private set;}
    public SnapType SnapType => snapType;
    public Vector3 SnapPosition { get; private set;}
    public bool SnappedFromCenter { get; private set;}

    public List<BuildSnapPoint> snappedPoints = new List<BuildSnapPoint>();
    private BoxCollider boxCollider;

    private void Awake() 
    {
        // Get components
        boxCollider = GetComponent<BoxCollider>();

        // SetVariables
        SetAvailability(true);
        SnapPosition = transform.position;
        SnappedFromCenter = false;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.TryGetComponent<BuildSnap>(out BuildSnap buildSnap))
        {
            if(buildSnap.BuildType == SnapType.Wall)
            {
                // Desactivate this snap point
                SetAvailability(false);

                buildSnap.SetSnappedFromCenter(SnappedFromCenter);
            }
        }

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
                return;
            }

            // Desactivate this snap point
            SetAvailability(false);

            // Desactivate the connected snap point
            //snapPoint.SetAvailability(false);

            // Add snap point to list
            snappedPoints.Add(snapPoint);
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if(other.TryGetComponent<BuildSnap>(out BuildSnap buildSnap))
        {
            if(buildSnap.BuildType == SnapType.Wall)
            {
                // Desactivate this snap point
                SetAvailability(true);

                buildSnap.SetSnappedFromCenter(SnappedFromCenter);
            }
        }

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
                return;
            }

            // Activate this snap point
            SetAvailability(true);

            // Activate the connected snap point
            //snapPoint.SetAvailability(true);

            // Remove snap point to list
            snappedPoints.Remove(snapPoint);
        }
    }

    public void SetAvailability(bool value)
    {
        IsAvailable = value;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = IsAvailable ? Color.green : Color.red;
        Gizmos.DrawCube(transform.position, new Vector3(0.05f, 0.05f, 0.05f));
    }
}
