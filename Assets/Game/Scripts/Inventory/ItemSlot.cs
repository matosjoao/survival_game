using System;

public class ItemSlot
{
    private Guid id;
    private ItemData itemData;
    private int quantity;

    public Guid Id => id;
    public ItemData Item => itemData;
    public int Quantity => quantity;

    public event Action<Guid> OnItemDrop;
    public event Action<Guid, int> OnItemAdd;
    public event Action<Guid, int> OnItemSwap;

    public ItemSlot(ItemData data, int amount)
    {
        id = Guid.NewGuid();
        itemData = data;
        quantity = amount;
    }

    public ItemSlot()
    {
        ClearSlot();
        id = Guid.NewGuid();
    }

    public virtual void ClearSlot()
    {
        itemData = null;
        quantity = -1;
    }

    public void UpdateSlot(ItemData data, int amount)
    {
        itemData = data;
        quantity = amount;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;

        if(quantity > itemData.maxStackAmount)
            quantity = itemData.maxStackAmount;
    }

    public void RemoveQuantity(int amount)
    {
        quantity -= amount;

        if(quantity < 0)
            quantity = 0;
    }

    public bool RoomLeftInStack(int amountToAdd, out int amountRemaining)
    {
        amountRemaining = itemData.maxStackAmount - quantity;
        
        return RoomLeftInStack(amountToAdd);
    }

    public bool RoomLeftInStack(int amountToAdd)
    {
        if(quantity + amountToAdd <= itemData.maxStackAmount) return true;
        else return false;
    }

    public void AddItem(int dPos)
    {
        OnItemAdd?.Invoke(Id, dPos);
    }
    
    public void SwapItems(int dPos)
    {
        OnItemSwap?.Invoke(Id, dPos);
    }

    public void DropItem()
    {
        OnItemDrop?.Invoke(Id);
    }
}
