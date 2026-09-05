using UnityEngine;

[System.Serializable]
public class ObjectiveEntry
{
    [Tooltip("The exact GameObject name in the hierarchy (e.g. Crate_Alpha)")]
    public string targetObjectName;

    [Tooltip("The color the player must paint this object")]
    public Color targetColor;
}
