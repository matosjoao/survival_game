using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

public class QuickSlotUI : MonoBehaviour, IDropHandler
{
    [Header("Properties")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI quickSlotPosition;

    private Outline outline;
    
    [HideInInspector] public bool Selected { get; private set;}
    [HideInInspector] public int Index { get; private set;}
    [HideInInspector] public QuickItemSlot CurrentItemSlot { get; private set;}
    [HideInInspector] public event Action<QuickSlotUI> OnItemDroppedOnQuickSlot;

    private void Awake() 
    {
        outline = GetComponent<Outline>();
    }

    private void OnEnable() 
    {
        outline.enabled = Selected;
    }

    public void Set(QuickItemSlot slot)
    {
        CurrentItemSlot = slot;

        icon.gameObject.SetActive(true);
        icon.sprite = slot.item.icon;

        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : string.Empty;

        if(outline != null)
        {
            outline.enabled = Selected;
        }
    }

    public void Clear()
    {
        CurrentItemSlot = null;
        
        icon.gameObject.SetActive(false);
        quantityText.text = string.Empty;

        icon.gameObject.SetActive(false);
        icon.sprite = null;
    }

    public void SetSelected(bool value)
    {
        Selected = value;
    }

    public void SetIndex(int value)
    {
        Index = value;
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // TODO:: Validate if comes from inventory or validate type
        OnItemDroppedOnQuickSlot?.Invoke(this);
    }

    public void SetPosition()
    {
        quickSlotPosition.text = (Index+1).ToString();
    }
}
