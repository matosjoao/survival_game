using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(Inventory))]
public class PlayerBuild : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float placementUpdateRate = 0.03f;
    [SerializeField] private float placementMaxDistance = 5.0f;
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private float rotationSpeed = 180.0f;

    [Header("Components")]
    private InputReader inputReader;
    private Inventory inventory;
    private Camera mainCam;

    private ItemData curItemData;
    private BuildingPreview curBuildingPreview;
    private float lastPlacementUpdateTime;
    private bool canPlace;
    private float curYRotation;
    
    private void Awake() 
    {
        mainCam = Camera.main;
        inputReader = GetComponent<InputReader>();
        inventory = GetComponent<Inventory>();
    }

    public void SetNewBuilding (ItemData item)
    {
        // Assing current data
        curItemData = item;
       
        // Instatiate preview building
        curBuildingPreview = Instantiate(item.previewPrefab).GetComponent<BuildingPreview>();
    }

    private void Update() 
    {
        if(curItemData != null && curBuildingPreview != null && Time.time - lastPlacementUpdateTime > placementUpdateRate)
        {
            lastPlacementUpdateTime = Time.time;

            // Shoot a raycast to where we're looking
            Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, placementMaxDistance, placementLayerMask))
            {
                curBuildingPreview.transform.position = hit.point;
                curBuildingPreview.transform.up = hit.normal;
                curBuildingPreview.transform.Rotate(new Vector3(0, curYRotation, 0), Space.Self);

                if(!curBuildingPreview.CollidingWithObjects())
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

        if(inputReader.IsRotating)
        {
            curYRotation += rotationSpeed * Time.deltaTime;

            if(curYRotation > 360.0f)
                curYRotation = 0.0f;
        } 
    }

    public void Build()
    {
        if(curItemData == null || curBuildingPreview == null || !canPlace)
            return;

        // Create object in world
        Instantiate(curItemData.spawnPrefab, curBuildingPreview.transform.position, curBuildingPreview.transform.rotation);

        // Remove from inventory
        inventory.OnActionReduceSelectedItemQuantity();
          
        // Reset 
        ResetData();
    }

    public void UnBuild()
    {
        ResetData();
    }

    private void ResetData()
    {
        if(curBuildingPreview != null)
            Destroy(curBuildingPreview.gameObject);
        
        curItemData = null;
        curBuildingPreview = null;
        canPlace = false;
        curYRotation = 0;
    }
}
