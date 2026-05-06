//using UnityEngine;

//public class ClockController : MonoBehaviour
//{
//    [Header("Setup")]
//    public Transform hourHandPivot;
//    [SerializeField] private float rotationSpeed = 30f;

//    private ClockPuzzle puzzle;

//    private void Awake()
//    {
//        puzzle = GetComponent<ClockPuzzle>();
//    }

//    private void Update()
//    {
//        if (puzzle != null && puzzle.IsSolved) return;
//        hourHandPivot.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
//    }
//}