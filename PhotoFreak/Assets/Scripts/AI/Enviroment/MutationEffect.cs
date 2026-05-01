using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class MutationEffect : MonoBehaviour
{
    [Header("Effect Tuning")]
    [SerializeField] private int particleCount = 20;

    [SerializeField] private float burstSpeed = 2.5f;


    [SerializeField] private float duration = 1.5f;

    [SerializeField] private float particleSize = 0.15f;


    [SerializeField] private float gravity = 0.4f;


    [SerializeField] private Color particleColor = Color.white;

    [SerializeField] private Material squareMaterial;

    public float Duration => duration;

    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        Configure();
    }

    public void Play()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }

    private void Configure()
    {
        // ── Main ──────────────────────────────────────────────────────────────
        var main             = ps.main;
        main.duration        = duration;
        main.loop            = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(duration * 0.5f, duration);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(burstSpeed * 0.5f, burstSpeed);
        main.startSize       = new ParticleSystem.MinMaxCurve(particleSize * 0.6f, particleSize * 1.4f);
        main.startColor      = particleColor;
        main.gravityModifier = gravity;
        // World space so squares fly out and fall independent of the NPC's movement.
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction      = ParticleSystemStopAction.None;
        main.playOnAwake     = false;
        // Random starting rotation so each square lands at a different angle.
        main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

        // ── Emission — single burst at t = 0 ─────────────────────────────────
        var emission          = ps.emission;
        emission.enabled      = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, (short)particleCount)
        });

        // ── Shape — small sphere so squares scatter in all directions ─────────
        var shape          = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.Sphere;
        shape.radius       = 0.25f;

        // ── Velocity over lifetime — disabled (burst speed drives movement) ───
        var vel     = ps.velocityOverLifetime;
        vel.enabled = false;

        // ── Color over lifetime — hold full alpha, then fade out ──────────────
        var col      = ps.colorOverLifetime;
        col.enabled  = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(particleColor, 0f),
                new GradientColorKey(particleColor, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(gradient);

        // ── Size over lifetime — hold size then shrink at the end ─────────────
        var sizeOverLife     = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f,    1f);
        sizeCurve.AddKey(0.7f,  1f);
        sizeCurve.AddKey(1f,    0f);
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ── Rotation over lifetime — spin each square for visual interest ──────
        var rotOverLife     = ps.rotationOverLifetime;
        rotOverLife.enabled = true;
        rotOverLife.z       = new ParticleSystem.MinMaxCurve(-90f * Mathf.Deg2Rad, 90f * Mathf.Deg2Rad);

        // ── Renderer — Billboard so squares always face the camera ────────────
        var rend          = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode   = ParticleSystemRenderMode.Billboard;
        rend.sortingOrder = 1;

        // Assign material. Sprites/Default renders as a solid flat quad with no
        // circular texture, giving hard-edged squares. If the user has assigned
        // their own material in the inspector that takes priority.
        if (squareMaterial != null)
        {
            rend.material = squareMaterial;
        }
        else
        {
            // Try URP particles unlit first, then fall back to Sprites/Default.
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");

            if (shader != null)
            {
                Material mat   = new Material(shader);
                mat.color      = particleColor;
                rend.material  = mat;
            }
        }
    }
}
