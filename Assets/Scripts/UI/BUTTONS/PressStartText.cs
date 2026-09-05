using UnityEngine;
using TMPro;

public class BlinkText : MonoBehaviour
{
    public float blinkSpeed = 1.5f;
    private TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}
