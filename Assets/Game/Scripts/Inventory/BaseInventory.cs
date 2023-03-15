using System;
using UnityEngine;

public class BaseInventory : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private int maxSlotsSize;

    [Header("Items")]
    private ItemSlot[] slots;

    public PlayerController pController;
    public int MaxSlotsSize => maxSlotsSize;
    public ItemSlot[] Slots => slots;

    private DroppedBag curDroppedBag;

    protected void InitializeSlots()
    {
        // Initialize slots
        slots = new ItemSlot[maxSlotsSize];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemSlot();
        }
    } 

    protected void AddItem(Guid id, int invPos)
    {
        // Has someone interacting with chest?
        if(pController == null)
            return;
        
        // Get player inventory
        Inventory pInventory = pController.GetComponent<Inventory>();
        if(pInventory == null)
            return;

        // Get inventory slot index
        int chestPos = GetItemPositionById(id);
        if(chestPos == -1)
            return;

        // Validate inventory item data
        ItemSlot itemSlot = pInventory.GetItemInSlot(invPos);
        if(itemSlot == null)
            return;

        // Has item in slot
        if(slots[chestPos].Item != null)
        {
            // Is the same type and can stack and as room left?
            if(slots[chestPos].Item == itemSlot.Item && slots[chestPos].Item.canStack && slots[chestPos].RoomLeftInStack(itemSlot.Quantity, out int roomLeftQuantity ))
            {
                // Calculate quantity to add
                int quantityToAdd = Mathf.Min(roomLeftQuantity, itemSlot.Quantity);

                // Exchange quantities
                slots[chestPos].AddQuantity(quantityToAdd);

                // Update quantity in inventory
                pInventory.UpdateSlotQuantity(invPos, quantityToAdd);
            }
            else
            {
                ItemSlot item1 = new ItemSlot(slots[chestPos].Item, slots[chestPos].Quantity);

                // Assign item to storage slot
                slots[chestPos].UpdateSlot(itemSlot.Item, itemSlot.Quantity);

                // Swap between inventory and storage
                pInventory.UpdateSlot(item1, invPos);
            }
        }
        else
        {
            // Assign item to storage slot
            slots[chestPos].UpdateSlot(itemSlot.Item, itemSlot.Quantity);

            // Remove item from inventory
            pInventory.RemoveItem(invPos); 
        }
    }

    protected void SwapItem(Guid id, int dPos)
    {
        // Get inventory slot index
        int tPos = GetItemPositionById(id);
        if(tPos == -1)
            return;

        // Swap slots
        ItemSlot item1 = slots[dPos];
        slots[dPos] = slots[tPos];
        slots[tPos] = item1;
    }

    protected void DropItem(Guid id)
    {
         // Get inventory slot index
        int dropIndex = GetItemPositionById(id);

        // Is index valid
        if(dropIndex == -1 || slots[dropIndex] == null || slots[dropIndex].Item == null)
            return;

        // Drop a bag
        if(curDroppedBag == null)
        {
            // New Bag
            // TODO:: Improve
            // Get Dropped bag from pool
            // listen to an event when the dropped bag is realease or empty and set Inventory droppedbag to null
            //curDroppedBag = Instantiate(dropPrefab, transform.position, Quaternion.Euler(Vector3.one * UnityEngine.Random.value * 360.0f)).GetComponent<DroppedBag>();
        }
        
        // Add Drop item
        /* if(curDroppedBag.AddDroppedItem(slots[dropIndex].Item, slots[dropIndex].Quantity))
        {
            RemoveItem(dropIndex);
        } */
    }

    public ItemSlot GetItemInSlot(int index)
    {
        return slots[index];
    }

    public virtual void RemoveItem(int index)
    {
        // Is index valid
        if(slots[index] == null || slots[index].Item == null)
            return;
        
        // Clear in slots
        slots[index].ClearSlot();
    }

    public virtual void UpdateSlot(ItemSlot itemSlot, int index)
    {
        // Is index valid
        if(slots[index] == null)
            return;

        // Update slot
        slots[index].UpdateSlot(itemSlot.Item, itemSlot.Quantity);
    }

    public virtual void UpdateSlotQuantity(int index, int amount)
    {
        // Is index valid
        if(slots[index] == null || slots[index].Item == null)
            return;

        // Remove quantity from slots
        slots[index].RemoveQuantity(amount);

        // Has quantity in slot?
        if(slots[index].Quantity == 0)
        {
            // Clear Slot
            slots[index].ClearSlot();
        }
    }

    protected void UpdateSlotByIndex(ItemData item, int quantity, int index)
    {
        slots[index].UpdateSlot(item, quantity);
    }

    protected int GetEmptySlotPosition()
    {
        // Search for a empty slot
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].Item == null)
            {
                return i;
            }
        }
        return -1;
    }

    private int GetItemPositionById(Guid id)
    {
        // Search for a slot of the same ID
        for (int i = 0; i < slots.Length; i++)
        {
            if(slots[i].Id == id)
            {
                return i;
            }
        }

        return -1;
    }
}
