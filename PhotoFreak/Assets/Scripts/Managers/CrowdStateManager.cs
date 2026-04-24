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

    [Header("Panic Ripple")]
    [SerializeField] private float rippleRadius = 12f;
    [SerializeField] private float ripplePanicAmount = 0.80f;
    [SerializeField] private float rippleVigilanceGain = 0.25f;

    [Header("Strike 2")]
    [SerializeField] private float scatterDelay = 0.6f;
    [SerializeField] private float scatterVigilanceSpike = 0.50f;
    [SerializeField] private float scatterPanicSpike = 0.90f;


    private float alertLevel = 0f;
    private float alertFloor = 0f;
    private bool  isFinalPanicTriggered = false;
    public float AlertLevel  => alertLevel;
    public bool IsFinalPanic { get; private set; } = false;

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
        alertLevel = 1.0f;
        if (totalStrikes >= 2) alertFloor = strike2AlertFloor;
        else if (totalStrikes >= 1) alertFloor = strike1AlertFloor;

        TriggerPanicRipple();

        if (totalStrikes == 2)
            StartCoroutine(MassScatterRoutine());
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

}
