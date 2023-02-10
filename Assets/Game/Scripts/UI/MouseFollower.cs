using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private ItemSlotUI item;

    [Header("Components")]
    private Camera mainCam;
    private InputReader inputReader;

    [HideInInspector] public bool IsInventory { get; private set;} 
    [HideInInspector] public ItemSlotUI UISlot => item;

    private void Awake() 
    {
        mainCam = Camera.main;
        inputReader = GetComponent<InputReader>();
    }
    
    private void OnEnable() 
    {
        // Clear old data
        if(item != null)
            item.Clear();
        
        IsInventory = false;

        // Update to recent position
        UpdateSlotPosition();
    }

    private void Update() 
    {
        UpdateSlotPosition();
    }

    private void UpdateSlotPosition()
    {
        Vector2 position;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)canvas.transform,
            inputReader.MousePosition,
            canvas.worldCamera,
            out position
        );

        transform.position = canvas.transform.TransformPoint(position);
    }

    public void SetData(ItemSlot slot, bool isInventory = true)
    {
        item.Set(slot);
        IsInventory = isInventory;
    }

    public void Toggle(bool visible = false)
    {
        gameObject.SetActive(visible);
    }
}
