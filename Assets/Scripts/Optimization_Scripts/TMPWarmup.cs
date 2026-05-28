using System.Collections;
using TMPro;
using UnityEngine;

public class TMPWarmup : MonoBehaviour
{
    [SerializeField] private TMP_Text[] textsToWarm;
    [SerializeField] private string warmString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,!?'\"";

    private IEnumerator Start()
    {
        foreach (var t in textsToWarm)
        {
            if (t == null) continue;
            t.gameObject.SetActive(true);
            t.SetText(warmString);
        }

        yield return null;
        yield return null;

        foreach (var t in textsToWarm)
        {
            if (t == null) continue;
            t.SetText("");
        }
    }
}