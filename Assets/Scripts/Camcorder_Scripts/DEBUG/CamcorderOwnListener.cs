//using UnityEngine;

//[RequireComponent(typeof(AudioListener))]
//[RequireComponent(typeof(CamcorderAudioCapture))]
//public class CamcorderOwnListener : MonoBehaviour
//{
//    private AudioListener ownListener;
//    private CamcorderAudioCapture ownCapture;
//    private AudioListener disabledFixedListener;

//    private void Awake()
//    {
//        ownListener = GetComponent<AudioListener>();
//        ownCapture = GetComponent<CamcorderAudioCapture>();
//        ownListener.enabled = false;
//    }

//    private void OnEnable()
//    {
//        GameEvents.OnRecordingStarted += OnRecordingStarted;
//        GameEvents.OnRecordingStopped += OnRecordingStopped;
//    }

//    private void OnDisable()
//    {
//        GameEvents.OnRecordingStarted -= OnRecordingStarted;
//        GameEvents.OnRecordingStopped -= OnRecordingStopped;
//    }

//    private void OnRecordingStarted()
//    {
//        // Desactivamos el listener de la fixed cam activa
//        disabledFixedListener = FindActiveListener();
//        if (disabledFixedListener != null)
//            disabledFixedListener.enabled = false;

//        ownListener.enabled = true;
//        ownCapture.StartCapture();
//    }

//    private void OnRecordingStopped()
//    {
//        ownCapture.StopCapture();
//        ownListener.enabled = false;

//        // Devolvemos el listener a la fixed cam
//        if (disabledFixedListener != null)
//        {
//            disabledFixedListener.enabled = true;
//            disabledFixedListener = null;
//        }
//    }

//    public AudioClip GetCapturedClip() => ownCapture.GetCapturedClip();

//    private AudioListener FindActiveListener()
//    {
//        foreach (var al in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
//            if (al.enabled && al != ownListener) return al;
//        return null;
//    }
//}