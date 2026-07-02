using UnityEngine;
using FMODUnity;

public class DoorActivator : MonoBehaviour
{
  

    [SerializeField] private EventReference doorOpenSound;

    public void ActivateDoor()
    {
      

        if (!doorOpenSound.IsNull)
            RuntimeManager.PlayOneShot(doorOpenSound, transform.position);
    }
}