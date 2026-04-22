using UnityEngine;

// takes in the pallete and apply colors based on the texture map for the NPCs 
[DisallowMultipleComponent]
public class NPCAppearance : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private FacialExpressionLibrary expressionLibrary;

    [Tooltip("Optional. If populated, one library is picked at random on Awake.")]
    [SerializeField] private FacialExpressionLibrary[] libraryPool;

    [Tooltip("Optional. If populated, one palette is picked at random on Awake.")]
    [SerializeField] private RegionColorPalette[] palettePool;

    [Header("Shader Property Names")]
    [SerializeField] private string baseTexturePropertyName  = "_BaseMap";
    [SerializeField] private string maskTexturePropertyName  = "_MaskMap";
    [SerializeField] private string primaryColorPropertyName   = "_PrimaryColor";
    [SerializeField] private string stripeColorPropertyName    = "_StripeColor";
    [SerializeField] private string skinColorPropertyName      = "_SkinColor";
    [SerializeField] private string hairColorPropertyName      = "_HairColor";
    [SerializeField] private string shoeColorPropertyName      = "_ShoeColor";
    [SerializeField] private string eyeColorPropertyName       = "_EyeColor";
    [SerializeField] private string lipColorPropertyName       = "_LipColor";

    [Header("Initial State")]
    [SerializeField] private FacialExpression initialExpression = FacialExpression.Neutral;
    [SerializeField] private RegionColorPalette initialPalette; 

    // Cached MPB and property IDs
    private MaterialPropertyBlock mpb;
    private int baseTexId, maskTexId, maskTex2Id;
    private int primaryId, stripeId, skinId, hairId;
    private int shoeId, eyeId, lipId;

    private FacialExpression currentExpression;
    private Color currentPrimary, currentStripe, currentSkin, currentHair;
    private Color currentShoe, currentAccessory, currentEye, currentLip;

    public FacialExpression CurrentExpression => currentExpression;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();

        baseTexId   = Shader.PropertyToID(baseTexturePropertyName);
        maskTexId   = Shader.PropertyToID(maskTexturePropertyName);
        primaryId   = Shader.PropertyToID(primaryColorPropertyName);
        stripeId    = Shader.PropertyToID(stripeColorPropertyName);
        skinId      = Shader.PropertyToID(skinColorPropertyName);
        hairId      = Shader.PropertyToID(hairColorPropertyName);
        shoeId      = Shader.PropertyToID(shoeColorPropertyName);
        eyeId       = Shader.PropertyToID(eyeColorPropertyName);
        lipId       = Shader.PropertyToID(lipColorPropertyName);

        mpb = new MaterialPropertyBlock();

        if (libraryPool != null && libraryPool.Length > 0) expressionLibrary = libraryPool[Random.Range(0, libraryPool.Length)];

        RegionColorPalette paletteToUse = initialPalette;

        if (palettePool != null && palettePool.Length > 0) paletteToUse = palettePool[Random.Range(0, palettePool.Length)];

        ApplyInitial(paletteToUse);
    }

    private void ApplyInitial(RegionColorPalette p)
    {
        targetRenderer.GetPropertyBlock(mpb);

        if (expressionLibrary != null && expressionLibrary.TryGet(initialExpression, out var entry))
        {
            if (entry.texture != null)      mpb.SetTexture(baseTexId,  entry.texture);
            if (entry.maskTexture != null)  mpb.SetTexture(maskTexId,  entry.maskTexture);
        }

        mpb.SetColor(primaryId,   p.primaryColor);
        mpb.SetColor(stripeId,    p.stripeColor);
        mpb.SetColor(skinId,      p.skinColor);
        mpb.SetColor(hairId,      p.hairColor);
        mpb.SetColor(shoeId,      p.shoeColor);
        mpb.SetColor(eyeId,       p.eyeColor);
        mpb.SetColor(lipId,       p.lipColor);


        targetRenderer.SetPropertyBlock(mpb);

        currentExpression = initialExpression;
        currentPrimary    = p.primaryColor;
        currentStripe     = p.stripeColor;
        currentSkin       = p.skinColor;
        currentHair       = p.hairColor;
        currentShoe       = p.shoeColor;
        currentEye        = p.eyeColor;
        currentLip        = p.lipColor;
    }

    public void SetExpression(FacialExpression expression)
    {
        if (expression == currentExpression) return;
        if (expressionLibrary == null) return;
        if (!expressionLibrary.TryGet(expression, out var entry)) return;

        targetRenderer.GetPropertyBlock(mpb);
        if (entry.texture != null)      mpb.SetTexture(baseTexId,  entry.texture);
        if (entry.maskTexture != null)  mpb.SetTexture(maskTexId,  entry.maskTexture);
        targetRenderer.SetPropertyBlock(mpb);

        currentExpression = expression;
    }

    public void SetPrimaryColor(Color c)   { SetSingleColor(primaryId,   c, ref currentPrimary); }
    public void SetStripeColor(Color c)    { SetSingleColor(stripeId,    c, ref currentStripe); }
    public void SetSkinColor(Color c)      { SetSingleColor(skinId,      c, ref currentSkin); }
    public void SetHairColor(Color c)      { SetSingleColor(hairId,      c, ref currentHair); }
    public void SetShoeColor(Color c)      { SetSingleColor(shoeId,      c, ref currentShoe); }
    public void SetEyeColor(Color c)       { SetSingleColor(eyeId,       c, ref currentEye); }
    public void SetLipColor(Color c)       { SetSingleColor(lipId,       c, ref currentLip); }

    private void SetSingleColor(int propId, Color c, ref Color cache)
    {
        if (c == cache) return;
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(propId, c);
        targetRenderer.SetPropertyBlock(mpb);
        cache = c;
    }

    public void ApplyPalette(RegionColorPalette palette)
    {
        if (palette == null) return;

        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(primaryId,   palette.primaryColor);
        mpb.SetColor(stripeId,    palette.stripeColor);
        mpb.SetColor(skinId,      palette.skinColor);
        mpb.SetColor(hairId,      palette.hairColor);
        mpb.SetColor(shoeId,      palette.shoeColor);
        mpb.SetColor(eyeId,       palette.eyeColor);
        mpb.SetColor(lipId,       palette.lipColor);
        targetRenderer.SetPropertyBlock(mpb);

        currentPrimary   = palette.primaryColor;
        currentStripe    = palette.stripeColor;
        currentSkin      = palette.skinColor;
        currentHair      = palette.hairColor;
        currentShoe      = palette.shoeColor;
        currentEye       = palette.eyeColor;
        currentLip       = palette.lipColor;
    }

    public void SetLibrary(FacialExpressionLibrary library, FacialExpression? overrideExpression = null)
    {
        if (library == null) return;
        expressionLibrary = library;

        var target = overrideExpression ?? currentExpression;
        if (!library.TryGet(target, out var entry))
        {
            if (!library.TryGet(initialExpression, out entry)) return;
            target = initialExpression;
        }

        targetRenderer.GetPropertyBlock(mpb);
        if (entry.texture != null)      mpb.SetTexture(baseTexId,  entry.texture);
        if (entry.maskTexture != null)  mpb.SetTexture(maskTexId,  entry.maskTexture);
        targetRenderer.SetPropertyBlock(mpb);

        currentExpression = target;
    }
}
