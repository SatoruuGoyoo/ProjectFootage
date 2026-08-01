using UnityEngine;

public class RNGAmbienceManager : MonoBehaviour
{
    public static RNGAmbienceManager Instance { get; private set; }

    [SerializeField] private AmbiencePack initialPack;

    public AmbiencePack CurrentPack { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentPack = initialPack;
    }

    public void SetPack(AmbiencePack pack)
    {
        if (pack != null)
            CurrentPack = pack;
    }
}
