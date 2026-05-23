using UnityEngine;
using TMPro;

public class MainMenuIntro : MonoBehaviour
{
    [Header("References")]
    public Transform mainCamera;
    public GameObject pressStartText;
    public GameObject mainMenuCanvas;
    public GameObject title;

    [Header("Camera Start (lejos)")]
    public Vector3 startPosition;
    public Vector3 startRotation;

    [Header("Camera End (pegado a la pantallita)")]
    public Vector3 endPosition;
    public Vector3 endRotation;

    [Header("Settings")]
    public float moveDuration = 2f;

    private bool waitingForInput = true;
    private bool isMoving = false;
    private float moveTimer = 0f;

    void Start()
    {
        // Asegurar estado inicial
        mainCamera.position = startPosition;
        mainCamera.eulerAngles = startRotation;
        mainMenuCanvas.SetActive(false);
        pressStartText.SetActive(true);
        title.SetActive(true);
    }

    void Update()
    {
        // Fase 1: Esperando SPACE
        if (waitingForInput)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                waitingForInput = false;
                isMoving = true;
                moveTimer = 0f;
                pressStartText.SetActive(false);
                title.SetActive(false);
            }
            return;
        }

        // Fase 2: Moviendo la cámara
        if (isMoving)
        {
            moveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(moveTimer / moveDuration);

            // Easing suave (slow in, slow out)
            float smooth = t * t * (3f - 2f * t);

            mainCamera.position = Vector3.Lerp(startPosition, endPosition, smooth);

            Quaternion startRot = Quaternion.Euler(startRotation);
            Quaternion endRot = Quaternion.Euler(endRotation);
            mainCamera.rotation = Quaternion.Slerp(startRot, endRot, smooth);

            // Cuando termina
            if (t >= 1f)
            {
                isMoving = false;
                mainMenuCanvas.SetActive(true);
            }
        }
    }
}