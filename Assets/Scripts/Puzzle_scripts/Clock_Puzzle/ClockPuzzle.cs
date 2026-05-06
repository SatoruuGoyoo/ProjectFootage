//using UnityEngine;

//public class ClockPuzzle : MonoBehaviour
//{
//    [Header("Setup")]
//    public Transform hourHandPivot;
//    public int correctAnswer;
//    public float anglePerFrame = 3.75f;

//    [Header("On Solve")]
//    public GameObject objectToActivate;

//    [SerializeField] private CamcorderPlayback playback;

//    public bool IsSolved { get; private set; } = false;
//    private bool playerInRange = false;

//    private void OnEnable()
//    {
//        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
//    }

//    private void OnDisable()
//    {
//        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//            playerInRange = true;
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))
//            playerInRange = false;
//    }

//    private void OnPlayerModeChanged(PlayerMode newMode)
//    {
//        if (newMode == PlayerMode.ExplorationMode && playerInRange)
//            ApplyAndFreeze(playback.CurrentFrame, playback.PlayerUsedRFF);
//    }

//    public void ApplyAndFreeze(int frame, bool playerUsedRFF)
//    {
//        if (!playerUsedRFF) return;
//        if (IsSolved) return;

//        float angle = frame * anglePerFrame;
//        hourHandPivot.localRotation = Quaternion.Euler(0f, angle, 0f);

//        SetLayerRecursive(hourHandPivot.gameObject, LayerMask.NameToLayer("Default"));

//        int currentNumber = GetNumberFromAngle(angle);
//        Debug.Log($"Aguja en número: {currentNumber} — Correcto: {correctAnswer}");

//        if (currentNumber == correctAnswer)
//        {
//            IsSolved = true;
//            if (objectToActivate != null)
//                objectToActivate.SetActive(true);

//            GameEvents.ClockSolved();
//            Debug.Log("Reloj resuelto!");
//        }
//    }

//    private int GetNumberFromAngle(float angle)
//    {
//        angle = angle % 360f;
//        if (angle < 0) angle += 360f;

//        int number = Mathf.RoundToInt(angle / 30f) % 12;
//        if (number == 0) number = 12;
//        return number;
//    }

//    private void SetLayerRecursive(GameObject obj, int layer)
//    {
//        obj.layer = layer;
//        foreach (Transform child in obj.transform)
//            SetLayerRecursive(child.gameObject, layer);
//    }
//}