using UnityEngine;
using System.Collections.Generic; 
public class AIManager : MonoBehaviour
{
    [SerializeField] private Timer timer; 
    public Transform Ais;
    private int NPCS = 0;
    private int LOOP = 0;

    [Header("Grouping Settings")]
    public float groupFormRate = 5.0f;      // period of time that npcs try to form a group 
    public float groupTimer = 0f; 

    [SerializeField] private int maxGroupSize = 5; 
    [SerializeField] private float timeToBreakGroup = 10f; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    void Update()
    {
        if (Ais == null) return; 
        Pathfinding[] allAgents = Ais.GetComponentsInChildren<Pathfinding>();

        if (timer != null && !(timer.getTime() > 0))
        {
            foreach(var a in allAgents)
            {
                // only move the leader npcs if they're not talking to other guests
                if (!a.follower && !a.isBusy && !a.isInfected) a.NodeMove(++LOOP % 2); 
            }
        }

        foreach(var a in allAgents)
        {
            if (a != null) a.Run(); 
        }

        groupTimer += Time.deltaTime; 
        if (groupTimer > groupFormRate)
        {
            FormRandomGroup(allAgents); 
            groupTimer = 0; 
        }
    }

    void FormRandomGroup(Pathfinding[] agents)
    {
        // list of ungrouped uninfected npcs 
        List<Pathfinding> available = new List<Pathfinding>(); 

        foreach(var a in agents)
        {
            if (!a.isInfected && !a.isBusy && !a.follower) available.Add(a); 
        }

        // group formation logic 
        if (available.Count < 2) return; 

        // leader assignment 
        Pathfinding leader = available[Random.Range(0, available.Count)]; 
        available.Remove(leader); 
        leader.isBusy = true;       // avoid switching rings 

        // follower assignment 
        int groupSize = Random.Range(1, maxGroupSize); 
        int actualSize = Mathf.Min(groupSize, available.Count); 
        
        List<Pathfinding> currGroup = new List<Pathfinding>(); 
        currGroup.Add(leader); 

        for (int i = 0; i < actualSize; i ++)
        {
            Pathfinding follower = available[Random.Range(0, available.Count)]; 
            available.Remove(follower); 

            follower.customLeader = leader; 
            follower.isBusy = true; 
            currGroup.Add(follower); 
        }

        StartCoroutine(DisbandGroup(currGroup, 10f)); 
    }

    System.Collections.IEnumerator DisbandGroup(List<Pathfinding> group, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach(var member in group)
        {
            if (member != null)
            {
                member.isBusy = false;
                member.customLeader = null;
            }
        }
    }   
}
