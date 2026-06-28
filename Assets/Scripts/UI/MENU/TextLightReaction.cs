using UnityEngine;
using TMPro;

public class TextLightReaction : MonoBehaviour
{
    public Light myLight;
    public TextMeshPro tmp;

    void Update()
    {
        float intensity = myLight.intensity;
        float brightness = Mathf.Clamp01(intensity / 10f); 
        float minBrightness = 0.25f;
        float finalBrightness = Mathf.Lerp(minBrightness, 1f, Mathf.Clamp01(intensity / 3f));
        tmp.color = new Color(finalBrightness, finalBrightness, finalBrightness, 1f);
    }
}