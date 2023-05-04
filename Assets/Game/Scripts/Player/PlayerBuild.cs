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

    [Header("Foundation Properties")]
    [SerializeField] private float foundationHeight = 0.2f;
    [SerializeField] private float foundationMovement = 1.0f;
    [SerializeField] private float maxFoundationHeight = 3.0f;

    [Header("Components")]
    private InputReader inputReader;
    private PlayerController playerController;
    private Inventory inventory;
    private Camera mainCam;

    private ItemData curItemData;
    private BuildingPreview curBuildingPreview;
    private float lastPlacementUpdateTime;
    private bool canPlace;

    [Header("Build Progress Bar")]
    private float curProgressTime;
    private bool curInProgress = false;
    private float curProgress;
    private BuildPreviewProgress bPreviewProgress;

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
        inputReader.RotateEvent += HandleRotate;
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        inputReader.MouseLeftEvent -= HandleBuild;
        inputReader.MouseRightEvent -= HandleUnBuild;
        inputReader.RotateEvent -= HandleRotate;
    }

    private void HandleRotate()
    {
        if(curItemData != null && curBuildingPreview != null && !curItemData.canSnap)
        {
            curBuildingPreview.transform.Rotate(Vector3.up, 90f);
        }
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
        if(inputReader.IsPressingLeftMouse && curInProgress)
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
                    // Get mouse delta y
                    float deltaY = inputReader.MouseDelta.y;

                    // Ajust the preview object height based on mouse y delta
                    foundationHeight += deltaY * foundationMovement * Time.deltaTime;
                    
                    // Ajust min height
                    foundationHeight = Mathf.Max(foundationHeight, 0.0f);

                    // Move the preview object
                    Vector3 foundationPosition = hit.point;
                    float terrainHeight = hit.point.y;
                    foundationPosition.y = Mathf.Min(terrainHeight + foundationHeight, maxFoundationHeight);
                    curBuildingPreview.transform.position = foundationPosition;
                }

                // Has snap component?
                BuildSnap buildSnap = hit.transform.gameObject.GetComponent<BuildSnap>();
                if (buildSnap != null)
                {
                    // Get closest snap point
                    BuildSnapPoint closestSnapPoint = GetClosestSnapPoint(buildSnap.SnapPoints, hit.point, curItemData.buildType);
                    if(closestSnapPoint != null)
                    {
                        // Transform in positions and directions
                        Vector3 snapPosition = closestSnapPoint.SnapPosition;
                        
                        // Rotate the preview object base on the closest snap point
                        Quaternion snapRotation = Quaternion.LookRotation(closestSnapPoint.transform.forward, Vector3.up);
                        
                        // Snap this object to the closest snap point
                        curBuildingPreview.transform.rotation = snapRotation;

                        // Ajust offset base on the wall position
                        float totalAjustOffset = 0.0f;
                        if(curItemData.buildType == SnapType.Floor && buildSnap.BuildType == SnapType.Wall)
                        {
                            if(buildSnap.SnappedFromCenter)
                            {
                                totalAjustOffset += closestSnapPoint.SnapOffset/2;
                            }
                            else
                            {
                                totalAjustOffset += closestSnapPoint.SnapOffset;
                            }
                        }

                        // Set up offset in a Vector3
                        Vector3 ajustFloorOffset = Vector3.zero;
                        ajustFloorOffset = totalAjustOffset * closestSnapPoint.transform.forward;

                        // Move the preview object base on the closest snap point and offset
                        curBuildingPreview.transform.position = snapPosition + ajustFloorOffset;

                        // A snap point was founded, set the bool to true
                        foudSnapPoint = true;
                    } 

                    // TODO:: Move to a hammer tool, and their we can repair or remove
                    if(inputReader.IsPressingRightMouse)
                    {
                        // Move to reset snap points
                        //hit.transform.position = new Vector3(0, 500, 0);

                        // Release it
                        Destroy(hit.transform.gameObject/* , 2.0f */);
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
