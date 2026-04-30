using UnityEngine;


public class DebugControlsToggle : MonoBehaviour
{
    [SerializeField] private GameObject debugControlsUI;
    [SerializeField] private GameObject pressPrompt;

    private void Start()
    {
        debugControlsUI.SetActive(false);
        pressPrompt.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            bool show = !debugControlsUI.activeSelf;
            debugControlsUI.SetActive(show);
            pressPrompt.SetActive(!show);
        }
    }
}