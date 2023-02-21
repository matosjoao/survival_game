using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    [SerializeField] ItemData item;
    [SerializeField] int quantityPerHit = 1;
    [SerializeField] int capacity;
    [SerializeField] GameObject hitParticle;

    public void Gather(Vector3 hitPoint, Vector3 hitNormal)
    {
        for (int i = 0; i < quantityPerHit; i++)
        {
            if(capacity <= 0)
                break;

            capacity -= 1;

            // TODO:: Change to a if if(inventory.AddItem(selectedItem.itemToCraft))
            Inventory.Instance.AddItem(item);
        }

        Debug.Log(hitNormal);

        // TODO:: Improve maybe change to pool
        Destroy(Instantiate(hitParticle, hitPoint, Quaternion.LookRotation(hitNormal, Vector3.up)), 1.0f);

        // TODO:: Improve change to pool
        if(capacity <= 0)
        {
            Destroy(gameObject);
        }
    }
   
}
