using UnityEngine;

public class ColorTarget : MonoBehaviour
{
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public void SetColor(Color newColor)
    {
        if (rend != null)
            rend.material.color = newColor;
    }
}