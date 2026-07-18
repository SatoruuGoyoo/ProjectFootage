using UnityEngine;

public class CamcorderMenuAnimator : MonoBehaviour
{
    private static readonly int OpenTrigger = Animator.StringToHash("Open");
    private static readonly int CloseTrigger = Animator.StringToHash("Close");

    [SerializeField] private Animator animator;

    public System.Action OnCloseAnimationFinished;

    public void PlayOpen() => animator.SetTrigger(OpenTrigger);
    public void PlayClose() => animator.SetTrigger(CloseTrigger);

    public void NotifyCloseFinished() => OnCloseAnimationFinished?.Invoke();
}