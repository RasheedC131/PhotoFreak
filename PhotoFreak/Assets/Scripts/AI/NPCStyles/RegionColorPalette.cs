// creates a color palette for our npcs 

using UnityEngine;

[CreateAssetMenu(fileName = "RegionColorPalette", menuName = "NPC/Region Color Palette")]
public class RegionColorPalette : ScriptableObject
{
    [Header("Mask colors")]
    [Tooltip("Dress / shirt / body garment primary color.")]
    [ColorUsage(showAlpha: false)] public Color primaryColor;
    [ColorUsage(showAlpha: false)] public Color secondaryColor;
    [ColorUsage(showAlpha: false)] public Color skinColor;
    [ColorUsage(showAlpha: false)] public Color hairColor;
    [ColorUsage(showAlpha: false)] public Color shoeColor;
    [ColorUsage(showAlpha: false)] public Color eyeColor;
    [ColorUsage(showAlpha: false)] public Color lipColor;
}
