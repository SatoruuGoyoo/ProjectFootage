//using UnityEngine;

///// <summary>
///// Activa este GameObject solo cuando la iteración llega al número configurado.
///// </summary>
//public class ActivateOnIteration : MonoBehaviour
//{
//    [Tooltip("En qué iteración se activa este GO. 0=primera, 1=segunda, 2=tercera")]
//    public int targetIteration = 2; // 2 = iteración 3

//    private void Start()
//    {
//        gameObject.SetActive(false); // empieza desactivado
//        GameEvents.OnIterationChanged += OnIterationChanged;
//    }

//    private void OnDestroy()
//    {
//        GameEvents.OnIterationChanged -= OnIterationChanged;
//    }

//    private void OnIterationChanged(int iteration)
//    {
//        if (iteration == targetIteration)
//            gameObject.SetActive(true);
//    }
//}