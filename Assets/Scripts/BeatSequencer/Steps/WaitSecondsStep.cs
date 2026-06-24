using System.Collections;
using UnityEngine;

public class WaitSecondsStep : SequenceStep
{
    [SerializeField] private float seconds = 1f;

    protected override void OnExecute()
    {
        StartCoroutine(WaitRoutine());
    }

    private IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(seconds);
        Complete();
    }
}