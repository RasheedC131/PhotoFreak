using UnityEngine;
using UnityEngine.AI;

public class Pathfinding : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public bool loopTmp;
    public Transform pathsContainer;     // grabbed from the manager 
    public Material monsterMaterial;
    public Renderer myRenderer; 
    private bool playerInside;



    [Header("Current NPC Status")]
    public bool isInfected = false;  // if the guest is turned into a monster 
    public bool isBusy = false;     // use for the AIManager
    public Pathfinding customLeader; // used for a dynamic defaultLeader
    public Pathfinding currVictim;   // who is being chased by monster 
    public int groupIdx = 0; 
    public int groupTotalSize = 1; 

    public bool follower;
    protected Transform ring;
    protected Transform node;
    protected Transform[] rings;
    protected Transform defaultLeader; // renamed it to avoid confusion with the new logic defaultLeader vars 
    protected Vector3 destination;
    protected float personalSpaceDist = 2.0f; 
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(name + ": Missing NavMeshAgent");
            return;
        }

        SetupNavigation(); 

        if (isInfected) Infect(); 

        agent.avoidancePriority = Random.Range(30, 70);
    }

    public virtual void Run()
    {
        // monster logic 
        if (isInfected)
        {
            Vector3 pos = leader.GetComponent<Pathfinding>().getBehind(); // get position of leader
            pos = new Vector3(pos.x, 0, pos.z);
            agent.SetDestination(pos);
            new WaitForSeconds(2);
        }

        // group logic (socializing with guest)
        if (customLeader != null)
        {
            Vector3 socialSpot = GetSocialPosition(); 
            float dist = Vector3.Distance(transform.position, socialSpot);

            // move to spot with tolerance 
            if (dist > 1.0f) 
            {
                agent.isStopped = false;
                agent.SetDestination(socialSpot);
            }

            // once guest ai is within range stop moving and "talk"
            else
            {
                agent.isStopped = true; 
                agent.velocity = Vector3.zero;

                // Smoothly rotate to face the leader
                Vector3 direction = (customLeader.transform.position - transform.position).normalized;
                direction.y = 0; 
                
                if (direction != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
                }
            }
            return; 
        }

        if (isBusy)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        // independent guest walking logic (approach the node and hang around it)
        if (node != null)
        {
            float distToDest = Vector3.Distance(transform.position, destination);

            if (loopTmp || distToDest < 1.5f) 
            {

                if (!loopTmp)
                {
                    NodeConnect script = node.GetComponent<NodeConnect>();
                    if (script != null) node = script.getForward();
                }

                loopTmp = false; 
                Vector3 randomOffset = new Vector3(Random.Range(-2.0f, 2.0f), 0, Random.Range(-2.0f, 2.0f));
                destination = node.position + randomOffset;
                
                agent.SetDestination(destination);
            }
        }
    }

    public void SetupNavigation(bool isMutating = false)
    {
        if (rings != null && rings.Length > 0) return; 

        if (pathsContainer == null)
        {
            if (AIManager.AIInstance != null)
            {
                pathsContainer = AIManager.AIInstance.globalPathsContainer;
            }
            
            if (pathsContainer == null)
            {
                Debug.LogError(name + ": AIManager is missing the 'globalPathsContainer' reference!");
                return; 
            }
        }

        int count = pathsContainer.childCount;
        rings = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            rings[i] = pathsContainer.GetChild(i);
        }

        if (rings.Length > 0)
        {
            ring = rings[0];
            
            if (!isMutating)
            {
                if (ring.childCount > 0)
                {
                    int randomStart = Random.Range(0, ring.childCount);
                    node = ring.GetChild(randomStart);
                    agent.Warp(node.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)));
                    loopTmp = true; 
                }
            }
            else
            {
                loopTmp = true;
            }
        }
    }

    // have the group try to form a circle 
    public Vector3 GetSocialPosition()
    {
        if (customLeader == null) return transform.position;

        
        float circleRadius = 2.5f; 
        

        float angleStep = 360f / (groupTotalSize + 1); 
        float myAngle = angleStep * (groupIdx + 1); 

        Quaternion rotation = Quaternion.Euler(0, myAngle, 0);
        Vector3 offset = rotation * Vector3.forward * circleRadius;

        return customLeader.transform.position + offset;
    }

    public Vector3 getBehind()
    {
        // get angle for destination
        float angle = Vector3.Angle(destination, this.transform.position);

        Vector3 newPos = this.transform.position;

        // calculate behind
        newPos = new Vector3(newPos.x + (2*Mathf.Sin(angle)), 0, newPos.z + (2*Mathf.Cos(angle)));

        return newPos;
    }

    public void NodeMove(int ringNum)
    {
        // switch rings if guest isn't socializing or they're not a monster 
        if (isBusy || isInfected) return; 

        if (rings != null && ringNum < rings.Length)
        {
            ring = rings[ringNum]; 
            if (ring.childCount > 0)
            {
                // switch to a random node 
                int randomNode = Random.Range(0, ring.childCount);
                node = ring.GetChild(randomNode); 
                loopTmp = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currVictim == null || currVictim.isInfected) FindNewVictim(); 

        // chase the victim 
        if (currVictim != null)
        {
            agent.SetDestination(currVictim.transform.position); 

            // infect the guest 
            if (Vector3.Distance(transform.position, currVictim.transform.position) < 1.5f)
            {
                currVictim.Infect(this); 
                currVictim = null;      // setup for next victim 
            }
        }
    }

    private void FindNewVictim()
    {
        Pathfinding[] allAgents = FindObjectsByType<Pathfinding>(FindObjectsSortMode.None);
        List<Pathfinding> potentialVictims = new List<Pathfinding>(); 

        foreach (var a in allAgents)
        {
            if (!a.isInfected) potentialVictims.Add(a); 
        }

        if (potentialVictims.Count > 0) 
        {
            currVictim = potentialVictims[Random.Range(0, potentialVictims.Count)];
        }
    }

    public virtual void Infect(Pathfinding attacker = null)
    {
        if (AIManager.AIInstance != null) AIManager.AIInstance.HandleInfection(this, attacker); 
        else Debug.LogError("Missing AI manager"); 
    }

    // TODO: for refactor make logic for promotion in the same file 
    public void ApplyStandardInfection()
    {
        isInfected = true; 
        isBusy = true; 
        // TODO: this is just a visual to see who is a monster. We need to implement a 
        // way to switch over the models where the monster can actually transform

        if (myRenderer != null) myRenderer.material = monsterMaterial;
        else  Debug.LogError(name + ": Missing Renderer or Monster Material");
        Debug.Log(name + " has been infected");
        // cleanup the old guest behaivor
        customLeader = null; 
        follower = false; 
    }
    
    public bool GetPlayerInside()
    {
        return playerInside;
    }
}
