using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace SM.UI
{
    public class AudioOptionsController : MonoBehaviour
    {
        [Header("Audio Mixer Reference")]
        [Tooltip("Asigna aquí el MasterMixer")]
        [SerializeField] private AudioMixer masterMixer;

        [Header("UI Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        // Keys para mantener guardado el progreso en disco (PlayerPrefs)
        private const string MasterVolumePref = "MasterVolume";
        private const string MusicVolumePref = "MusicVolume";
        private const string SfxVolumePref = "SFXVolume";

        private void Start()
        {
            LoadVolumePreferences();
            
            // Agregamos un comportamiento dinámico: al deslizar la barra se actualiza en tiempo real
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        private void LoadVolumePreferences()
        {
            // Cargamos con un valor predeterminado del 75%
            float masterVol = PlayerPrefs.GetFloat(MasterVolumePref, 0.75f);
            float musicVol = PlayerPrefs.GetFloat(MusicVolumePref, 0.75f);
            float sfxVol = PlayerPrefs.GetFloat(SfxVolumePref, 0.75f);

            masterSlider.value = masterVol;
            musicSlider.value = musicVol;
            sfxSlider.value = sfxVol;

            // Se lo inyectamos al mixer usando la matemática de Decibeles
            SetMasterVolume(masterVol);
            SetMusicVolume(musicVol);
            SetSFXVolume(sfxVol);
        }

        public void SetMasterVolume(float sliderValue)
        {
            // Unity Mixer usa Decibels (-80 a 0 logarítmico), no porcentaje (0-1 lineal).
            float decibels = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f;
            masterMixer.SetFloat("MasterVolume", decibels);
            PlayerPrefs.SetFloat(MasterVolumePref, sliderValue);
        }

        public void SetMusicVolume(float sliderValue)
        {
            float decibels = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f;
            masterMixer.SetFloat("MusicVolume", decibels);
            PlayerPrefs.SetFloat(MusicVolumePref, sliderValue);
        }

        public void SetSFXVolume(float sliderValue)
        {
            float decibels = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20f;
            masterMixer.SetFloat("SFXVolume", decibels);
            PlayerPrefs.SetFloat(SfxVolumePref, sliderValue);
        }

        private void OnDestroy()
        {
            // Salvar al destruir el objeto (salir del juego o escena local)
            PlayerPrefs.Save();
            masterSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.RemoveAllListeners();
        }
    }
}
