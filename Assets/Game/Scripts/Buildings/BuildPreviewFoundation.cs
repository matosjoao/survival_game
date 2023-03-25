using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPreviewFoundation : MonoBehaviour
{
    [SerializeField] private List<GameObject> supportFoundations = new List<GameObject>();
    public List<GameObject> SupportFoundations => supportFoundations;
}
