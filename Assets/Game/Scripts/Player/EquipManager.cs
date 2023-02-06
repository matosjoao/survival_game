using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputReader))]
public class EquipManager : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private Transform equipParent;
    
    [Header("Components")]
    private InputReader inputReader;
    private AnimatorManager animatorManager;

    private Equip currentEquip;
    private bool attacking;

    private void Awake() 
    {
        animatorManager = GetComponent<AnimatorManager>();
        inputReader = GetComponent<InputReader>();
    }

    private void Update() {
        if(inputReader.IsAttaking && currentEquip != null && UIManager.Instance.CanLook)
        {
            OnAttackInput();
        }
    }

    private void OnAttackInput()
    {
        if(!attacking)
        {
            attacking = true;

            // Set up animaiton
            animatorManager.SetOnAttackAnimation();

            // Call attack function with attackRate time delay
            Invoke("OnCanAttack", 1.2f);
        }
    }

    private void OnCanAttack()
    {
        attacking = false;
        
        // Set up animaiton
        animatorManager.SetFreeLookAnimation();
    }


    private void OnAltAttackInput()
    {
        if(currentEquip != null && UIManager.Instance.CanLook)
        {

        }
    }

    

    public void EquipNewItem(ItemData item)
    {
        UnEquip();

        // Instantiate Equipment
        currentEquip = Instantiate(item.equipPrefab, equipParent).GetComponent<Equip>();
    }

    public void UnEquip()
    {
        if(currentEquip != null)
        {
            // TODO:: Improve change to a pool method
            Destroy(currentEquip.gameObject);

            currentEquip = null;
        }
    }

    public void EnableEquipTool()
    {
        if(currentEquip != null)
        {
            currentEquip.EnableCollider();
        }
    }

    public void DisableEquipTool()
    {
        if(currentEquip != null)
        {
            currentEquip.DisableCollider();
        }
    }
}
