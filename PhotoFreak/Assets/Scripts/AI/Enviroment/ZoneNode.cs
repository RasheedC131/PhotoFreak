using UnityEngine;
using System.Collections.Generic; 

// Permanent nodes that the npcs travel to 
public class ZoneNode : MonoBehaviour
{
    public List<Transform> neighborNodes = new List<Transform>();
    public Transform NextNode { get; private set; }
    public Transform PreviousNode { get; private set; }

    void Awake()
    {
        int idx = transform.GetSiblingIndex();
        int numNodes = transform.parent.childCount;

        NextNode = transform.parent.GetChild((idx + 1) % numNodes); 
        PreviousNode = transform.parent.GetChild((idx - 1 + numNodes) % numNodes); 

        if (neighborNodes.Count == 0) {        
            neighborNodes.Add(NextNode);
            neighborNodes.Add(PreviousNode);
        }
    }

    public Transform GetRandomNeighbor(Transform comingFrom)
    {
        if (neighborNodes.Count <= 1) return neighborNodes[0];

        List<Transform> choices = new List<Transform>(neighborNodes);
        if (choices.Contains(comingFrom))
        {
            choices.Remove(comingFrom);
        }

        int r = Random.Range(0, choices.Count);
        return choices[r];
    }
}
