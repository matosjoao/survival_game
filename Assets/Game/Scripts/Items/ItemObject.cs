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
        Debug.Log(itemData.displayName);
    }
}
