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
            yield return FadeManager.Instance.FadeOut(fadeOutDuration);

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetScene);
        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.9f)
            yield return null;

        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
            yield return null;

        yield return new WaitForEndOfFrame();

        if (fadeInAfterLoad && FadeManager.Instance != null)
            yield return FadeManager.Instance.FadeIn(fadeInDuration);

        Complete();
    }
}