using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using FMODUnity;

public class ReadableUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private CanvasGroup bgGroup;
    [SerializeField] private Image itemSprite;
    [SerializeField] private TMP_Text textField;
    [SerializeField] private TMP_Text pageIndicator;
    [SerializeField] private UIPositioner positioner;

    [Header("Page Arrows")]
    [SerializeField] private GameObject leftArrow;
    [SerializeField] private GameObject rightArrow;

    [Header("Animation")]
    [SerializeField] private float bgFadeInDuration = 0.4f;
    [SerializeField] private float bgFadeOutDuration = 0.4f;
    [SerializeField] private float bgTargetAlpha = 0.7f;

    [Header("Audio")]
    [SerializeField] private EventReference pageTurnSound;

    private Coroutine _animCoroutine;
    private InputAction _navigateAction;
    private InputAction _cancelAction;
    private bool _isOpen;
    private bool _navigateNeutral = true;
    private string[] _pages;
    private int _currentPage;
    private Action _onCloseRequested;

    private void Awake() => ForceHide();

    private void Start()
    {
        _navigateAction = PlayerInput.Actions.UI.Navigate;
        _cancelAction = PlayerInput.Actions.UI.Cancel;
    }

    private void OnEnable()
    {
        GameEvents.OnReadableOpened += OnOpened;
        GameEvents.OnReadableClosed += OnClosed;
    }

    private void OnDisable()
    {
        GameEvents.OnReadableOpened -= OnOpened;
        GameEvents.OnReadableClosed -= OnClosed;
    }

    private void Update()
    {
        if (!_isOpen) return;

        if (_cancelAction.WasPressedThisFrame())
        {
            _onCloseRequested?.Invoke();
            return;
        }

        float h = _navigateAction.ReadValue<Vector2>().x;

        if (_navigateNeutral)
        {
            if (h > 0.5f) GoToPage(_currentPage + 1);
            else if (h < -0.5f) GoToPage(_currentPage - 1);

            if (Mathf.Abs(h) > 0.5f) _navigateNeutral = false;
        }
        else if (Mathf.Abs(h) < 0.1f)
        {
            _navigateNeutral = true;
        }
    }

    private void OnOpened(Sprite sprite, string[] pages, UIPositioner.ScreenPosition position, Action onCloseRequested)
    {
        if (!UILayerManager.TryShow(UILayerManager.Layer.Readable, ForceHide)) return;
        positioner?.SetPosition(position);
        if (itemSprite != null) itemSprite.sprite = sprite;

        _pages = (pages != null && pages.Length > 0) ? pages : new[] { "" };
        _currentPage = 0;
        _onCloseRequested = onCloseRequested;
        _navigateNeutral = Mathf.Abs(_navigateAction.ReadValue<Vector2>().x) < 0.5f;
        RefreshPage();
        _isOpen = true;

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateIn());
    }

    private void GoToPage(int index)
    {
        index = Mathf.Clamp(index, 0, _pages.Length - 1);
        if (index == _currentPage) return;
        _currentPage = index;
        RefreshPage();
        if (!pageTurnSound.IsNull) RuntimeManager.PlayOneShot(pageTurnSound, transform.position);
    }

    private void RefreshPage()
    {
        if (textField != null) textField.SetText(_pages[_currentPage]);
        if (pageIndicator != null) pageIndicator.SetText($"{_currentPage + 1}/{_pages.Length}");
        if (leftArrow != null) leftArrow.SetActive(_currentPage > 0);
        if (rightArrow != null) rightArrow.SetActive(_currentPage < _pages.Length - 1);
    }

    private void OnClosed()
    {
        _isOpen = false;
        UILayerManager.Release(UILayerManager.Layer.Readable);
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateOut());
    }

    private void ForceHide()
    {
        _isOpen = false;
        UILayerManager.Release(UILayerManager.Layer.Readable);
        if (_animCoroutine != null) { StopCoroutine(_animCoroutine); _animCoroutine = null; }
        SetVisible(false);
    }

    private IEnumerator AnimateIn()
    {
        if (bgGroup != null) bgGroup.alpha = 0f;
        if (itemSprite != null) itemSprite.gameObject.SetActive(false);
        if (textField != null) textField.gameObject.SetActive(false);

        SetVisible(true);

        float t = 0f;
        while (t < bgFadeInDuration)
        {
            t += Time.deltaTime;
            if (bgGroup != null) bgGroup.alpha = Mathf.Lerp(0f, bgTargetAlpha, t / bgFadeInDuration);
            yield return null;
        }
        if (bgGroup != null) bgGroup.alpha = bgTargetAlpha;

        if (itemSprite != null) itemSprite.gameObject.SetActive(true);
        if (textField != null) textField.gameObject.SetActive(true);

        _animCoroutine = null;
    }

    private IEnumerator AnimateOut()
    {
        if (itemSprite != null) itemSprite.gameObject.SetActive(false);
        if (textField != null) textField.gameObject.SetActive(false);

        float startAlpha = bgGroup != null ? bgGroup.alpha : 0f;
        float t = 0f;
        while (t < bgFadeOutDuration)
        {
            t += Time.deltaTime;
            if (bgGroup != null) bgGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / bgFadeOutDuration);
            yield return null;
        }
        if (bgGroup != null) bgGroup.alpha = 0f;

        SetVisible(false);
        _animCoroutine = null;
    }

    private void SetVisible(bool visible)
    {
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }
}