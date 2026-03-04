using UnityEngine;
using UnityEngine.AI;

public class Pathfinding : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public bool follower;
    public bool loopTmp;
    private Transform ring;
    private Transform node;
    private Transform[] rings;
    private Transform leader;
    private Vector3 destination;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform parent = this.transform.parent.parent.parent;
        Transform paths = parent.Find("Paths");

        int count = paths.transform.childCount;
        rings = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            rings[i] = paths.GetChild(i);
        }

        ring = rings[0];
        node = ring.GetChild(0);

        if (!follower)
        {
            this.transform.SetAsFirstSibling();
        }
        leader = this.transform.parent.GetChild(0); // 0 for first sibling
    }

    public void Run()
    {
        if (!follower)
        {
            if (loopTmp)
            {
                loopTmp = false;
                Vector3 pos = node.position;
                pos = new Vector3(pos.x, 0, pos.z);
                destination = pos; // for other npcs to use
                agent.SetDestination(pos);
            }
        }
        else
        {
            Vector3 pos = leader.GetComponent<Pathfinding>().getBehind(); // get position of leader
            pos = new Vector3(pos.x, 0, pos.z);
            agent.SetDestination(pos);
            new WaitForSeconds(2);
        }
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
        ring = rings[ringNum];
        // Debug.Log("Switching");
        // Debug.Log(ring.name);
        // Debug.Log(ring.childCount);
        node = ring.GetChild(0); // magic number lol
        loopTmp = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        NodeConnect script = node.GetComponent<NodeConnect>();
        node = script.getForward();
        loopTmp = true;
    }
}
