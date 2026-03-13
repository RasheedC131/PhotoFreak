using UnityEngine;
using System.Collections.Generic; 
using System.Collections; 
// TODO: still need to scale tell chance exponetially when we have our ingame clock setup 

public class MonsterPathfinding : Pathfinding
{

    [Header("Monster Behavior")]
    [SerializeField] private float minTimeBetweenTells = 5f; 
    [SerializeField] private float maxTimeBetweenTells = 15f; 
    [Range(0f, 1f)]
    [SerializeField] private float tellTriggerProbability = 0.7f; 
    [SerializeField] private float stalkDuration = 1.0f; 
    [SerializeField] private float stalkDistance = 10.0f; // TODO: maybe make this random range
    [SerializeField] public GameObject defaultModel;
    [SerializeField] public GameObject killModel;


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
        
        if (defaultModel != null) defaultModel.SetActive(true);
        if (killModel != null) killModel.SetActive(false);
        
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
            
            if (defaultModel != null) defaultModel.SetActive(true);
            if (killModel != null) killModel.SetActive(false);
            
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
            Debug.DrawLine(transform.position, currVictim.transform.position , Color.yellow);

            if (currStalkTimer <= 0)
            {
                isStalking = false;
                agent.isStopped = false; 
                
                if (defaultModel != null) defaultModel.SetActive(false);
                if (killModel != null) killModel.SetActive(true);
                
                Debug.Log("Monster is now attacking");
            }
        }
        // kill state
        else
        {
            agent.SetDestination(currVictim.transform.position);
            Debug.DrawLine(transform.position, currVictim.transform.position, Color.red);
            
            if (defaultModel != null && defaultModel.activeSelf) defaultModel.SetActive(false);
            if (killModel != null && !killModel.activeSelf) killModel.SetActive(true);

            // Infect if close enough
            if (dist < 1.5f)
            {
                currVictim.Infect(this);
                currVictim = null; 
            }
        }
    }

    public override void Infect(Pathfinding attacker = null)
    {
        ApplyStandardInfection(); 
        currStalkTimer = stalkDuration; 
        isGlitching = false; 
        
        if (defaultModel != null) defaultModel.SetActive(true);
        if (killModel != null) killModel.SetActive(false);
        
        FindNewVictim(); 
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
        
        List<Pathfinding> independentTargets = new List<Pathfinding>();
        List<Pathfinding> groupedTargets = new List<Pathfinding>();

        foreach (var a in allAgents)
        {
            // prioritize independent guest first 
            if (!a.isInfected && a != this)
            {
                if (!a.isBusy && a.customLeader == null) 
                {
                    independentTargets.Add(a); 
                }
                else 
                {
                    groupedTargets.Add(a);     
                }
            }
        }

        List<Pathfinding> validTargets = (independentTargets.Count > 0) ? independentTargets : groupedTargets;

        float closestDist = Mathf.Infinity;
        Pathfinding bestCandidate = null;

        foreach (var target in validTargets)
        {
            float d = Vector3.Distance(transform.position, target.transform.position);
            if (d < closestDist)
            {
                closestDist = d;
                bestCandidate = target;
            }
        }

        currVictim = bestCandidate;
        
        if (currVictim != null)
             Debug.Log("Monster selected target: " + currVictim.name + " [Type: " + (independentTargets.Count > 0 ? "Independent" : "Grouped") + "]");
    }
}