using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum CamcorderInputLock
{
    None,
    Lift,
    Record,
    Menu
}

[Serializable]
public class InputTutorialPrompt
{
    [TextArea(1, 3)]
    public string promptText;
    public InputActionReference inputAction;
    public CamcorderInputLock unlockOnShow = CamcorderInputLock.None;
    public bool requiresHold;
    public float holdDuration = 1f;
    public float delayAfterCompleted = 0f;
    public UnityEvent OnPromptCompleted;
}

public class InputTutorialStep : SequenceStep
{
    [SerializeField] private InputTutorialPrompt[] prompts;
    [SerializeField] private UIPositioner.ScreenPosition textPosition = UIPositioner.ScreenPosition.LowerCenter;

    protected override void OnExecute()
    {
        StartCoroutine(RunPrompts());
    }

    private IEnumerator RunPrompts()
    {
        foreach (var prompt in prompts)
        {
            if (prompt.inputAction == null || prompt.inputAction.action == null)
                continue;

            InputAction action = prompt.inputAction.action;
            action.Enable();
            Unlock(prompt.unlockOnShow);

            GameEvents.TutorialPromptShown(prompt.promptText, textPosition);

            yield return prompt.requiresHold ? WaitForHold(action, prompt.holdDuration) : WaitForPress(action);

            GameEvents.TutorialPromptHidden();
            prompt.OnPromptCompleted?.Invoke();

            if (prompt.delayAfterCompleted > 0f)
                yield return new WaitForSeconds(prompt.delayAfterCompleted);
        }

        Complete();
    }

    private void Unlock(CamcorderInputLock target)
    {
        switch (target)
        {
            case CamcorderInputLock.Lift:
                CamcorderController.LiftInputBlocked = false;
                break;
            case CamcorderInputLock.Record:
                CamcorderController.RecordInputBlocked = false;
                break;
            case CamcorderInputLock.Menu:
                CamcorderMenuController.MenuInputBlocked = false;
                break;
        }
    }

    private IEnumerator WaitForPress(InputAction action)
    {
        while (!action.WasPerformedThisFrame())
            yield return null;
    }

    private IEnumerator WaitForHold(InputAction action, float duration)
    {
        float heldTime = 0f;
        while (heldTime < duration)
        {
            heldTime = action.IsPressed() ? heldTime + Time.deltaTime : 0f;
            yield return null;
        }
    }
}