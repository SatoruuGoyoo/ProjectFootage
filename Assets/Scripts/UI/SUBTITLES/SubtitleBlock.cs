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
    private bool _isVisible;

    private void Awake()
    {
        _isVisible = true;
        ForceHide();
    }

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;

    private void OnDisable()
    {
        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
        Hide();
    }

    public void Show(string text, UIPositioner.ScreenPosition position = UIPositioner.ScreenPosition.LowerCenter)
    {
        StopSequence();
        ShowLine(text, position);
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
        UILayerManager.Release(UILayerManager.Layer.Subtitles, this);
        SetVisible(false);
    }

    private void ShowLine(string text, UIPositioner.ScreenPosition position)
    {
        if (string.IsNullOrEmpty(text)) return;

        _pendingText = text;
        _pendingPosition = position;

        if (!UILayerManager.TryShow(UILayerManager.Layer.Subtitles, this, position, ForceHide)) return;

        positioner?.SetPosition(position);
        if (label != null) label.SetText(text);
        SetVisible(true);
    }

    private IEnumerator RunSequence(SubtitleEntry[] entries)
    {
        foreach (var entry in entries)
        {
            ShowLine(entry.text, entry.position);
            yield return new WaitForSeconds(entry.duration);
        }
        _sequenceCoroutine = null;
        Hide();
    }

    private void StopSequence()
    {
        if (_sequenceCoroutine == null) return;
        StopCoroutine(_sequenceCoroutine);
        _sequenceCoroutine = null;
    }

    private void OnConfirmationClosed()
    {
        if (string.IsNullOrEmpty(_pendingText)) return;
        ShowLine(_pendingText, _pendingPosition);
    }

    private void ForceHide()
    {
        UILayerManager.Release(UILayerManager.Layer.Subtitles, this);
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }
}