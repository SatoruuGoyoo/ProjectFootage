using UnityEngine;
using UnityEngine.UI;

public class CamcorderDisplayScaler : MonoBehaviour
{
    private static readonly int DisplayUpscaleID = Shader.PropertyToID("_DisplayUpscale");

    [SerializeField] private RawImage viewfinderImage;
    [SerializeField] private Material ditherMaterial;
    [SerializeField] private RenderTexture recordingTexture;

    private Canvas _canvas;
    private float _lastUpscale = -1f;

    private void Awake()
    {
        _canvas = viewfinderImage.canvas;
    }

    private void Update()
    {
        if (viewfinderImage == null || ditherMaterial == null || recordingTexture == null) return;

        float scaleFactor = _canvas != null ? _canvas.scaleFactor : 1f;
        float displayPixelHeight = viewfinderImage.rectTransform.rect.height * scaleFactor;
        float upscale = displayPixelHeight / recordingTexture.height;

        if (Mathf.Approximately(upscale, _lastUpscale)) return;

        _lastUpscale = upscale;
        ditherMaterial.SetFloat(DisplayUpscaleID, upscale);
    }
}