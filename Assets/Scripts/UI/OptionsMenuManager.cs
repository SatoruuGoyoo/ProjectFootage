using UnityEngine;

namespace SM.UI
{
    public class OptionsMenuManager : MonoBehaviour
    {
        [Header("Menu Navigation")]
        [SerializeField] private MainMenuManager mainMenuManager;

        [Header("Option Tabs")]
        [SerializeField] private GameObject audioTab;
        // Futuras Pestañas:
        // [SerializeField] private GameObject videoTab;
        // [SerializeField] private GameObject controlsTab;

        private void OnEnable()
        {
            // Por defecto abrir las opciones de audio al entrar
            ShowAudioOptions();
        }

        public void ShowAudioOptions()
        {
            audioTab.SetActive(true);
            // En un futuro, desactivar los otros tabs aquí.
            // videoTab.SetActive(false);
        }

        public void OnBackClicked()
        {
            // Llama de regreso al manager principal para salir del Panel de Opciones
            mainMenuManager.ShowMainPanel();
        }
    }
}
