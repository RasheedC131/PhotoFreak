using UnityEngine;
using System.Collections;

// Tracks the alert level of the npcs and react to each strike from the freak meter
public class CrowdStateManager : MonoBehaviour
{
    public static CrowdStateManager Instance { get; private set; }

    [Header("Alert Level")]
    [SerializeField] private float alertDecayRate = 0.04f;
    [SerializeField] private float strike1AlertFloor = 0.30f;
    [SerializeField] private float strike2AlertFloor = 0.65f;
    [SerializeField] private float strike3AlertFloor = 1.00f;

    [Header("Panic Ripple")]
    [SerializeField] private float rippleRadius = 12f;
    [SerializeField] private float ripplePanicAmount = 0.80f;
    [SerializeField] private float rippleVigilanceGain = 0.25f;

    [Header("Strike 2")]
    [SerializeField] private float scatterDelay = 0.6f;
    [SerializeField] private float scatterVigilanceSpike = 0.50f;
    [SerializeField] private float scatterPanicSpike = 0.90f;

    [Header("Per-Strike Player Avoidance")]
    [Tooltip("Radius (m) within which NPCs treat the player as a threat once they have N strikes. " +
             "Index 0 = no strikes (use GuestSettings.fleePlayerSightRadius), " +
             "1 = first strike (5m), 2 = second strike (10m), 3 = final strike (close-range only).")]
    [SerializeField] private float[] playerAvoidRadiiByStrike = new float[] { 0f, 5f, 10f, 3f };

    [Header("Per-Strike Vigilance Bump")]
    [Tooltip("Additive vigilance applied to every guest each time the player earns a strike.")]
    [SerializeField] private float vigilancePerStrike = 0.20f;

    [Header("Monster Kill Mode (Strike 3)")]
    [Tooltip("When the player hits the final strike, all monsters enter kill mode and aggressively " +
             "target the nearest non-monster.")]
    [SerializeField] private float killModeStalkSpeedMultiplier = 1.35f;

    [Header("Monster Majority (Player Targeting)")]
    [Tooltip("Fraction of NPCs that must be monsters before any monster is allowed to target the player. " +
             "0.5 = 50%.")]
    [SerializeField] private float monsterMajorityThreshold = 0.5f;
    [Tooltip("How often (seconds) to recompute the monster-majority cache. 0 = every frame.")]
    [SerializeField] private float monsterMajorityRefreshInterval = 1.0f;

    private float alertLevel = 0f;
    private float alertFloor = 0f;
    private bool  isFinalPanicTriggered = false;

    private int   currentStrikes = 0;
    private bool  isKillMode = false;
    private bool  monsterMajorityCached = false;
    private float monsterMajorityNextRefresh = 0f;

    public float AlertLevel  => alertLevel;
    public bool  IsFinalPanic { get; private set; } = false;

    /// <summary>Number of strikes the player currently has. Updated by FreakMeter.OnStrikeEarned.</summary>
    public int CurrentStrikes => currentStrikes;

    /// <summary>True after the player has hit their final strike. Monsters drop into kill mode.</summary>
    public bool IsKillMode => isKillMode;

    /// <summary>True when more than <see cref="monsterMajorityThreshold"/> of NPCs are monsters.</summary>
    public bool MonsterMajority
    {
        get
        {
            if (Time.time >= monsterMajorityNextRefresh)
            {
                monsterMajorityCached = ComputeMonsterMajority();
                monsterMajorityNextRefresh = Time.time + monsterMajorityRefreshInterval;
            }
            return monsterMajorityCached;
        }
    }


