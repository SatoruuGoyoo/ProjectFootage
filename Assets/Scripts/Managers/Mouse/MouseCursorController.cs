using UnityEngine;

public class MouseCursorController : MonoBehaviour
{
    public static MouseCursorController Instance { get; private set; }

    private int _requestCount = 0;

    public bool CursorRequested => _requestCount > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        Debug.Log("[MouseCursorController] Start");
        Apply();
    }

    public void RequestCursor()
    {
        _requestCount++;
        Apply();
    }

    public void ReleaseCursor()
    {
        _requestCount = Mathf.Max(0, _requestCount - 1);
        Apply();
    }

    public void ForceRelease()
    {
        _requestCount = 0;
        Apply();
    }

    private void Apply()
    {
        bool showCursor = _requestCount > 0;
        Cursor.visible = showCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Debug.Log($"[MouseCursorController] Apply | requestCount: {_requestCount} | visible: {showCursor}");
    }
}