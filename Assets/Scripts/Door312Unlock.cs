using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace ProjectFootage.Puzzle
{
    public class Door312Unlock : MonoBehaviour
    {
        [SerializeField] private string requiredEventId = "bathtub_01";
        [SerializeField] private GameObject doorToOpen;
        [SerializeField] private Transform doorPivot;
        [SerializeField] private EventReference unlockSound;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float speed = 120f;

        private bool armed;
        private bool isOpen;
        private Quaternion closedRot;
        private Quaternion openRot;

        private void OnEnable() => GameEvents.OnRecordableEventCompleted += HandleCompleted;
        private void OnDisable() => GameEvents.OnRecordableEventCompleted -= HandleCompleted;

    private void Awake()
    {
        if (doorToOpen != null)
        {
            if (doorPivot == null) doorPivot = doorToOpen.transform;
            closedRot = doorPivot.localRotation;
            openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);
        }
    }

    private void HandleCompleted(string id)
    {
        if (id == requiredEventId) armed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!armed) return;
        if (!other.CompareTag("Player")) return;
        Debug.Log("Door unlocked!");
        OpenDoor();
    }

    private void OpenDoor()
    {
        if (doorToOpen == null) return;

        isOpen = true;
        if (!unlockSound.IsNull) RuntimeManager.PlayOneShot(unlockSound, doorToOpen.transform.position);
    }

    private void Update()
    {
        if (doorToOpen != null && isOpen && doorPivot != null)
        {
            doorPivot.localRotation = Quaternion.RotateTowards(
                doorPivot.localRotation,
                openRot,
                speed * Time.deltaTime
            );
        }
    }
}
}