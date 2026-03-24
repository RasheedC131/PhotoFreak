using UnityEngine;

// Permanent nodes that the npcs travel to 
public class ZoneNode : MonoBehaviour
{
    public Transform NextNode { get; private set; }
    public Transform PreviousNode { get; private set; }

    void Awake()
    {
        int idx = transform.GetSiblingIndex();
        int numNodes = transform.parent.childCount;

        // Calculate the wrap-around indices 
        int nextidx = (idx + 1) % numNodes;
        int previdx = (idx - 1 + numNodes) % numNodes;

        // Assign the actual transform references
        NextNode = transform.parent.GetChild(nextidx);
        PreviousNode = transform.parent.GetChild(previdx);
    }
}
