using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : Singleton<GameController>
{
    Inventory inv;

    [SerializeField] ItemData itemInv1;
    [SerializeField] ItemData itemInv2;
    [SerializeField] ItemData itemInv3;
    [SerializeField] ItemData itemInv4;

    private void Awake() {
        inv = FindObjectOfType<Inventory>();
    }

    // Update is called once per frame
    private void Start() {
        for (int i = 0; i < 100; i++)
        {
            inv.AddItem(itemInv1);
        }

        for (int i = 0; i < 100; i++)
        {
            inv.AddItem(itemInv2);
        }

        for (int i = 0; i < 100; i++)
        {
            inv.AddItem(itemInv3);
        }
        
        for (int i = 0; i < 100; i++)
        {
            inv.AddItem(itemInv4);
        }
    }
}
