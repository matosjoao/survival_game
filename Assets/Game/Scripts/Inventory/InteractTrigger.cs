using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractTrigger : MonoBehaviour
{
    [HideInInspector] public event Action<IInteractable> OnPlayerLeave;
    
    private List<IInteractable> interactables = new List<IInteractable>();

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            interactables.Add(interactable);
        }
    }

   private void OnTriggerExit(Collider other) 
   {
        if(other.gameObject.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            interactables.Remove(interactable);

            OnPlayerLeave?.Invoke(interactable);
        }
   }
}
