using UnityEngine;
using UnityEngine.AI;

public class ActionAttack : UtilityAction
{
    private NavMeshAgent agent;
    private AIContext ctx;
    private NPCIdentity myIdentity; 
    private NPCIdentity victimIdentity; 
    
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;

    void Awake()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        ctx = GetComponentInParent<AIContext>();
        myIdentity = GetComponentInParent<NPCIdentity>();
    }

    public override void ExecuteAction()
    {
        if (ctx.currentVictim == null || ctx.currentVictim.isMonster) return;
        
        agent.isStopped = false;
        
        if (myIdentity is not null) myIdentity.ShowMonsterModel(); 

        victimIdentity = ctx.currentVictim.GetComponent<NPCIdentity>(); 
        if (victimIdentity is not null) victimIdentity.Mutate(true); 

        Debug.Log($"Monster: [{ctx.gameObject.name}] infected: [{ctx.currentVictim.gameObject.name}]"); 
        
        ctx.currentVictim = null; 
    }
}