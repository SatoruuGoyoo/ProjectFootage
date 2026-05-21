using UnityEngine;

public class NoDataFeedback : MonoBehaviour
{
    [SerializeField] private GameObject overlay;
    [SerializeField] private float durartion = 1.5f;

    private float _timer;

    private void OnEnable()
    {
        GameEvents.OnRecordableEventInterrupted += HandleInterrupted;
        if (overlay != null) overlay.SetActive(false);
    }

    private void OnDisable() => GameEvents.OnRecordableEventInterrupted -= HandleInterrupted;

    private void Update()
    {
        if (_timer <= 0f) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f && overlay != null) overlay.SetActive(false);
    }

    private void HandleInterrupted(string id)
    {
        if(overlay != null) overlay.SetActive(true);
        _timer = durartion;
    }


}
