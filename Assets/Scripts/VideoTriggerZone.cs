using UnityEngine;
using UnityEngine.InputSystem;

public class VideoTriggerZone : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string playerTag = "Player";

    [Header("Escena al terminar")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private string introText = "";

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;
        inputActions.Disable();
        FadeManager.Instance.FadeToScene(sceneToLoad, introText);
    }
}