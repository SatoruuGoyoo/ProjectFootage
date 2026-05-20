using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Si usás TextMeshPro

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referencias")]
    public Image backgroundImage;   // Imagen de fondo del botón
    public TextMeshProUGUI buttonText; // O usa Text si no tenés TMP

    [Header("Colores")]
    public Color normalBgColor = new Color(0, 0, 0, 0);   // Transparente
    public Color hoverBgColor = Color.white;
    public Color normalTextColor = Color.white;
    public Color hoverTextColor = Color.black;

    public void OnPointerEnter(PointerEventData eventData)
    {
        backgroundImage.color = hoverBgColor;
        buttonText.color = hoverTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        backgroundImage.color = normalBgColor;
        buttonText.color = normalTextColor;
    }
}