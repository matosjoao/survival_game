using System;
using UnityEngine;

public class BuildPreviewProgress : MonoBehaviour
{
    [SerializeField] GameObject progressBar;

    public void UpdateProgress(float progress)
    {
        progressBar.transform.localScale = new Vector3(1.0f, 1.0f, progress);
    }
    
    public void ResetProgress()
    {
        progressBar.transform.localScale = new Vector3(1.0f, 1.0f, 0.0f);
    }
}
