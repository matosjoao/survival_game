using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Inventory))]
public class PlayerBuild : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float placementUpdateRate = 0.03f;
    [SerializeField] private float placementMaxDistance = 5.0f;
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private float rotationSpeed = 180.0f;
    [SerializeField] private LayerMask buildLayer;

    [Header("Components")]
    private InputReader inputReader;
    private PlayerController playerController;
    private Inventory inventory;
    private Camera mainCam;

    private ItemData curItemData;
    private BuildingPreview curBuildingPreview;
    private float lastPlacementUpdateTime;
    private bool canPlace;
    private float curYRotation;
    // Way1:: private SnapPoint currentSnapPoint;

    [Header("Build Progress Bar")]
    private float curProgressTime;
    private bool curInProgress = false;
    private float curProgress;
    private BuildPreviewProgress bPreviewProgress;

    [Header("Foundation Properties")]
    private float foundationHeight = 0.4f;
    private float originalBaseFoudationSize = 0.2f;
    private BuildPreviewFoundation curFoundation;

    private void Awake() 
    {
        // Get components
        inputReader = GetComponent<InputReader>();
        inventory = GetComponent<Inventory>();
        playerController = GetComponent<PlayerController>();

        mainCam = Camera.main;
    }

    private void OnEnable() 
    {
        // Subscribe to events
        inputReader.MouseLeftEvent += HandleBuild;
        inputReader.MouseRightEvent += HandleUnBuild;
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.MouseLeftEvent -= HandleBuild;
        inputReader.MouseRightEvent -= HandleUnBuild;
    }

    public void StartBuilding(ItemData item)
    {
        ResetData();

        // Assing current data
        curItemData = item;
       
        // Instatiate preview building
        // TODO:: Change to pool
        curBuildingPreview = Instantiate(item.previewPrefab).GetComponent<BuildingPreview>();
    }

    public void StopBuilding()
    {
        ResetData();
    }

    private void Update() 
    {
        if(inputReader.IsAttaking && curInProgress)
        {
            // Update progress
            curProgress += Time.deltaTime / curProgressTime;
            bPreviewProgress.UpdateProgress(curProgress);

            // Is progress done?
            if(curProgress < 1.0f)
                return;

            // Reset preview
            curInProgress = false;
            bPreviewProgress.ResetProgress();
            bPreviewProgress = null;

            // Build
            Build();
        }
        else if(curInProgress)
        {
            ResetProgress();
        }
        
        // Set up preview build object
        if(curItemData != null && curBuildingPreview != null && Time.time - lastPlacementUpdateTime > placementUpdateRate)
        {
            lastPlacementUpdateTime = Time.time;
            // Way1:: with no snap points gameObjecs and colliders
            //currentSnapPoint = null;

            // If is snap and founded the snap point
            bool foudSnapPoint = true;
            if(curItemData.canSnap && (curItemData.buildType == SnapType.Wall || curItemData.buildType == SnapType.Floor))
            {
                foudSnapPoint = false;
            }

            // Shoot a raycast to where we're looking
            Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, placementMaxDistance, placementLayerMask))
            {
                // Build normal way
                curBuildingPreview.transform.position = hit.point;
                //curBuildingPreview.transform.up = hit.normal;
                //curBuildingPreview.transform.Rotate(new Vector3(0, curYRotation, 0), Space.Self);


                // Is foundation?
                // Adjust the foundation position to match the terrain height
                if(curItemData.canSnap && curItemData.buildType == SnapType.Foundation)
                {
                    float deltaY = inputReader.MouseDelta.y;
                    foundationHeight += deltaY * 1.0f * Time.deltaTime;
                    if (foundationHeight < 0.0f)
                    {
                        foundationHeight = 0.0f;
                    }

                    if (foundationHeight > 2.0f)
                    {
                        foundationHeight = 2.0f;
                    }

                    Vector3 foundationPosition = hit.point;
                    float terrainHeight = hit.point.y;
                    foundationPosition.y = terrainHeight + foundationHeight;

                    curBuildingPreview.transform.position = foundationPosition;

                    // Get current foundation
                    if(curFoundation == null)
                    {
                        curFoundation = curBuildingPreview.GetComponent<BuildPreviewFoundation>();
                    }
                    if(curFoundation.SupportFoundations != null)
                    {
                        float distanceToFloor = Vector3.Distance(hit.point, curBuildingPreview.transform.position);
                        
                        foreach (GameObject supportFoundation in curFoundation.SupportFoundations)
                        {
                            float totalHeight = distanceToFloor + supportFoundation.transform.localScale.y;
                            float scaleFactor = totalHeight / originalBaseFoudationSize;

                            supportFoundation.transform.localScale = new Vector3(supportFoundation.transform.localScale.x, originalBaseFoudationSize * scaleFactor , supportFoundation.transform.localScale.z);
                        }
                    }
                }

                // Has snap component?
                BuildSnap buildSnap = hit.transform.gameObject.GetComponent<BuildSnap>();
                if (buildSnap != null)
                {
                    // Get closest snap point
                    BuildSnapPoint closestSnapPoint = GetClosestSnapPoint(buildSnap.SnapPoints, hit.point, curItemData.buildType);
                    if(closestSnapPoint != null)
                    {
                        // Is Floor
                        // Check if the wall was created in center or normal position
                        // Set up ajust offset
                        // 0.1f is walf of floor height
                        Vector3 ajustFloorOffset = Vector3.zero;
                        if(curItemData.buildType == SnapType.Floor && buildSnap.SnappedFromCenter)
                        {
                            ajustFloorOffset = 0.1f * closestSnapPoint.transform.forward;
                        }

                        // Transform in positions and directions
                        //Vector3 snapPosition = closestSnapPoint.transform.position; TODO:: DELETE
                        Vector3 snapPosition = closestSnapPoint.SnapPosition;
                        Quaternion snapRotation = Quaternion.LookRotation(closestSnapPoint.transform.forward, Vector3.up);
                        
                        // Snap this object to the closest snap point
                        curBuildingPreview.transform.position = snapPosition + ajustFloorOffset;
                        curBuildingPreview.transform.rotation = snapRotation;

                        foudSnapPoint = true;
                    } 
                }
            }

            // Is Snap?
            // TODO:: Delete - Old Snap 
            /* if(Physics.Raycast(ray, out RaycastHit hit2, placementMaxDistance, buildLayer))
            {
                BuildSnap buildSnap = hit2.transform.gameObject.GetComponent<BuildSnap>();
                if (buildSnap != null)
                {
                    BuildSnapPoint closestSnapPoint = GetClosestSnapPoint(buildSnap.snapPoints, hit2.point, curItemData.snapType);
                    if(closestSnapPoint != null)
                    {
                        // Transform in positions and directions
                        Vector3 snapPosition = closestSnapPoint.transform.position;
                        Quaternion snapRotation = Quaternion.LookRotation(closestSnapPoint.transform.forward, Vector3.up);
                        
                        // Snap this object to the closest snap point
                        curBuildingPreview.transform.position = snapPosition;
                        curBuildingPreview.transform.rotation = snapRotation;

                        foudSnapPoint = true;
                    } */

                    // Way1:: with no snap points gameObjecs and colliders
                    // Find the closest snap point on the hit object
                    /* SnapPoint closestSnapPoint = GetClosestSnapPoint(buildSnap.snapPoints, hit2.point, curItemData.snapType);
                    if(closestSnapPoint != null)
                    {
                        // Transform in positions and directions
                        Vector3 snapPosition = closestSnapPoint.SnapPosition;
                        Quaternion snapRotation = Quaternion.LookRotation(closestSnapPoint.SnapNormal, Vector3.up);

                        // Snap this object to the closest snap point
                        curBuildingPreview.transform.position = snapPosition + closestSnapPoint.SnapOffset;
                        curBuildingPreview.transform.rotation = snapRotation;
                    }
                    currentSnapPoint = closestSnapPoint; */
                /* }
            } */

            // Set materials
            if(!curBuildingPreview.CollidingWithObjects() && foudSnapPoint)
            {
                if(!canPlace)
                    curBuildingPreview.CanPlace();

                canPlace = true;
            }
            else
            {
                if(canPlace)
                    curBuildingPreview.CannotPlace();

                canPlace = false;
            }
        }

        // Build Rotation
        if(inputReader.IsRotating)
        {
            curYRotation += rotationSpeed * Time.deltaTime;

            if(curYRotation > 360.0f)
                curYRotation = 0.0f;
        } 
    }

    private void HandleBuild()
    {
        if(curItemData == null || curBuildingPreview == null || !canPlace)
            return;

        // Is interacting ? 
        if(playerController.IsInteracting)
            return;
        
        // Has time to build
        bPreviewProgress = curBuildingPreview.GetComponent<BuildPreviewProgress>();
        if(bPreviewProgress != null)
        {
            curProgressTime = 1.0f; // TODO:: Change to build time
            curInProgress = true;
            curProgress = 0.0f;
        }
        else
        {
            Build();
        }
    }

    private void Build()
    {
        // Create object in world
        GameObject obj = Instantiate(curItemData.spawnPrefab, curBuildingPreview.transform.position, curBuildingPreview.transform.rotation);

        // Is foundation ?    
        if(curItemData.buildType == SnapType.Foundation && obj.TryGetComponent<BuildPreviewFoundation>(out BuildPreviewFoundation buildPreviewFoundation))
        {
            if(buildPreviewFoundation.SupportFoundations != null)
            {
                foreach (GameObject baseObj in buildPreviewFoundation.SupportFoundations)
                {
                    baseObj.transform.localScale = curFoundation.SupportFoundations[0].transform.localScale;
                }
            }
        }

        /* 
        // Way1:: with no snap points gameObjecs and colliders
        // Is build snap add snap points
        if(obj.TryGetComponent<BuildSnap>(out BuildSnap bSnap))
        {
            //Update snap point
            bSnap.UpdateSnapPoints(currentSnapPoint != null ? false : true);

            // Set snap point as unavailable
            if(currentSnapPoint !=null) 
            {
                currentSnapPoint.SetAvailability(false);
                currentSnapPoint = null;
            };
        } 
        */

        // Remove from inventory, is empty?
        if(inventory.OnActionReduceSelectedItemQuantity())
        {
            // Reset 
            ResetData();
        }
        
        // Reset 
        //ResetData();
        canPlace = false;
    }

    private void HandleUnBuild()
    {

    }

    private BuildSnapPoint GetClosestSnapPoint(List<BuildSnapPoint> snapPoints, Vector3 hitPoint, SnapType pointType)
    {
        float closestDistance = float.MaxValue;
        BuildSnapPoint closestSnapPoint = null;

        foreach (BuildSnapPoint sp in snapPoints)
        {
            if(sp.IsAvailable && sp.SnapType == pointType)
            {
                // Calculate the distance between the snap point and the given point
                float distance = Vector3.Distance(sp.transform.position, hitPoint);

                // Check if this snap point is closer than the current closest snap point
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSnapPoint = sp;
                }
            }
        }

        return closestSnapPoint;
    }

    /* 
    // Way1:: with no snap points gameObjecs and colliders
    private SnapPoint GetClosestSnapPoint(List<SnapPoint> snapPoints, Vector3 hitPoint, SnapType pointType)
    {
        float closestDistance = float.MaxValue;
        SnapPoint closestSnapPoint = null;

        foreach (SnapPoint sp in snapPoints)
        {
            if(sp.Available && sp.SnapType == pointType)
            {
                // Calculate the distance between the snap point and the given point
                float distance = Vector3.Distance(sp.SnapPosition, hitPoint);

                // Check if this snap point is closer than the current closest snap point
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestSnapPoint = sp;
                }
            }
        }

        return closestSnapPoint;
    } 
    */

    private void ResetData()
    {
        if(curBuildingPreview != null)
        {
            // TODO:: Change to pool
            Destroy(curBuildingPreview.gameObject);
        }
        
        curItemData = null;
        curBuildingPreview = null;
        canPlace = false;
        curYRotation = 0;
        curFoundation = null;
    }

    private void ResetProgress()
    {
        // Reset variables
        curInProgress = false;
        curProgress = 0.0f;

        bPreviewProgress.ResetProgress();
        bPreviewProgress = null;
    }
}
