using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private float flashSpeed;

    private Coroutine fadeAway;

    public void Flash()
    {
        if(fadeAway != null)
        {
            StopCoroutine(fadeAway);
        }

        image.enabled = true;
        image.color = Color.white;

        fadeAway = StartCoroutine(FadeAway());
    }

    IEnumerator FadeAway()
    {
        float alpha = 1.0f;

        while(alpha > 0.0f)
        {
            alpha -= (1.0f / flashSpeed) * Time.deltaTime;
            image.color = new Color(1.0f, 1.0f, 1.0f, alpha);

            yield return null;
        }

        image.enabled = false;
    }
}
