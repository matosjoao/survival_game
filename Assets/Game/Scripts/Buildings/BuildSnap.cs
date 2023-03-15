using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSnap : MonoBehaviour
{
    [SerializeField] private List<SnapPoint> defaultSnapPoints = new List<SnapPoint>();
    [SerializeField] private bool isFloor;

    public List<SnapPoint> snapPoints = new List<SnapPoint>();

    private void Start() 
    {
        // Block
        /* defaultSnapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, 0.0f, 1.0f)), transform.TransformDirection(Vector3.forward)));
        defaultSnapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f)), transform.TransformDirection(Vector3.back)));
        defaultSnapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.5f, 0.0f, 0.5f)), transform.TransformDirection(Vector3.right)));
        defaultSnapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(-0.5f, 0.0f, 0.5f)), transform.TransformDirection(Vector3.left)));
        defaultSnapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, 0.5f, 0.5f)), transform.TransformDirection(Vector3.up)));
        defaultSnapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, -0.5f, 0.5f)), transform.TransformDirection(Vector3.down))); */

        //Floor
        if(isFloor)
        {
            defaultSnapPoints.Add(new SnapPoint(new Vector3(0.0f, 0.0f, 2.0f), Vector3.forward, new Vector3(0.0f, 0.0f, 0.0f), true, SnapType.Floor));
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(0.0f, 0.0f, 0.0f), Vector3.back, 
                new Vector3(0.0f, 0.0f, 0.0f), true, SnapType.Floor));
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(1.0f, 0.0f, 1.0f), Vector3.right, 
                new Vector3(0.0f, 0.0f, 0.0f), true, SnapType.Floor));
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(-1.0f, 0.0f, 1.0f), Vector3.left, 
                new Vector3(0.0f, 0.0f, 0.0f), true, SnapType.Floor));

            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(-0.95f, 0.05f, 1.0f), Vector3.up, new Vector3(0.0f, 0.0f, 0.0f), true, SnapType.Wall));//Left
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(0.95f, 0.05f, 1.0f), Vector3.up, new Vector3(0.0f, 0.0f, 0.0f), true, SnapType.Wall));//Right
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(0.0f, 0.05f, 1.95f), Vector3.up, new Vector3(0.0f, 0.0f, 0.0f), true, SnapType.Wall));//Forward
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(0.0f, 0.05f, 0.05f), Vector3.up, new Vector3(0.0f, 0.0f, 0.0f), true, SnapType.Wall));//Back
        }
        else
        {
            //Wall
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(1.0f, 1.0f, 0.0f), Vector3.right, 
                new Vector3(0.0f, 0.0f, 0.0f)));
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(-1.0f, 1.0f, 0.0f), Vector3.left, 
                new Vector3(0.0f, 0.0f, 0.0f)));
            defaultSnapPoints.Add(new SnapPoint(
                new Vector3(0.0f, 2.0f, 0.0f), Vector3.up, 
                new Vector3(0.0f, 0.0f, 0.0f)));
        }
    }

    public void UpdateSnapPoints()
    {   
        // Clear snap points
        snapPoints.Clear();

        // Add your snap points based on the current position and orientation of the object
        
        foreach (SnapPoint snapoint in defaultSnapPoints)
        {
            snapPoints.Add(new SnapPoint(
                transform.TransformPoint(snapoint.SnapPosition), 
                transform.TransformDirection(snapoint.SnapNormal),
                snapoint.SnapOffset,
                snapoint.Available,
                snapoint.SnapType
            ));
        }

        /* //Floor
        if(isFloor)
        {
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, 0.0f, 2.0f)), transform.TransformDirection(Vector3.forward), true, SnapType.Floor));
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, 0.0f, 0.0f)), transform.TransformDirection(Vector3.back), true, SnapType.Floor));

            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(1.0f, 0.0f, 1.0f)), transform.TransformDirection(Vector3.right), true, SnapType.Floor));
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(-1.0f, 0.0f, 1.0f)), transform.TransformDirection(Vector3.left), true, SnapType.Floor));

            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(-0.95f, 0.05f, 1.0f)), transform.TransformDirection(Vector3.up), true, SnapType.Wall));//Left
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.95f, 0.05f, 1.0f)), transform.TransformDirection(Vector3.up), true, SnapType.Wall));//Right
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, 0.05f, 1.95f)), transform.TransformDirection(Vector3.up), true, SnapType.Wall));//Forward
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, 0.05f, 0.05f)), transform.TransformDirection(Vector3.up), true, SnapType.Wall));//Back
        }
        else
        {
            //Wall
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(1.0f, 1.0f, 0.0f)), transform.TransformDirection(Vector3.right)));
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(-1.0f, 1.0f, 0.0f)), transform.TransformDirection(Vector3.left)));
            snapPoints.Add(new SnapPoint(transform.TransformPoint(new Vector3(0.0f, 2.0f, 0.0f)), transform.TransformDirection(Vector3.up)));
        } */
    }

    private void OnDrawGizmos()
    {
        foreach (SnapPoint snapPoint in snapPoints)
        {
            Gizmos.color = snapPoint.Available ? Color.green : Color.red;
            Gizmos.DrawSphere(snapPoint.SnapPosition, 0.1f);
            Gizmos.DrawLine(snapPoint.SnapPosition, snapPoint.SnapPosition + snapPoint.SnapNormal);
        }
    }
}


[System.Serializable]
public class SnapPoint
{
    private Vector3 snapPosition;
    private Vector3 snapNormal;
    private Vector3 snapOffset;
    private bool available;
    private SnapType snapPointType;

    public Vector3 SnapPosition => snapPosition;
    public Vector3 SnapNormal => snapNormal;
    public Vector3 SnapOffset => snapOffset;
    public bool Available => available;
    public SnapType SnapType => snapPointType;

    public SnapPoint(Vector3 position, Vector3 normal, Vector3 offset, bool visible = true, SnapType pointType = SnapType.General)
    {
        snapPosition = position;
        snapNormal = normal;
        snapOffset = offset;
        available = visible;
        snapPointType = pointType;
    }

    public void SetAvailability(bool value)
    {
        available = value;
    }
}

[System.Serializable]
public enum SnapType
{
    General,
    Wall,
    Door,
    Ramp,
    Floor
}

