using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    [Header("Player Needs")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image hungerBar;
    [SerializeField] private Image thirstBar;

    public UnityEvent onTakeDamage;

    [Header("Player Interaction")]
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Player Inventory")]
    [SerializeField] private GameObject inventoryWindow;
    [SerializeField] private ItemSlotUI[] uiSlots;

    [SerializeField] private GameObject useButton;
    [SerializeField] private GameObject equipButton;
    [SerializeField] private GameObject unEquipButton;
    [SerializeField] private GameObject dropButton;

    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;

    #region Player Needs
    public void UpdateNeedsUI(float health, float hunger, float thirst)
    {
        healthBar.fillAmount = health;
        hungerBar.fillAmount = hunger;
        thirstBar.fillAmount = thirst;
    }

    public void TakePhisicalDamage()
    {
        onTakeDamage?.Invoke();
    }
    #endregion

    #region Player Interaction
    public void SetPromptText(bool visible, string text = "")
    {
        if(visible)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = text;
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Player Inventory
    public void ToggleInventoryWindow(bool visible = false)
    {
        inventoryWindow.SetActive(visible);
    }

    public bool IsInventoryOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    public int GetInventorySize()
    {
        return uiSlots.Length;
    }

    public void ToggleInventoryButtons(ItemType type, int itemIndex)
    {
        useButton.SetActive(type == ItemType.Consumable);
        equipButton.SetActive(type == ItemType.Equipable && !uiSlots[itemIndex].equipped);
        unEquipButton.SetActive(type == ItemType.Equipable && uiSlots[itemIndex].equipped);
        dropButton.SetActive(true);
    }

    public void DisableInventoryButtons()
    {
        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void InitializeUISlots(ItemSlot[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            uiSlots[i].index = i;
            uiSlots[i].Clear();
        }
    }

    public void UpdateInventorySlots(ItemSlot[] slots)
    {
        // Update UI Slots
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item != null)
            {
                uiSlots[i].Set(slots[i]);
            }
            else
            {
                uiSlots[i].Clear();
            }
        }
    }

    public void OnUseButton()
    {
        // TODO:: Try to solve this without singleton
        Inventory.Instance.OnUseButton(); 
    }
    public void OnEquipButton()
    {
        // TODO:: Try to solve this without singleton
        Inventory.Instance.OnEquipButton();
    }

    public void OnUnEquipButton()
    {
        // TODO:: Try to solve this without singleton
        Inventory.Instance.OnUnEquipButton();
    }

    public void OnDropButton()
    {
        // TODO:: Try to solve this without singleton
        Inventory.Instance.OnDropButton();
    }
    #endregion
}
