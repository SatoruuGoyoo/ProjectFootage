using UnityEngine;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;

public class AudioOptionsController : MonoBehaviour
{
    [System.Serializable]
    public class BusSlider
    {
        public string busPath;
        public Slider slider;
    }

    [SerializeField] private BusSlider[] busSliders;

    private void Start()
    {
        foreach (var bs in busSliders)
        {
            if (bs.slider == null) continue;

            Bus bus = RuntimeManager.GetBus(bs.busPath);
            bus.getVolume(out float current);
            bs.slider.SetValueWithoutNotify(current);

            string path = bs.busPath;
            bs.slider.onValueChanged.AddListener(value => SetBusVolume(path, value));
        }
    }

    private void SetBusVolume(string busPath, float value)
    {
        Bus bus = RuntimeManager.GetBus(busPath);
        bus.setVolume(value);
    }
}