using System;
using UnityEngine;
using FMODUnity;

[Serializable]
public class PlayerVoiceLine
{
    [Header("Audio (FMOD)")]
    public EventReference audioEvent;

    [Header("Text")]
    [TextArea(3, 5)]
    public string text = "";

    [Header("Timing")]
    public float displayDurationNoAudio = 3f;
    public float graceTimeAfterAudio = 0.5f;
}