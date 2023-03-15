using System;

public class QuickItemSlot : ItemSlot
{
    private int itemSlotPosition;
    public int ItemSlotPosition => itemSlotPosition;

    public event Action<Guid, int, bool> OnItemAddToQuickSlot;
    
    public override void ClearSlot()
    {
        base.ClearSlot();

        itemSlotPosition = -1;
    }

    public void UpdateQuickSlot(ItemData data, int amount, int slotPosition)
    {
        UpdateSlot(data, amount);

        itemSlotPosition = slotPosition;
    }

    public void SetItemSlotPosition(int slotPosition)
    {
        itemSlotPosition = slotPosition;
    }

    public void AddToQuickSlot(int dPos, bool isInv)
    {
        OnItemAddToQuickSlot?.Invoke(Id, dPos, isInv);
    }
}
