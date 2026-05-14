using UnityEngine;

public class MouseConfiner : MonoBehaviour
{
    private void Awake() => DontDestroyOnLoad(gameObject);

    private void Start() => Confine();
    private void LateUpdate() => Confine();

    private void OnApplicationFocus(bool focus)
    {
         if (focus) Confine();
    }

    private static void Confine()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
   
}
