using System.Collections;
using UnityEngine;

public class CinematicBarsStep : SequenceStep
{
    public enum BarsAction { Show, Hide }

    [SerializeField] private BarsAction action = BarsAction.Show;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool waitForCompletion = true;

    protected override void OnExecute()
    {
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        var controller = CinematicBarsController.Instance;

        if (action == BarsAction.Show)
        {
            if (waitForCompletion)
                yield return controller.Show(fadeDuration, curve);
            else
                StartCoroutine(controller.Show(fadeDuration, curve));
        }
        else
        {
            if (waitForCompletion)
                yield return controller.Hide(fadeDuration, curve);
            else
                StartCoroutine(controller.Hide(fadeDuration, curve));
        }

        Complete();
    }
}