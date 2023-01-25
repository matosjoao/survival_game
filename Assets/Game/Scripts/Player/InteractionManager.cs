using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Inventory inventory;


    [Header("Properties")]
    [SerializeField] private float checkRate = 0.05f;
    [SerializeField] private float maxCheckDistance;
    [SerializeField] private LayerMask layerMask;

    private float lastCheckTime;
    private GameObject curInteractGameObject;
    private IInteractable curInteractable;
    private Camera cam;
    private Transform cameraTransform;

    private void OnEnable() 
    {
        inputReader.InteractEvent += OnInteract;
    }

    private void OnDisable() 
    {
        inputReader.InteractEvent -= OnInteract;
    }

    private void Start() 
    {
        cam = Camera.main;

        cameraTransform = cam.gameObject.transform;
    }

    private void Update() 
    {
        if(Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            //if(Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxCheckDistance, layerMask))
            {
                if(hit.collider.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText();
                }
            }
            else
            {
                curInteractGameObject = null;
                curInteractable = null;
                UIManager.Instance.SetPromptText(false);
            }
        }
    }

    private void SetPromptText()
    {
        UIManager.Instance.SetPromptText(true, string.Format("<b>[E]</b> {0}", curInteractable.GetInteractPrompt()));
    }

    private void OnInteract()
    {
        if(curInteractable == null) return;
        
        // TODO:: Delete - Course way
        curInteractable.OnInteract();

        /* // Get current interacted object ItemObject component
        ItemObject itemObject = curInteractGameObject.GetComponent<ItemObject>();

        // TODO:: Improve change to pool
        Destroy(curInteractGameObject);

        // Try add item to inventory
        inventory.AddItem(itemObject.Item); */

        // Reset interaction
        curInteractGameObject = null;
        curInteractable = null;
        UIManager.Instance.SetPromptText(false);
    }
}

// TODO:: Put in a seperated file
public interface IInteractable 
{
    string GetInteractPrompt();
    void OnInteract();
}
