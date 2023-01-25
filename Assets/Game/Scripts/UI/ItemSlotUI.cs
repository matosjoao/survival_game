using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI quantityText;

    private ItemSlot curSlot;
    private Outline outline;

    public bool Equipped { get; private set;}
    public int Index { get; private set;}

    private void Awake() 
    {
        outline = GetComponent<Outline>();
    }

    private void OnEnable() 
    {
        outline.enabled = Equipped;
    }

    public void Set(ItemSlot slot)
    {
        curSlot = slot;

        icon.gameObject.SetActive(true);
        icon.sprite = slot.item.icon;

        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : string.Empty;

        if(outline != null)
        {
            outline.enabled = Equipped;
        }
    }

    public void Clear()
    {
        curSlot = null;
        
        icon.gameObject.SetActive(false);
        quantityText.text = string.Empty;
    }

    public void OnButtonClick()
    {
        // TODO:: Try to solve this without singleton
        Inventory.Instance.SelectItem(Index); 
    }

    public void SetEquipped(bool value)
    {
        Equipped = value;
    }

    public void SetIndex(int value)
    {
        Index = value;
    }
}
