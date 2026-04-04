using UnityEngine;

public class ClockPuzzleManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private ClockPuzzle[] clocks;
    [SerializeField] private GameObject handSolved;

    private int solvedCount = 0;

    private void OnEnable()
    {
        GameEvents.OnClockSolved += OnClockSolved;
    }

    private void OnDisable()
    {
        GameEvents.OnClockSolved -= OnClockSolved;
    }

    private void OnClockSolved()
    {
        solvedCount++;
        Debug.Log($"Clock solved! Total solved: {solvedCount}/{clocks.Length}");

        if( solvedCount >= clocks.Length)
        {
            handSolved.SetActive(true);
            Debug.Log("All clocks solved! Hand is now active.");
        }
    }
}
