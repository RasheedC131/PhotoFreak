// RegionColorPalette.cs
// Designer-authored color combinations for all 8 recolorable regions.
// Create these as reusable assets and assign them to NPCs or drop into
// NPCAppearance.palettePool for randomized variants.

using UnityEngine;

[CreateAssetMenu(fileName = "RegionColorPalette", menuName = "NPC/Region Color Palette")]
public class RegionColorPalette : ScriptableObject
{
    [Header("Mask colors")]
    [Tooltip("Dress / shirt / body garment primary color.")]
    [ColorUsage(showAlpha: false)] public Color primaryColor = Color.white;
    [ColorUsage(showAlpha: false)] public Color stripeColor = Color.black;
    [ColorUsage(showAlpha: false)] public Color skinColor = new Color(0.92f, 0.78f, 0.65f);
    [ColorUsage(showAlpha: false)] public Color hairColor = new Color(0.25f, 0.15f, 0.10f);
    [ColorUsage(showAlpha: false)] public Color shoeColor = new Color(0.15f, 0.10f, 0.08f);
    [ColorUsage(showAlpha: false)] public Color eyeColor = new Color(0.30f, 0.50f, 0.70f);
    [ColorUsage(showAlpha: false)] public Color lipColor = new Color(0.75f, 0.35f, 0.40f);
}
