using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPreview : MonoBehaviour
{
    [SerializeField] private Material canPlaceObject;
    [SerializeField] private Material cannotPlaceObject;

    private MeshRenderer[] meshRenderers;
    private List<GameObject> collidingObjects = new List<GameObject>();

    void Awake ()
    {
        meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
    }

    public void CanPlace ()
    {
        SetMaterial(canPlaceObject);
    }

    public void CannotPlace ()
    {
        SetMaterial(cannotPlaceObject);
    }

    void SetMaterial (Material mat)
    {
        for(int x = 0; x < meshRenderers.Length; x++)
        {
            Material[] mats = new Material[meshRenderers[x].materials.Length];

            for(int y = 0; y < mats.Length; y++)
            {
                mats[y] = mat;
            }

            meshRenderers[x].materials = mats;
        }
    }

    public bool CollidingWithObjects ()
    {
        collidingObjects.RemoveAll(x => x == null);
        return collidingObjects.Count > 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 10 is the terrain layer
        if(other.gameObject.layer != 10)
            collidingObjects.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        // 10 is the terrain layer
        if(other.gameObject.layer != 10)
            collidingObjects.Remove(other.gameObject);
    }
}
