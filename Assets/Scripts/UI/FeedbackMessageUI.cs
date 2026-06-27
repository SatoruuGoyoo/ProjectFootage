//using UnityEngine;
//using TMPro;

//public class FeedbackMessageUI : MonoBehaviour
//{
//    [SerializeField] private GameObject container;
//    [SerializeField] private TMP_Text label;
//    [SerializeField] private float duration = 2f;

//    private float timer;

//    private void OnEnable()
//    {
//        GameEvents.OnFeedbackMessage += OnMessage;
//        if (container != null) container.SetActive(false);
//    }

//    private void OnDisable() => GameEvents.OnFeedbackMessage -= OnMessage;

//    private void Update()
//    {
//        if (timer <= 0f) return;
//        timer -= Time.deltaTime;
//        if (timer <= 0f && container != null) container.SetActive(false);
//    }

//    private void OnMessage(string message)
//    {
//        if (label != null) label.text = message;
//        if (container != null) container.SetActive(true);
//        timer = duration;
//    }
//}