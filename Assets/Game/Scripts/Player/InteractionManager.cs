using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float checkRate = 0.05f;
    [SerializeField] private float maxCheckDistance;
    [SerializeField] private LayerMask layerMask;

    [Header("Components")]
    private InputReader inputReader;
    private PlayerController playerController;
    private Transform cameraTransform;

    private float lastCheckTime;
    private GameObject curInteractGameObject;
    private IInteractable curInteractable;
   

    private void OnEnable() 
    {
        inputReader.InteractEvent += OnInteract;
    }

    private void OnDisable() 
    {
        inputReader.InteractEvent -= OnInteract;
    }

    private void Awake() 
    {
        inputReader = GetComponent<InputReader>();
        playerController = GetComponent<PlayerController>();
    }

    private void Start() 
    {
        cameraTransform = Camera.main.gameObject.transform;
    }

    private void Update() 
    {   
        // If can't interact return
        if(playerController.IsInteracting)
            return;

        if(Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            // Old version to delete
            //Ray ray = mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            //RaycastHit hit;
            //if(Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxCheckDistance, layerMask))
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
        // If dont exist current interactable
        if(curInteractable == null) return;
        
        // If can't interact return
        if(playerController.IsInteracting)
            return;

        // Interact
        curInteractable.OnInteract(playerController);

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
    void OnInteract(PlayerController playerController);
}
