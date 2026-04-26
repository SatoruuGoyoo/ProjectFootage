using UnityEngine;

public class KeyButton3D : MonoBehaviour
{
    [Header("Configuración de tecla")]
    public KeyCode assignedKey;

    [Header("Movimiento de presión")]
    [Tooltip("Profundidad de bajada. 0.001 = 1 mm (escala 1u = 1m)")]
    public float pressDepth = 0.001f;

    [Tooltip("Velocidad de la animación de presión/liberación")]
    public float pressSpeed = 20f;

  
    private static KeyCode currentlyPressed = KeyCode.None;

    private Vector3 restPosition;
    private Vector3 pressedPosition;

    void Start()
    {
        restPosition = transform.localPosition;
        pressedPosition = restPosition - new Vector3(0f, pressDepth, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(assignedKey) && currentlyPressed == KeyCode.None)
            currentlyPressed = assignedKey;

        if (Input.GetKeyUp(assignedKey) && currentlyPressed == assignedKey)
            currentlyPressed = KeyCode.None;

        bool isPressed = (currentlyPressed == assignedKey);

        Vector3 target = isPressed ? pressedPosition : restPosition;
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            target,
            Time.deltaTime * pressSpeed
        );
    }
}