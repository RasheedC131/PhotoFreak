using UnityEngine;
using System.Collections.Generic;

public class ZoneNode : MonoBehaviour
{
    private GuestSettings gs; 
    public int activeCapacity { get; private set; } 

    public List<AIContext> currentCrowd = new List<AIContext>();
    public List<AIContext> incomingCrowd = new List<AIContext>();

    void Awake()
    {
        gs = GuestSettings.Instance; 
        activeCapacity = Random.Range(2, gs.wanderNodeMaxCapacity + 1);
    }

    public bool HasOpenSlots()
    {
        return (currentCrowd.Count + incomingCrowd.Count) < activeCapacity; 
    }
}