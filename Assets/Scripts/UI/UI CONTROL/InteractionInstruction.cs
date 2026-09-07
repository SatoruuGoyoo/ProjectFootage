using System;
using UnityEngine;

[Serializable]
public class InteractionInstruction
{
    [SerializeField] private Sprite icon;
    [TextArea(1, 2)]
    [SerializeField] private string text;

    public Sprite Icon => icon;
    public string Text => text;
}