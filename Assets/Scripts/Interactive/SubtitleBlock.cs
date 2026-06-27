using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleBlock : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private TMP_Text label;
    [SerializeField] private UIPositioner positioner;

    private string _pendingText;
    private UIPositioner.ScreenPosition _pendingPosition;
    private Coroutine _sequenceCoroutine;

    private void Awake() => ForceHide();

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;
    private void OnDisable()
    {
        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
        StopSequence();
    }

    public void Show(string text, UIPositioner.ScreenPosition position = UIPositioner.ScreenPosition.LowerCenter)
    {
        if (string.IsNullOrEmpty(text)) return;
        _pendingText = text;
        _pendingPosition = position;

        if (!UILayerManager.TryShow(UILayerManager.Layer.Subtitles, ForceHide)) return;

        positioner?.SetPosition(position);
        if (label != null) label.SetText(text);
        SetVisible(true);
    }

    public void ShowSequence(SubtitleEntry[] entries)
    {
        if (entries == null || entries.Length == 0) return;
        StopSequence();
        _sequenceCoroutine = StartCoroutine(RunSequence(entries));
    }

    public void Hide()
    {
        StopSequence();
        _pendingText = null;
        UILayerManager.Release(UILayerManager.Layer.Subtitles);
        SetVisible(false);
    }

    private IEnumerator RunSequence(SubtitleEntry[] entries)
    {
        foreach (var entry in entries)
        {
            Show(entry.text, entry.position);
            yield return new WaitForSeconds(entry.duration);
        }
        Hide();
    }

    private void StopSequence()
    {
        if (_sequenceCoroutine != null)
        {
            StopCoroutine(_sequenceCoroutine);
            _sequenceCoroutine = null;
        }
    }

    private void OnConfirmationClosed()
    {
        if (string.IsNullOrEmpty(_pendingText)) return;
        Show(_pendingText, _pendingPosition);
    }

    private void SetVisible(bool visible)
    {
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }

    private void ForceHide()
    {
        if (container == null) return;
        container.alpha = 0f;
        container.interactable = false;
        container.blocksRaycasts = false;
    }
}