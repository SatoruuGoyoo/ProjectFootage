using UnityEngine;
public class HallwayAudioManager : MonoBehaviour
{
    public static HallwayAudioManager Instance { get; private set; }
    [Tooltip("Un clip por iteración, en orden.")]
    public AudioClip[] iterationClips = new AudioClip[3];

    [Header("Ambient")]
    [Tooltip("AudioSource del GO de música de ambiente.")]
    public AudioSource ambientSource;
    [Tooltip("Un clip de ambiente por iteración, en orden.")]
    public AudioClip[] ambientClips = new AudioClip[3];

    private int _currentIteration = 0;
    public AudioClip CurrentClip => iterationClips[_currentIteration];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AdvanceIteration()
    {
        _currentIteration = Mathf.Clamp(_currentIteration + 1, 0, iterationClips.Length - 1);

        if (ambientSource != null && _currentIteration < ambientClips.Length && ambientClips[_currentIteration] != null)
        {
            ambientSource.clip = ambientClips[_currentIteration];
            ambientSource.Play();
        }
    }
}