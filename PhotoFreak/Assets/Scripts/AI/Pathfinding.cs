using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] protected Camera cam;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected bool loopTmp;
    [SerializeField] protected Transform pathsContainer;


    [Header("Current NPC Status")]
    public bool isInfected = false;  // if the guest is turned into a monster 
    public bool isBusy = false;     // use for the AIManager
    public Pathfinding customLeader; // used for a dynamic defaultLeader
    public Pathfinding currVictim;   // who is being chased by monster 

    [SerializeField] private Renderer myRenderer; 

    public bool follower;
    protected Transform ring;
    protected Transform node;
    protected Transform[] rings;
    protected Transform defaultLeader; // renamed it to avoid confusion with the new logic defaultLeader vars 
    protected Vector3 destination;
    protected float personalSpaceDist = 2.0f; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        if (pathsContainer == null)
        {
            GameObject obj = GameObject.Find("Pahts"); 
            if (obj != null)
            {
                pathsContainer = obj.transform; 
            }

            else
            {
                Debug.LogError("No 'Paths' object was assigned or it couldn't be found");
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
            if (ring.childCount > 0)
            {
                node = ring.GetChild(0);
            }
        }

        if (!follower)
        {
            this.transform.SetAsFirstSibling();
        }

        if (this.transform.parent != null && this.transform.parent.childCount > 0) 
        {
            defaultLeader = this.transform.parent.GetChild(0); // 0 for first sibling
        }
        else 
        { 
            defaultLeader = null;
        }

        if (isInfected) Infect(); 
    }

    public virtual void Run()
    {
        // monster logic 
        if (isInfected)
        {
            StartHunting(); 
            return; 
        }

        // group logic (socializing with guest)
        if (customLeader != null)
        {
            Vector3 pos = customLeader.getBehind(); 
            agent.SetDestination(pos); 
            return; 
        }

        // if follower follow defaultLeader
        if (follower && defaultLeader != null)
        {
            Vector3 pos = defaultLeader.GetComponent<Pathfinding>().getBehind();
            agent.SetDestination(pos); 
        }

        // if defaultLeader/independent from a group walk the nodes 
        else
        {
            // walk the node path if it's an independent guest 
            if (node == null)
            {
                Debug.LogWarning(name + ": 'node' is NULL!");
                return;
            }

            if (loopTmp || agent.remainingDistance < 0.5f)
            {
                loopTmp = false;
                destination = node.position;
                agent.SetDestination(destination);
                
                Debug.DrawLine(transform.position, destination, Color.green);
            }
        }
    }

    public Vector3 getBehind()
    {
        // get angle for destination
        float angle = 0; 
        

        if (destination == Vector3.zero) angle = transform.eulerAngles.y;  // if still use the forward vec 
        else angle = Vector3.Angle(destination, this.transform.position); 

        float radians = angle * Mathf.Deg2Rad;
        Vector3 newPos = this.transform.position;
        // calculate behind
        newPos = new Vector3(newPos.x + (personalSpaceDist * Mathf.Sin(radians)), 0, newPos.z + (personalSpaceDist * Mathf.Cos(radians)));

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
                node = ring.GetChild(0); 
                loopTmp = true; 
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // added safety checks 
        if (node != null && other.transform == node)
        {
            NodeConnect script = node.GetComponent<NodeConnect>();
            if (script != null)
            {
                node = script.getForward();
                loopTmp = true;
            }
        }
    }

    protected void StartHunting()
    {
        if (currVictim == null || currVictim.isInfected) FindNewVictim(); 

        // chase the victim 
        if (currVictim != null)
        {
            agent.SetDestination(currVictim.transform.position); 

            // infect the guest 
            if (Vector3.Distance(transform.position, currVictim.transform.position) < 1.5f)
            {
                currVictim.Infect(); 
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

    public void Infect()
    {
        isInfected = true; 
        isBusy = true; 
        // TODO: this is just a visual to see who is a monster. We need to implement a 
        // way to switch over the models where the monster can actually transform

        if (myRenderer != null) myRenderer.material.color = Color.red; 

        // cleanup the old guest behaivor
        customLeader = null; 
        follower = false; 
    }
}
