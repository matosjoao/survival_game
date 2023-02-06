using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private ItemSlotUI item;

    private Camera mainCam;
    private InputReader inputReader;

    private void Awake() 
    {
        mainCam = Camera.main;
        inputReader = GetComponent<InputReader>();
    }
    
    private void OnEnable() 
    {
        // Clear old data
        item.Clear();

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

    public void SetData(ItemSlot slot)
    {
        item.Set(slot);
    }

    public void Toggle(bool visible = false)
    {
        gameObject.SetActive(visible);
    }
}
