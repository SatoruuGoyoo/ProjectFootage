using System;
using TMPro;
using UnityEngine;

public class RealTimeDisplay : MonoBehaviour
{
    private TMP_Text textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TMP_Text>();

        UpdateTime();
        InvokeRepeating(nameof(UpdateTime), 1f, 1f);
    }

    private void UpdateTime()
    {
        textMesh.text = DateTime.Now.ToString("HH:mm:ss");
    }
}