using UnityEngine;
using UnityEngine.AI;

public class Pathfinding : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public bool loopTmp;
    private Transform ring;
    private Transform node;
    private Transform[] rings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Transform parent = this.transform.parent;
        Transform paths = parent.Find("Paths");

        int count = paths.transform.childCount;
        rings = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            rings[i] = paths.GetChild(i);
        }

        ring = rings[0];
        node = ring.GetChild(0);
    }

    void Update()
    {
        if (loopTmp)
        {
            loopTmp = false;
            Vector3 pos = node.position;
            pos = new Vector3(pos.x, 0, pos.z);
            agent.SetDestination(pos);
        }

        if (this.transform.position.x == node.position.x && this.transform.position.z == node.position.z)
        {
            NodeConnect script = node.GetComponent<NodeConnect>();
            node = script.getForward();
            Debug.Log(node.name);
            loopTmp = true;
        }

    }

    public void MouseMove()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Vector3 pos = hitInfo.point;
            pos = new Vector3(pos.x, 0, pos.z);
            agent.SetDestination(pos);
            Debug.Log("hello world");
        }
    }

    public void NodeMove(int ringNum)
    {
        ring = rings[ringNum];
        node = ring.GetChild(0);
        Debug.Log("Switching");
        Debug.Log(ring.name);
        loopTmp = true;
    }
}
