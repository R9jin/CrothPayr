using UnityEngine;

public static class GameColors
{
    public static readonly Color[] Palette = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        Color.white
    };

    public static readonly string[] Names = new string[]
    {
        "Red",
        "Blue",
        "Green",
        "Yellow",
        "Cyan",
        "Magenta",
        "White"
    };

    public static string GetColorName(Color color)
    {
        for (int i = 0; i < Palette.Length; i++)
        {
            if (ColorsMatch(Palette[i], color))
                return Names[i];
        }
        return "Custom Color";
    }

    public static bool ColorsMatch(Color a, Color b)
    {
        // Allow a small tolerance due to float precision or material variance
        return Mathf.Abs(a.r - b.r) < 0.05f &&
               Mathf.Abs(a.g - b.g) < 0.05f &&
               Mathf.Abs(a.b - b.b) < 0.05f;
    }
}
