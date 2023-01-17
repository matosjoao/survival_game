using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour
{
    [field: Header("Properties")]
    [SerializeField] private int damage;
    [SerializeField] private float damageRate;

    private List<IDamagable> _toDamageList = new List<IDamagable>();

    private void Start() 
    {
        StartCoroutine(DealDamage());
    }

    private void OnCollisionEnter(Collision collision) 
    {
        if(collision.gameObject.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            _toDamageList.Add(damagable);
        }
    }

    private void OnCollisionExit(Collision collision) 
    {
        if(collision.gameObject.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            _toDamageList.Remove(damagable);
        }
    }

    IEnumerator DealDamage()
    {
        while(true)
        {
            for (int i = 0; i < _toDamageList.Count; i++)
            {
                _toDamageList[i].TakePhisicalDamage(damage);
            }
            yield return new WaitForSeconds(damageRate);
        }
    }
}
