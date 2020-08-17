using UnityEngine;

public static class ColorUtility
{
    public static Color Lerped(this Color color, Color other, float factor)
    {
        return Color.Lerp(color, other, factor);
    }

    public static Color AlphaChangedTo(this Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}
