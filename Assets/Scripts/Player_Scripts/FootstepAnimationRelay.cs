using UnityEngine;

public class FootstepAnimationRelay : MonoBehaviour
{
    [SerializeField] private PlayerFootsteps playerFootsteps;

    private void Awake()
    {
        if (playerFootsteps == null)
            playerFootsteps = GetComponentInParent<PlayerFootsteps>();
    }

    public void PlayFootstep()
    {
        if (playerFootsteps != null)
            playerFootsteps.PlayFootstep();
    }
}