    public float PlayerAvoidRadius
    {
        get
        {
            if (playerAvoidRadiiByStrike == null || playerAvoidRadiiByStrike.Length == 0) return 0f;
            int idx = Mathf.Clamp(currentStrikes, 0, playerAvoidRadiiByStrike.Length - 1);
            return playerAvoidRadiiByStrike[idx];
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        FreakMeter.OnStrikeEarned += HandleStrikeEarned;
    }

    void OnDisable()
    {
        FreakMeter.OnStrikeEarned -= HandleStrikeEarned;
    }

    void Update()
    {
        if (GlobalGameState.Instance != null &&
            GlobalGameState.Instance.currentState != GlobalGameState.GameState.PLAYING) return;

        if (alertLevel > alertFloor)
            alertLevel = Mathf.Max(alertFloor, alertLevel - alertDecayRate * Time.deltaTime);
    }


    private void HandleStrikeEarned(int totalStrikes)
    {
        currentStrikes = totalStrikes;
        alertLevel = 1.0f;

        if      (totalStrikes >= 3) alertFloor = strike3AlertFloor;
        else if (totalStrikes >= 2) alertFloor = strike2AlertFloor;
        else if (totalStrikes >= 1) alertFloor = strike1AlertFloor;

        // Every strike additively boosts every guest's vigilance.
        BumpAllGuestVigilance(vigilancePerStrike);

        TriggerPanicRipple();

        if (totalStrikes == 2)
            StartCoroutine(MassScatterRoutine());

        if (totalStrikes >= 3)
            EnterKillMode();
    }

    /// <summary>Adds <paramref name="amount"/> to every (non-monster) AIContext's vigilance.</summary>
    private void BumpAllGuestVigilance(float amount)
    {
        if (amount <= 0f) return;

        AIContext[] all = FindObjectsOfType<AIContext>();
        foreach (AIContext ctx in all)
        {
            if (ctx == null || ctx.isMonster) continue;
            ctx.vigilance = Mathf.Min(1f, ctx.vigilance + amount);
        }
    }

    // creates a sphere around each AI to detect if player
    public void SpreadPanic(Vector3 origin, float radius, float panicAmount, float vigilanceGain)
    {
        Collider[] hits = Physics.OverlapSphere(origin, radius);
        foreach (Collider col in hits)
        {
            AIContext ctx = col.GetComponentInParent<AIContext>();
            if (ctx == null || ctx.isMonster) continue;

            ctx.panicBoost  = Mathf.Min(1f, ctx.panicBoost  + panicAmount);
            ctx.vigilance   = Mathf.Min(1f, ctx.vigilance   + vigilanceGain);
        }
    }


    public void TriggerFinalPanic()
    {
        if (isFinalPanicTriggered) return;
        isFinalPanicTriggered = true;
        IsFinalPanic = true;

        // Flood every guest to maximum panic and break all social groups.
        AIContext[] allContexts = FindObjectsOfType<AIContext>();
        foreach (AIContext ctx in allContexts)
        {
            if (ctx.isMonster) continue;
            ctx.panicBoost = 1f;
            ctx.vigilance  = 1f;
        }

        ActionSocialize[] allSocial = FindObjectsOfType<ActionSocialize>();
        foreach (ActionSocialize social in allSocial)
        {
            if (social == null) continue;
            AIContext ctx = social.GetComponentInParent<AIContext>();
            if (ctx != null && !ctx.isMonster) social.LeaveGroup();
        }

        // Kill mode follows from the final strike too — guarantee it's on even if
        // somebody calls TriggerFinalPanic without going through HandleStrikeEarned.
        EnterKillMode();

        Debug.Log("[CrowdStateManager] Final panic state active — monsters may now hunt the player.");
    }


    private void TriggerPanicRipple()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        SpreadPanic(player.transform.position, rippleRadius, ripplePanicAmount, rippleVigilanceGain);
    }

    private IEnumerator MassScatterRoutine()
    {
        // Small delay so the ripple visually plays out first.
        yield return new WaitForSeconds(scatterDelay);

        // Break every active social group and inject panic into every guest.
        ActionSocialize[] allSocial = FindObjectsOfType<ActionSocialize>();
        foreach (ActionSocialize social in allSocial)
        {
            if (social == null) continue;
            AIContext ctx = social.GetComponentInParent<AIContext>();
            if (ctx == null || ctx.isMonster) continue;

            social.LeaveGroup();
            ctx.vigilance  = Mathf.Min(1f, ctx.vigilance  + scatterVigilanceSpike);
            ctx.panicBoost = Mathf.Min(1f, ctx.panicBoost + scatterPanicSpike);
        }

        Debug.Log("[CrowdStateManager] Strike 2 — Mass Scatter complete.");
    }

    // ------------------------------------------------------------------
    // Kill Mode (strike 3)
    // ------------------------------------------------------------------

    /// <summary>
    /// Flips every monster into kill mode: forces them to acquire a victim immediately,
    /// boosts their stalk speed, and clears any "got bored" cooldown.
    /// </summary>
    private void EnterKillMode()
    {
        if (isKillMode) return;
        isKillMode = true;

        AIContext[] all = FindObjectsOfType<AIContext>();

        foreach (AIContext ctx in all)
        {
            if (ctx == null || !ctx.isMonster) continue;

            // Reset any "bored" timer so the next consideration tick keeps them aggressive.
            ctx.currentStalkTimer = 0f;

            // Bump nav-agent speed so the kill mode feels lethal.
            UnityEngine.AI.NavMeshAgent agent = ctx.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = Mathf.Max(agent.speed, agent.speed * killModeStalkSpeedMultiplier);
            }

            // If a monster has no victim, force-assign the closest non-monster so they
            // start hunting NPCs the moment kill mode starts.
            if (ctx.currentVictim == null)
            {
                AIContext nearest = FindNearestGuest(ctx, all);
                if (nearest != null)
                {
                    ctx.currentVictim = nearest;
                    nearest.currentStalker = ctx;
                    nearest.AddStalker();
                }
            }
        }

        Debug.Log("[CrowdStateManager] Strike 3 — monsters entered kill mode and are targeting NPCs.");
    }

    private static AIContext FindNearestGuest(AIContext from, AIContext[] all)
    {
        AIContext best = null;
        float bestSqr = float.MaxValue;

        foreach (AIContext ctx in all)
        {
            if (ctx == null || ctx == from || ctx.isMonster) continue;
            // Don't poach a victim that's already being stalked by another monster.
            if (ctx.isBeingStalked && ctx.currentStalker != from) continue;

            float sqr = (ctx.transform.position - from.transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = ctx;
            }
        }
        return best;
    }

    // ------------------------------------------------------------------
    // Monster Majority
    // ------------------------------------------------------------------

    private bool ComputeMonsterMajority()
    {
        AIContext[] all = FindObjectsOfType<AIContext>();
        if (all == null || all.Length == 0) return false;

        int monsters = 0;
        int total    = 0;

        foreach (AIContext ctx in all)
        {
            if (ctx == null) continue;
            total++;
            if (ctx.isMonster) monsters++;
        }

        if (total == 0) return false;
        return ((float)monsters / total) > monsterMajorityThreshold;
    }
}
