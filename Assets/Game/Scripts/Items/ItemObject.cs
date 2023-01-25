using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    public ItemData Item => itemData;

    public string GetInteractPrompt()
    {
        return string.Format("Pickup {0}", itemData.displayName);
    }

    public void OnInteract()
    {
        // Try add item to inventory
        Inventory.Instance.AddItem(itemData);

        // TODO:: Improve change to pool
        Destroy(gameObject);
    }
}
