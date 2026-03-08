using UnityEngine;
using System.Collections; 
// TODO: still need to scale tell chance exponetially when we have our ingame clock setup 

public class MonsterPathfinding : Pathfinding
{

    [Header("Monster Behavior")]
    [SerializeField] private float minTimeBetweenTells = 5f; 
    [SerializeField] private float maxTimeBetweenTells = 15f; 
    [Range(0f, 1f)]
    [SerializeField] private float tellTriggerProbability = 0.7f; 
    [SerializeField] private float stalkDuration = 8.0f; 
    [SerializeField] private float stalkDistance = 6.0f; // TODO: maybe make this random range


    private float nextTellTime = 0f; 
    private bool isGlitching = false; 
    private float defaultAngularSpeed; 
    private float currStalkTimer;
    private bool isStalking = true;

    protected override void Start()
    {
        base.Start(); 
        isInfected = true; 
        follower = false;
        currStalkTimer = stalkDuration; 
        
        defaultAngularSpeed = agent.angularSpeed; 
        // SetNextTellTime();
        Infect();
    }

    // public override void Run()
    // {
    //     // see if monster is able to do its tell 
    //     if (!isGlitching && Time.time >= nextTellTime) 
    //     {    
    //         if (Random.value <= tellTriggerProbability) StartCoroutine(TriggerTell());
    //         else SetNextTellTime(); 
    //     }

    //     // TODO: tweak or change this later based on in-game clock 
    //     // Snap to the player to reveal itself as a tell 
    //     if (!isGlitching)
    //     {
    //         // personalSpaceDist = 2.0f; 
    //         // agent.angularSpeed = defaultAngularSpeed; 
    //         base.Run(); 
    //     }
    //     else
    //     {
    //         personalSpaceDist = 2.0f; 
    //         agent.angularSpeed = defaultAngularSpeed; 
    //         base.Run(); 
    //     }
    // }

    public override void Run()
    {
        if (currVictim == null || currVictim.isInfected)
        {
            FindNewVictim();
            isStalking = true;
            currStalkTimer = stalkDuration;
            return;
        }

        float dist = Vector3.Distance(transform.position, currVictim.transform.position);

        if (isStalking)
        {
            currStalkTimer -= Time.deltaTime;

            // Move towards them
            agent.SetDestination(currVictim.transform.position);

            // tries to not get too close to victim 
            if (dist < stalkDistance)
            {
                agent.isStopped = true; 
                transform.LookAt(currVictim.transform); 
            }
            else
            {
                agent.isStopped = false; // Catch up if they run away
            }

            Debug.DrawLine(transform.position, currVictim.transform.position, Color.yellow);

            if (currStalkTimer <= 0)
            {
                isStalking = false;
                agent.isStopped = false; 
                Debug.Log("Monster is now attacking");
            }
        }
        // kill state
        else
        {
            agent.SetDestination(currVictim.transform.position);
            Debug.DrawLine(transform.position, currVictim.transform.position, Color.red);

            // Infect if close enough
            if (dist < 1.5f)
            {
                currVictim.Infect();
                currVictim = null; 
            }
        }
    }

    private void SetNextTellTime()
    {
        nextTellTime = Time.time + Random.Range(minTimeBetweenTells, maxTimeBetweenTells);
    }

    private IEnumerator TriggerTell()
    {
        isGlitching = true;
        
        int tellType = Random.Range(0, 3);

        switch (tellType)
        {
            // come close 
            case 0: 
                personalSpaceDist = 0.5f; 
                break;
            // snap quickly
            case 1: 
                agent.angularSpeed = 10000f; 
                break;
            // 
            case 2: 
                agent.isStopped = true;
                yield return new WaitForSeconds(0.4f); 
                agent.isStopped = false;
                break;
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        isGlitching = false;
        SetNextTellTime();
    }

    private void FindNewVictim()
    {
        Pathfinding[] allAgents = FindObjectsByType<Pathfinding>(FindObjectsSortMode.None);
        
        float closestDist = Mathf.Infinity;
        Pathfinding bestCandidate = null;

        foreach (var a in allAgents)
        {
            if (!a.isInfected && a != this)
            {
                float d = Vector3.Distance(transform.position, a.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    bestCandidate = a;
                }
            }
        }

        currVictim = bestCandidate;
        if (currVictim != null)
             Debug.Log("Monster selected new target: " + currVictim.name);
    }
}
