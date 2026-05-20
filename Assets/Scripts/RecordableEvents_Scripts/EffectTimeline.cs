using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EffectTimeline : MonoBehaviour, IRecordableEffect
{
    [Serializable]
    public class Beat
    {
        public string label;
        [Range(0f, 1f)] public float triggerAt;
        public UnityEvent onReached;
    }

    [SerializeField] private List<Beat> beats = new();

    private bool[] fired;

    private void Awake() => fired = new bool[beats.Count];

    public void OnRecordingStarted() => ResetBeats();

    public void OnRecordingProgress(float t)
    {
        for (int i = 0; i < beats.Count; i++)
        {
            if (fired[i]) continue;
            if (t >= beats[i].triggerAt)
            {
                fired[i] = true;
                beats[i].onReached?.Invoke();
            }
        }
    }

    public void OnRecordingCompleted() { }

    public void OnRecordingInterrupted() => ResetBeats();

    private void ResetBeats()
    {
        if (fired == null || fired.Length != beats.Count) fired = new bool[beats.Count];
        for (int i = 0; i < fired.Length; i++) fired[i] = false;
    }
}