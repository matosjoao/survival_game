using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildSnapPoint : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private SnapType snapType;
    [SerializeField] private float snapOffset;
    [SerializeField] private bool defaultSnapFromCenter;
    [SerializeField] private BuildSnapPoint parentSnapPoint;

    [Header("Components")]
    private BoxCollider boxCollider;

    public SnapType SnapType => snapType;
    public float SnapOffset => snapOffset;
    public bool IsAvailable { get; private set;}
    public Vector3 SnapPosition { get; private set;}
    public bool SnappedFromCenter { get; private set;}

    private void Awake() 
    {
        // Get components
        boxCollider = GetComponent<BoxCollider>();

        // Set availability
        SetAvailability(true);

        // Set snap position
        SnapPosition = transform.position;

        // FIQUEI AQUI
        // Reset snap from center
        SnappedFromCenter = defaultSnapFromCenter;
        if(defaultSnapFromCenter)
        {
            SnapPosition = boxCollider.bounds.center;
        }
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

                buildSnap.OnBuildDestroyed += HandleOnBuildDestroyed;

                // If we built a wall ?
                if(buildSnap.BuildType == SnapType.Wall)
                {
                    buildSnap.SetSnappedFromCenter(SnappedFromCenter);

                    // FIQUEI AQUI
                    // HÁ MANEIRA DE CONTORNAR ISTO  SERÁ UMA BOA OPÇÃO??? 
                    if(!SnappedFromCenter && parentSnapPoint)
                    {
                        parentSnapPoint.SetAvailability(false);
                    }
                }
            }
        }
    }

    private void HandleOnBuildDestroyed(BuildSnap obj)
    {
        SetAvailability(true);
        
        obj.OnBuildDestroyed -= HandleOnBuildDestroyed;
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

                buildSnap.OnBuildDestroyed -= HandleOnBuildDestroyed;

                // If we built a wall ?
                if(buildSnap.BuildType == SnapType.Wall)
                {
                    buildSnap.SetSnappedFromCenter(SnappedFromCenter);
                }
            }
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
