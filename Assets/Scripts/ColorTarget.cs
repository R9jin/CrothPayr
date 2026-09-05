using System;
using UnityEngine;

public class ColorTarget : MonoBehaviour
{
    public static event Action<ColorTarget, Color> OnColorChanged;

    private Renderer rend;
    private Color currentColor = Color.white;

    public Color CurrentColor => currentColor;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            currentColor = rend.material.color;
        }
    }

    public void SetColor(Color newColor)
    {
        if (rend != null)
        {
            rend.material.color = newColor;
            currentColor = newColor;
            OnColorChanged?.Invoke(this, newColor);
        }
    }
}