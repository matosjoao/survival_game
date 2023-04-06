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
    [SerializeField] ItemData itemInv5;
    [SerializeField] ItemData itemInv6;
    [SerializeField] ItemData itemInv7;

    private void Awake() {
        inv = FindObjectOfType<Inventory>();
    }

    private void Start() {
        for (int i = 0; i < 100; i++)
        {
            inv.AddItem(itemInv1);
            inv.AddItem(itemInv2);
            inv.AddItem(itemInv3);
            inv.AddItem(itemInv4);
            inv.AddItem(itemInv5);
            inv.AddItem(itemInv6);
        }

        for (int i = 0; i < 20; i++)
        {
            inv.AddItem(itemInv7);
        }
    }
}
