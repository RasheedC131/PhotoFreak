using UnityEngine;

// Attach this to a child GameObject on the NPC prefab.
// Unity will auto-add a ParticleSystem component via RequireComponent.
// Call Play() to trigger the effect — NPCIdentity does this automatically
// when the monster model is revealed.
//
// Setup:
//   1. Add a child GameObject to the NPC root and name it "MutationEffect".
//   2. Add this component to it — ParticleSystem is added automatically.
//   3. No further Inspector setup needed; everything is configured in code.
[RequireComponent(typeof(ParticleSystem))]
public class MutationEffect : MonoBehaviour
{
    [Header("Effect Tuning")]
    [Tooltip("How many circles orbit the NPC.")]
    [SerializeField] private int particleCount = 8;

    [Tooltip("Radius of the orbit ring around the NPC.")]
    [SerializeField] private float orbitRadius = 0.6f;

    [Tooltip("How fast the circles travel around the NPC (radians per second).")]
    [SerializeField] private float orbitSpeed = 3f;

    [Tooltip("How long the effect lasts in seconds.")]
    [SerializeField] private float duration = 2f;

    [Tooltip("Size of each circle.")]
    [SerializeField] private float particleSize = 0.12f;

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
        var main          = ps.main;
        main.duration     = duration;
        main.loop         = false;
        main.startLifetime  = duration;
        main.startSpeed     = 0f;           // orbital velocity drives movement, not start speed
        main.startSize      = particleSize;
        main.startColor     = Color.white;
        main.gravityModifier = 0f;
        // Local space so the orbital velocity circles around the NPC's own origin.
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.stopAction      = ParticleSystemStopAction.None;
        main.playOnAwake     = false;

        // ── Emission — one burst at t = 0 ────────────────────────────────────
        var emission = ps.emission;
        emission.enabled      = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, (short)particleCount)
        });

        // ── Shape — spawn evenly on the edge of a circle ─────────────────────
        // radiusThickness 0 = spawn only on the outer rim (a true ring).
        var shape          = ps.shape;
        shape.enabled      = true;
        shape.shapeType    = ParticleSystemShapeType.Circle;
        shape.radius       = orbitRadius;
        shape.radiusThickness = 0f;

        // ── Velocity over lifetime — orbit around Y axis ──────────────────────
        var vel      = ps.velocityOverLifetime;
        vel.enabled  = true;
        vel.space    = ParticleSystemSimulationSpace.Local;
        vel.orbitalY = orbitSpeed;

        // ── Color over lifetime — fade out in the last third ─────────────────
        var col      = ps.colorOverLifetime;
        col.enabled  = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.65f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(gradient);

        // ── Size over lifetime — shrink gently at the end ────────────────────
        var sizeOverLife    = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        sizeCurve.MoveKey(0, new Keyframe(0f,    1f));
        sizeCurve.MoveKey(1, new Keyframe(0.75f, 1f));
        sizeCurve.AddKey(1f, 0f);
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ── Renderer ──────────────────────────────────────────────────────────
        var renderer          = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode   = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 1;
    }
}
