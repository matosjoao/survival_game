using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Inventory inventory;


    [Header("Properties")]
    [SerializeField] private float checkRate = 0.05f;
    [SerializeField] private float maxCheckDistance;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private TextMeshProUGUI promptText;

    private float lastCheckTime;
    private GameObject curInteractGameObject;
    private IInteractable curInteractable;
    private Camera cam;

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
    }

    private void Update() 
    {
        if(Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
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
                promptText.gameObject.SetActive(false);
            }
        }
    }

    private void SetPromptText()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = string.Format("<b>[E]</b> {0}", curInteractable.GetInteractPrompt());
    }

    private void OnInteract()
    {
        if(curInteractable == null) return;
        
        // Course way
        //curInteractable.OnInteract();

        ItemObject itemObject = curInteractGameObject.GetComponent<ItemObject>();
        inventory.AddItem(itemObject.Item);

        curInteractGameObject = null;
        curInteractable = null;
        promptText.gameObject.SetActive(false);
    }
}

// TODO:: Put in a seperated file
public interface IInteractable 
{
    string GetInteractPrompt();
    void OnInteract();
}
