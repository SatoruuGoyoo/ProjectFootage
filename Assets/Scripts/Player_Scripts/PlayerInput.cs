using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float Move { get; private set; }
    public float Turn { get; private set; }

    void Update()
    {
        Move = Input.GetAxis("Vertical"); // W/S
        Turn = Input.GetAxis("Horizontal"); // A/D

    }
}
