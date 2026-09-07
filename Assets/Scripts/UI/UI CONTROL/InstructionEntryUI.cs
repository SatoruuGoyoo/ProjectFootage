using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionEntryUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    public void Bind(InteractionInstruction instruction)
    {
        if (instruction == null) return;

        if (icon != null)
        {
            bool hasIcon = instruction.Icon != null;
            icon.gameObject.SetActive(hasIcon);
            if (hasIcon) icon.sprite = instruction.Icon;
        }

        if (label != null)
        {
            bool hasText = !string.IsNullOrEmpty(instruction.Text);
            label.gameObject.SetActive(hasText);
            if (hasText) label.SetText(instruction.Text);
        }
    }
}