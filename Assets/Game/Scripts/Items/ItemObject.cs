using UnityEngine;

public class ItemObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    public ItemData Item => itemData;

    public string GetInteractPrompt()
    {
        return string.Format("Pickup {0}", itemData.displayName);
    }

    public void OnInteract(PlayerController playerController)
    {
        // Get Inventory
        Inventory pInventory = playerController.GetComponent<Inventory>();
        if(pInventory == null)
            return;

        // Try add item to inventory
        pInventory.AddItem(itemData);

        // TODO:: Improve change to pool
        Destroy(gameObject);
    }
}
