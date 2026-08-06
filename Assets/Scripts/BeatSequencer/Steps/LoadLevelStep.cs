using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelStep : SequenceStep
{
    [SerializeField] private SceneField targetScene;
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private bool fadeInAfterLoad = true;

    protected override void OnExecute()
    {
        StartCoroutine(RunLoad());
    }

    private IEnumerator RunLoad()
    {
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOut(fadeOutDuration);

            if (fadeInAfterLoad)
                FadeManager.Instance.RequestFadeInOnNextLoad(fadeInDuration);
        }

        SceneManager.LoadScene(targetScene);
        Complete();
    }
}