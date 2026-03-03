using UnityEngine;

public class NodeConnect : MonoBehaviour
{
    private Transform forwardNode;
    private Transform previousNode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int index = this.transform.GetSiblingIndex();
        int numNodes = this.transform.parent.transform.childCount;

        forwardNode = this.transform.parent.GetChild((index+1+numNodes)%numNodes);
        previousNode = this.transform.parent.GetChild((index-1+numNodes)%numNodes);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Transform getForward()
    {
        return forwardNode;
    }
}
