using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODManager : MonoBehaviour
{
    public static FMODManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // One-shot event trigger ( fire and forget )
    public void PlayOneShot(EventReference eventReference, Vector3 position = default)
    {
        RuntimeManager.PlayOneShot(eventReference, position);
    }

    // Create persistent event instance ( for looping or 3D sound )
    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        return RuntimeManager.CreateInstance(eventReference);
    }

    // Sets a global parameter 
        public void SetGlobalParameter(string name, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(name, value);
    }
}
