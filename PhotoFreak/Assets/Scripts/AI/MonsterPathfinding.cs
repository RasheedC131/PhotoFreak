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
    private float defaultSpeed;         // used for the tell where it goes faster
    private float defaultAngularSpeed; 
    private float currStalkTimer;
    private bool isStalking = true;

    protected override void Start()
    {
        base.Start(); 
        isInfected = true; 
        follower = false;
        currStalkTimer = stalkDuration; 
        defaultSpeed = agent.speed;
        defaultAngularSpeed = agent.angularSpeed; 
        // SetNextTellTime();
        Infect();
    }

    public override void Run()
    {
        if (isStalking && !isGlitching && Time.time >= nextTellTime)
        {
            if (Random.value <= tellTriggerProbability) StartCoroutine(TriggerTell()); 
            SetNextTellTime(); 
        }

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

            // only update movement of the AI when the tell isn't being triggered 
            if (!isGlitching)
            {
                agent.SetDestination(currVictim.transform.position);

                if (dist < stalkDistance)
                {
                    agent.isStopped = true; 
                    transform.LookAt(currVictim.transform); 
                }
                else
                {
                    agent.isStopped = false; 
                }
            }

            // Draw Stalk Line
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
        Debug.Log("Monster Tell Triggered: " + tellType);

        switch (tellType)
        {
           // snap to victim 
            case 0: 
                float oldAngular = agent.angularSpeed;
                agent.angularSpeed = 10000f; 
                
                if(currVictim != null) agent.SetDestination(currVictim.transform.position);                
                yield return new WaitForSeconds(0.5f);
                agent.angularSpeed = oldAngular;
                break;

            // moves really fast 
            case 1: 
                agent.speed = defaultSpeed * 4.0f; 
                yield return new WaitForSeconds(0.25f); 
                agent.speed = defaultSpeed;
                agent.acceleration = 8f;          
                break;

            // Twitch (rotates a bit)
            case 2: 
                agent.updateRotation = false; 
                float duration = 0.6f;
                float timer = 0f;
                
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float spasm = Random.Range(-30f, 30f);
                    transform.Rotate(0, spasm, 0);
                    yield return null; 
                }
                
                agent.updateRotation = true; 
                break;
        }

        isGlitching = false;
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
