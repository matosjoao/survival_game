using System.Collections.Generic;
using UnityEngine;

public class EquipTool : Equip
{
    [Header("Components")]
    [SerializeField] private Collider toolCollider;

    [Header("Properties")]
    [SerializeField] private float attackRate;

    [Header("Resource Gathering")]
    [SerializeField] private bool canGatherResourses;
    [SerializeField] private int gatherQuantity; // TODO:: Change gather qty from resource to here

    [Header("Combat")]
    [SerializeField] private bool canCombat;
    [SerializeField] private int damage;
    [SerializeField] private string attackAnimation;

    private List<Collider> alreadyCollidedWith = new List<Collider>();

    private void OnEnable() 
    {
        alreadyCollidedWith.Clear();
    }

    private void OnTriggerEnter(Collider other) 
    {   
        //if(other == myCollider) { return; }

        // Avoid hitting multiple times on same collider
        if(alreadyCollidedWith.Contains(other)) { return; }

        alreadyCollidedWith.Add(other);

        // Hit a resource
        if(canGatherResourses && other.TryGetComponent<Resource>(out Resource resource))
        {
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            Vector3 collisionNormal = transform.position - collisionPoint;

            resource.Gather(collisionPoint, collisionNormal);
        }

        // Hit a damagable
        if(canCombat && other.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            damagable.TakePhisicalDamage(damage);
        }
    }  

    /* public override void OnAttackInput()
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
    } */

    public override void EnableCollider()
    {
        // We gonna turn on the tool collider so it can be trigger with other collider
        toolCollider.enabled = true;
    }

    public override void DisableCollider()
    {
        // Disable tool collider
        toolCollider.enabled = false;
        
        // TODO:: Change the collider to a seperated gameobject and ativate and desactivated the gameobject 
        // and clear the list on disable
        alreadyCollidedWith.Clear();
    }
}
