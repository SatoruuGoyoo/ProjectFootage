using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class VideoTriggerZone : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string playerTag = "Player";

    [Header("Escena al terminar")]
    [SerializeField] private string sceneToLoad;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;
        if (inputActions != null) inputActions.Disable();

        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOut();

            AsyncOperation loadOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneToLoad);
            loadOp.allowSceneActivation = false;

            while (loadOp.progress < 0.9f)
                yield return null;

            loadOp.allowSceneActivation = true;

            while (!loadOp.isDone)
                yield return null;

            yield return new WaitForEndOfFrame();

            yield return FadeManager.Instance.FadeIn();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }
}