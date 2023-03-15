using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;

public class UIManager : Singleton<UIManager>
{
    [Header("Player Needs")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image hungerBar;
    [SerializeField] private Image thirstBar;
    public UnityEvent onTakeDamage;

    [Header("Player Interaction")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Slider actionSlider;

    [Header("Player Inventory")]
    [SerializeField] private GameObject inventoryWindow;
    [SerializeField] private RectTransform inventorySlotsParent;
    [SerializeField] private RectTransform quickSlotsParent;
    [SerializeField] private ItemSlotUI inventorySlotUIPrefab;
    [SerializeField] private QuickSlotUI quickSlotUIPrefab;
    [SerializeField] private MouseFollower inventorySlotUIDraggable;
    [SerializeField] private Dropzone droppablezone;

    [Header("Player Base Craft")]
    [SerializeField] private TextMeshProUGUI craftItemName;
    [SerializeField] private Image craftItemIcon;
    [SerializeField] private GameObject craftButton;
    [SerializeField] private TextMeshProUGUI craftResourcesCosts;
    [SerializeField] private CraftingRecipeUI[] craftRecipeUIs;
    [SerializeField] private GameObject baseCraftWindow;

    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;
    private List<ItemSlotUI> uiSlots = new List<ItemSlotUI>();
    private List<QuickSlotUI> quickUISlots = new List<QuickSlotUI>();

    private void OnEnable() 
    {
        // Subscribe to events
        droppablezone.OnItemDroppedOnDropZone += HandleItemDroppedOnDropZone;

        foreach (ItemSlotUI uiSlot in uiSlots)
        {
            
            uiSlot.OnItemBeginDrag += HandleItemBeginDrag;
            uiSlot.OnItemEndDrag += HandleItemEndDrag;
            uiSlot.OnItemDroppedOn += HandleItemDrop;
        }

        foreach (QuickSlotUI quickUISlot in quickUISlots)
        {
            quickUISlot.OnItemDroppedOnQuickSlot += HandleItemDropQuickSlot;
        }
    }

    private void OnDisable() 
    {
        // Unsubscribe to events
        droppablezone.OnItemDroppedOnDropZone -= HandleItemDroppedOnDropZone;

        foreach (ItemSlotUI uiSlot in uiSlots)
        {
            
            uiSlot.OnItemBeginDrag -= HandleItemBeginDrag;
            uiSlot.OnItemEndDrag -= HandleItemEndDrag;
            uiSlot.OnItemDroppedOn -= HandleItemDrop;
        }

        foreach (QuickSlotUI quickUISlot in quickUISlots)
        {
            quickUISlot.OnItemDroppedOnQuickSlot -= HandleItemDropQuickSlot;
        }
    }

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

    public void ShowSlider()
    {
        actionSlider.gameObject.SetActive(true);
    }

    public void HideSlider()
    {
        actionSlider.value = 0.0f;
        actionSlider.gameObject.SetActive(false);
    }

    public void UpdateActionSlider(float value)
    {
        actionSlider.value = value;
    }
    #endregion

    #region Player Inventory
    public void ToggleInventoryWindow(bool visible = false, bool isInv = false)
    {
        inventoryWindow.SetActive(visible);

        baseCraftWindow.SetActive(isInv);
    }

    public bool IsInventoryOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    public void InitializeUISlots(int size)
    {
        for (int i = 0; i < size; i++)
        {
            ItemSlotUI uiSlot = Instantiate(inventorySlotUIPrefab, Vector3.zero, Quaternion.identity);
            uiSlot.transform.SetParent(inventorySlotsParent);
            
            uiSlot.Clear();
            uiSlot.SetIndex(i);

            uiSlots.Add(uiSlot);
        }
    }

    private void HandleItemDrop(ItemSlotUI itemSlotUI)
    {
        if(inventorySlotUIDraggable.IsInventory)
        {
            // From inventory to inventory
            itemSlotUI.CurrentItemSlot.SwapItems(inventorySlotUIDraggable.UISlot.Index);
        }
        else
        {
            // From storage to inventory
            itemSlotUI.CurrentItemSlot.AddItem(inventorySlotUIDraggable.UISlot.Index);
        }

        // Hide draggable slot
        inventorySlotUIDraggable.Toggle(false);
    }

    private void HandleItemBeginDrag(ItemSlotUI itemSlotUI)
    {   
        // Slot has item
        if(itemSlotUI.CurrentItemSlot == null)
            return;
        
        // Show draggable slot in mouse position
        inventorySlotUIDraggable.Toggle(true);
        inventorySlotUIDraggable.SetData(itemSlotUI.CurrentItemSlot, itemSlotUI.Index);
    }

    private void HandleItemEndDrag(ItemSlotUI itemSlotUI)
    {  
        inventorySlotUIDraggable.Toggle(false);
    }

    public void UpdateInventorySlots(ItemSlot[] slots)
    {
        ClearUISlots();

        // Update UI Slots
        for (int i = 0; i < slots.Length; i++)
        {
            uiSlots[i].Set(slots[i]);
        }
    }

    private void ClearUISlots()
    {
        foreach (ItemSlotUI uiSlot in uiSlots)
        {
            uiSlot.Clear();
        }
    }
    #endregion

    #region Dropzone
    private void HandleItemDroppedOnDropZone()
    {
        // Drop item
        if(inventorySlotUIDraggable.UISlot.CurrentItemSlot.Item == null)
            return;

        inventorySlotUIDraggable.UISlot.CurrentItemSlot.DropItem();
    }
    #endregion

    #region Quick Slots
    public void InitializeUIQuickSlots(int size)
    {
        for (int i = 0; i < size; i++)
        {
            QuickSlotUI uiSlot = Instantiate(quickSlotUIPrefab, Vector3.zero, Quaternion.identity);
            uiSlot.transform.SetParent(quickSlotsParent);
            
            uiSlot.Clear();
            uiSlot.SetIndex(i);
            uiSlot.SetPosition();
            
            quickUISlots.Add(uiSlot);
        }
    }

    private void HandleItemDropQuickSlot(QuickSlotUI quickSlotUI)
    {
        quickSlotUI.CurrentItemSlot.AddToQuickSlot(inventorySlotUIDraggable.UISlot.Index, inventorySlotUIDraggable.IsInventory);
        
        // Hide draggable slot
        inventorySlotUIDraggable.Toggle(false);
    }

    public void UpdateInventoryQuickSlots(QuickItemSlot[] slots)
    {
        ClearUIQuickSlots();

        // Update UI Quick Slots
        for (int i = 0; i < slots.Length; i++)
        {
            quickUISlots[i].Set(slots[i]);
        }
    }

    private void ClearUIQuickSlots()
    {
        foreach (QuickSlotUI quickUISlot in quickUISlots)
        {
            quickUISlot.Clear();
        }
    }

    public void SetQuickSlotSelected(int index, bool value)
    {
        quickUISlots[index].SetSelected(value);
    }

    public bool IsQuickSlotSelected(int index)
    {
        return quickUISlots[index].Selected;
    }
    #endregion

    #region Player Base Craft
    public void UpdateCraftInfo(CraftingData craftItem = null)
    {   
        if(craftItem != null)
        {
            craftItemName.text = craftItem.itemToCraft.displayName;
            craftItemIcon.sprite = craftItem.itemToCraft.icon;

            craftItemIcon.gameObject.SetActive(true);
            craftButton.SetActive(true);

            craftResourcesCosts.text = string.Empty;
            for(int x = 0; x < craftItem.resourceCosts.Length; x++)
            {
                craftResourcesCosts.text += string.Format(
                    "{0}: {1}\n", 
                    craftItem.resourceCosts[x].item.displayName.ToString(), 
                    craftItem.resourceCosts[x].quantity.ToString()
                );
            }
        }
        else
        {
            craftItemName.text = "(Select a item)";

            craftItemIcon.gameObject.SetActive(false);
            craftButton.SetActive(false);

            craftResourcesCosts.text = string.Empty;
        }
        
    }

    public void OnCraftButton()
    {
        // TODO:: Try to solve this without singleton
        Crafting.Instance.OnCraftButton(); 
    }

    public void UpdateCraftRecipesUIs()
    {
        // Update crafting UI
        for (int i = 0; i < craftRecipeUIs.Length; i++)
        {
            craftRecipeUIs[i].UpdateCanCraft();
        }
    }
    #endregion
}
