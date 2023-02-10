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

    [Header("Player Inventory")]
    [SerializeField] private GameObject inventoryWindow;
    [SerializeField] private RectTransform inventorySlotsParent;
    [SerializeField] private RectTransform quickSlotsParent;
    [SerializeField] private ItemSlotUI inventorySlotUIPrefab;
    [SerializeField] private QuickSlotUI quickSlotUIPrefab;
    [SerializeField] private MouseFollower inventorySlotUIDraggable;
    [SerializeField] private Dropzone droppablezone;

    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;
    private List<ItemSlotUI> uiSlots = new List<ItemSlotUI>();
    private List<QuickSlotUI> quickUISlots = new List<QuickSlotUI>();
    private int currentDraggedItemIndex = -1;

    [Header("Player Base Craft")]
    [SerializeField] private TextMeshProUGUI craftItemName;
    [SerializeField] private Image craftItemIcon;
    [SerializeField] private GameObject craftButton;
    [SerializeField] private TextMeshProUGUI craftResourcesCosts;
    [SerializeField] private CraftingRecipeUI[] craftRecipeUIs;

    [HideInInspector] public bool CanLook { get; private set;} = true;
    [HideInInspector] public bool CanInteract { get; private set;} = true;
    
    public void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        CanLook = !toggle;
    }
    
    public void ToggleInteract(bool value = false)
    {
        CanInteract = value;
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

    public void InitializeUISlots(int size)
    {
        for (int i = 0; i < size; i++)
        {
            ItemSlotUI uiSlot = Instantiate(inventorySlotUIPrefab, Vector3.zero, Quaternion.identity);
            uiSlot.transform.SetParent(inventorySlotsParent);
            
            uiSlot.SetIndex(i);
            uiSlot.Clear();
            uiSlots.Add(uiSlot);

            uiSlot.OnItemBeginDrag += HandleItemBeginDrag;
            uiSlot.OnItemEndDrag += HandleItemEndDrag;
            uiSlot.OnItemDroppedOn += HandleItemDrop;
            uiSlot.OnItemRightMouseClick += HandleItemRightClick;
        }
    }

    private void HandleItemRightClick(ItemSlotUI itemSlotUI)
    {
        Debug.Log("Right Click");
    }

    private void HandleItemDrop(ItemSlotUI itemSlotUI)
    {
        // Hide draggable slot
        inventorySlotUIDraggable.Toggle(false);

        // TODO:: Try to solve this without singleton
        if(inventorySlotUIDraggable.IsInventory)
        {
            // From inventory
            Inventory.Instance.SwapItems(itemSlotUI.Index, currentDraggedItemIndex); 
        }
        else
        {
            // From bag
            Inventory.Instance.AddItemFromBag(itemSlotUI.Index, inventorySlotUIDraggable.UISlot.CurrentItemSlot); 
        }
    }

    private void HandleItemBeginDrag(ItemSlotUI itemSlotUI)
    {   
        // Slot has item
        if(itemSlotUI.CurrentItemSlot == null)
            return;
        
        // Assign current slot index
        currentDraggedItemIndex = itemSlotUI.Index;

        // Show draggable slot in mouse position
        inventorySlotUIDraggable.Toggle(true);
        inventorySlotUIDraggable.SetData(itemSlotUI.CurrentItemSlot);
    }

    private void HandleItemEndDrag(ItemSlotUI itemSlotUI)
    {  
        inventorySlotUIDraggable.Toggle(false);
        currentDraggedItemIndex = -1;
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
    #endregion

    #region Dropzone
    public void InitializeDropzone()
    {
        droppablezone.OnItemDroppedOnDropZone += HandleItemDroppedOnDropZone;
    }

    private void HandleItemDroppedOnDropZone()
    {
        if(currentDraggedItemIndex == -1)
            return;

        // Drop from inventory
        if(inventorySlotUIDraggable.IsInventory)
        {
            Inventory.Instance.DropItem(currentDraggedItemIndex);
        }
    }
    #endregion

    #region Quick Slots
    public void InitializeUIQuickSlots(int size)
    {
        for (int i = 0; i < size; i++)
        {
            QuickSlotUI uiSlot = Instantiate(quickSlotUIPrefab, Vector3.zero, Quaternion.identity);
            uiSlot.transform.SetParent(quickSlotsParent);
            
            uiSlot.SetIndex(i);
            uiSlot.SetPosition();
            uiSlot.Clear();
            quickUISlots.Add(uiSlot);

            uiSlot.OnItemDroppedOnQuickSlot += HandleItemDropQuickSlot;
        }
    }

    private void HandleItemDropQuickSlot(QuickSlotUI quickSlotUI)
    {
        // Hide draggable slot
        inventorySlotUIDraggable.Toggle(false);

        // TODO:: Try to solve this without singleton
        if(inventorySlotUIDraggable.IsInventory)
        {
            // From inventory
            Inventory.Instance.AddToQuickSlot(quickSlotUI.Index, currentDraggedItemIndex); 
        }
        else
        {
            // From bag
            Inventory.Instance.AddItemFromBagToQuickSlot(quickSlotUI.Index, inventorySlotUIDraggable.UISlot.CurrentItemSlot); 
        }
    }

    public void UpdateInventoryQuickSlots(QuickItemSlot[] slots)
    {
        // Update UI Quick Slots
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item != null)
            {
                quickUISlots[i].Set(slots[i]);
            }
            else
            {
                quickUISlots[i].Clear();
            }
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
