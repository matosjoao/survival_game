using UnityEngine;

public class BuildPreviewProgress : MonoBehaviour
{
    [SerializeField] GameObject progressBar;
    [SerializeField] ScaleAxis scaleAxis;

    public void UpdateProgress(float progress)
    {
        Vector3 currentScale = new Vector3();

        switch (scaleAxis)
        {
            case ScaleAxis.X:
                currentScale = new Vector3(progress, 1.0f, 1.0f);
                break;
            case ScaleAxis.Y:
                currentScale = new Vector3(1.0f, progress, 1.0f);
                break;
            case ScaleAxis.Z:
                currentScale = new Vector3(1.0f, 1.0f, progress);
                break;
            default:
                currentScale = new Vector3(1.0f, 1.0f, progress);
                break;
        }

        progressBar.transform.localScale = currentScale;
    }
    
    public void ResetProgress()
    {
        progressBar.transform.localScale = Vector3.zero;
    }
}

[System.Serializable]
public enum ScaleAxis
{
    X,
    Y,
    Z
}