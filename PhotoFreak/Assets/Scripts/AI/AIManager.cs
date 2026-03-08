using UnityEngine;
using System.Collections.Generic; 
public class AIManager : MonoBehaviour
{
    [SerializeField] private Timer timer; 
    public Transform Ais;
    private int NPCS = 0;
    private int LOOP = 0;

    // TODO: change these once we have a ingame timer setup
    [Header("Timers")]
    private float movementTimer = 0f; 
    public float groupFormRate = 5.0f;      // period of time that npcs try to form a group 
    public float groupTimer = 0f; 

    [Header("Grouping Settings")]
    [SerializeField] private int minGroupSize = 2; 
    [SerializeField] private int maxGroupSize = 4; 
    [SerializeField] private float timeToBreakGroup = 15f;      // change this in actual demo  

    void Update()
    {
if (Ais == null) return;

        // ring switching logic 
        movementTimer -= Time.deltaTime;
        if (movementTimer <= 0)
        {
            movementTimer = 8f; 
            Pathfinding[] allAgents = Ais.GetComponentsInChildren<Pathfinding>();
            LOOP++; 

            foreach (var a in allAgents)
            {
                // move npcs if they're not 'socializing' in a group 
                if (a != null && !a.follower && !a.isBusy && !a.isInfected)
                {
                    a.NodeMove(LOOP % 2);
                }
            }
        }

        Pathfinding[] activeAgents = Ais.GetComponentsInChildren<Pathfinding>();
        foreach (var a in activeAgents) if (a != null) a.Run();

        groupTimer += Time.deltaTime;
        if (groupTimer > groupFormRate)
        {
            FormPartyGroup(activeAgents);
            groupTimer = 0;
        }
    }

    void FormPartyGroup(Pathfinding[] agents)
        {
            // get available guest (not busy, a monster, or in a group)
            List<Pathfinding> available = new List<Pathfinding>();
            foreach (var a in agents)
            {
                if (!a.isInfected && !a.isBusy && !a.follower) available.Add(a);
            }

            if (available.Count < 2) return; 

            // assign the leader guest that other guest will follow 
            Pathfinding leader = available[Random.Range(0, available.Count)];
            available.Remove(leader);
            leader.isBusy = true; 

            // choose a group size 
            int desiredSize = Random.Range(minGroupSize, maxGroupSize + 1);
            int actualFollowers = Mathf.Min(desiredSize - 1, available.Count);

            List<Pathfinding> currGroup = new List<Pathfinding>();
            currGroup.Add(leader);

            // assign npcs to the leader and form a circle 
            for (int i = 0; i < actualFollowers; i++)
            {
                Pathfinding follower = available[Random.Range(0, available.Count)];
                available.Remove(follower);

                follower.customLeader = leader;
                follower.isBusy = true;
                
                follower.groupIdx = i; 
                follower.groupTotalSize = actualFollowers; 

                currGroup.Add(follower);
            }

            Debug.Log($"Formed a group of {currGroup.Count} around {leader.name}");
            StartCoroutine(DisbandGroup(currGroup, timeToBreakGroup));
        }

        System.Collections.IEnumerator DisbandGroup(List<Pathfinding> group, float delay)
        {
            yield return new WaitForSeconds(delay);

            foreach (var member in group)
            {
                if (member != null)
                {
                    member.isBusy = false;
                    member.customLeader = null;
                }
            }
        }
        
}


