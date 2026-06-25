using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SequenceRunner : MonoBehaviour
{
    public enum StartTrigger
    {
        Manual,
        OnStart,
        OnCamcorderPickedUp,
        OnCorridorIteration,
        OnTriggerZoneEntered
    }

    [Header("Trigger")]
    [SerializeField] private StartTrigger startTrigger = StartTrigger.Manual;
    [Tooltip("Solo si startTrigger = OnCorridorIteration: el número de iteración que dispara esta secuencia.")]
    [SerializeField] private int iterationToMatch = 1;
    [Tooltip("Solo si startTrigger = OnTriggerZoneEntered: la zona que dispara esta secuencia.")]
    [SerializeField] private TriggerZone triggerZone;

    [Header("Setup")]
    [SerializeField] private SequenceStep[] steps;
    [SerializeField] private bool logSteps = true;

    [Header("Events")]
    public UnityEvent OnSequenceCompleted;

    public bool IsRunning { get; private set; }
    public int CurrentStepIndex { get; private set; } = -1;

    private void OnEnable()
    {
        if (startTrigger == StartTrigger.OnCamcorderPickedUp)
            GameEvents.OnCamcorderPickedUp += HandleCamcorderPickedUp;
        if (startTrigger == StartTrigger.OnCorridorIteration)
            CorridorTeleporter.OnIterationChanged += HandleIterationChanged;
        if (startTrigger == StartTrigger.OnTriggerZoneEntered && triggerZone != null)
            triggerZone.OnPlayerEntered += HandleTriggerEntered;
    }

    private void OnDisable()
    {
        if (startTrigger == StartTrigger.OnCamcorderPickedUp)
            GameEvents.OnCamcorderPickedUp -= HandleCamcorderPickedUp;
        if (startTrigger == StartTrigger.OnCorridorIteration)
            CorridorTeleporter.OnIterationChanged -= HandleIterationChanged;
        if (startTrigger == StartTrigger.OnTriggerZoneEntered && triggerZone != null)
            triggerZone.OnPlayerEntered -= HandleTriggerEntered;
    }

    private void Start()
    {
        if (startTrigger == StartTrigger.OnStart) StartSequence();
    }

    private void HandleCamcorderPickedUp() => StartSequence();

    private void HandleIterationChanged(int iteration)
    {
        if (iteration == iterationToMatch) StartSequence();
    }

    private void HandleTriggerEntered() => StartSequence();

    public void StartSequence()
    {
        if (IsRunning) return;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        IsRunning = true;

        for (int i = 0; i < steps.Length; i++)
        {
            CurrentStepIndex = i;

            if (steps[i] == null)
            {
                if (logSteps) Debug.LogWarning($"[SequenceRunner] Step {i} is null, skipping.");
                continue;
            }

            if (logSteps) Debug.Log($"[SequenceRunner] Executing step {i}: {steps[i].GetType().Name}");

            steps[i].Execute();

            while (!steps[i].IsCompleted) yield return null;

            if (logSteps) Debug.Log($"[SequenceRunner] Step {i} completed.");
        }

        IsRunning = false;
        CurrentStepIndex = -1;

        if (logSteps) Debug.Log($"[SequenceRunner] Sequence completed.");

        OnSequenceCompleted?.Invoke();
    }

    //public void ResetSequence()
    //{
    //    if (IsRunning) StopAllCoroutines();
    //    IsRunning = false;
    //    CurrentStepIndex = -1;
    //    foreach (var step in steps)
    //        if (step != null) step.ResetStep();
    //}
}