using UnityEngine;

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
    [SerializeField] private string primaryColorPropertyName = "_PrimaryColor";
    [SerializeField] private string stripeColorPropertyName  = "_StripeColor";
    [SerializeField] private string skinColorPropertyName    = "_SkinColor";
    [SerializeField] private string hairColorPropertyName    = "_HairColor";
    [SerializeField] private string shoeColorPropertyName    = "_ShoeColor";
    [SerializeField] private string eyeColorPropertyName     = "_EyeColor";
    [SerializeField] private string lipColorPropertyName     = "_LipColor";

    [Header("Initial State")]
    [SerializeField] private FacialExpression initialExpression = FacialExpression.Neutral;
    [SerializeField] private RegionColorPalette initialPalette; 

    private Material mat;
    private int baseTexId, maskTexId;
    private int primaryId, stripeId, skinId, hairId;
    private int shoeId, eyeId, lipId;

    private FacialExpression currentExpression;
    private Color currentPrimary, currentStripe, currentSkin, currentHair;
    private Color currentShoe, currentEye, currentLip;

    public FacialExpression CurrentExpression => currentExpression;

    private void Start()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();

        mat = targetRenderer.material;

        baseTexId   = Shader.PropertyToID(baseTexturePropertyName);
        maskTexId   = Shader.PropertyToID(maskTexturePropertyName);
        primaryId   = Shader.PropertyToID(primaryColorPropertyName);
        stripeId    = Shader.PropertyToID(stripeColorPropertyName);
        skinId      = Shader.PropertyToID(skinColorPropertyName);
        hairId      = Shader.PropertyToID(hairColorPropertyName);
        shoeId      = Shader.PropertyToID(shoeColorPropertyName);
        eyeId       = Shader.PropertyToID(eyeColorPropertyName);
        lipId       = Shader.PropertyToID(lipColorPropertyName);

        if (libraryPool != null && libraryPool.Length > 0) 
            expressionLibrary = libraryPool[Random.Range(0, libraryPool.Length)];

        RegionColorPalette paletteToUse = initialPalette;

        if (palettePool != null && palettePool.Length > 0) 
            paletteToUse = palettePool[Random.Range(0, palettePool.Length)];

        ApplyInitial(paletteToUse);
    }

    private void ApplyInitial(RegionColorPalette p)
    {
        if (expressionLibrary != null && expressionLibrary.TryGet(initialExpression, out var entry))
        {
            if (entry.texture != null)      mat.SetTexture(baseTexId,  entry.texture);
            if (entry.maskTexture != null)  mat.SetTexture(maskTexId,  entry.maskTexture);
        }

        mat.SetColor(primaryId,   p.primaryColor);
        mat.SetColor(stripeId,    p.secondaryColor);
        mat.SetColor(skinId,      p.skinColor);
        mat.SetColor(hairId,      p.hairColor);
        mat.SetColor(shoeId,      p.shoeColor);
        mat.SetColor(eyeId,       p.eyeColor);
        mat.SetColor(lipId,       p.lipColor);

        currentExpression = initialExpression;
        currentPrimary    = p.primaryColor;
        currentStripe     = p.secondaryColor;
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

        if (entry.texture != null)      mat.SetTexture(baseTexId,  entry.texture);
        if (entry.maskTexture != null)  mat.SetTexture(maskTexId,  entry.maskTexture);

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
        mat.SetColor(propId, c);
        cache = c;
    }

    public void ApplyPalette(RegionColorPalette p)
    {
        if (p == null) return;

        mat.SetColor(primaryId,   p.primaryColor);
        mat.SetColor(stripeId,    p.secondaryColor);
        mat.SetColor(skinId,      p.skinColor);
        mat.SetColor(hairId,      p.hairColor);
        mat.SetColor(shoeId,      p.shoeColor);
        mat.SetColor(eyeId,       p.eyeColor);
        mat.SetColor(lipId,       p.lipColor);

        currentPrimary   = p.primaryColor;
        currentStripe    = p.secondaryColor;
        currentSkin      = p.skinColor;
        currentHair      = p.hairColor;
        currentShoe      = p.shoeColor;
        currentEye       = p.eyeColor;
        currentLip       = p.lipColor;
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

        if (entry.texture != null)      mat.SetTexture(baseTexId,  entry.texture);
        if (entry.maskTexture != null)  mat.SetTexture(maskTexId,  entry.maskTexture);

        currentExpression = target;
    }

    private void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }
}