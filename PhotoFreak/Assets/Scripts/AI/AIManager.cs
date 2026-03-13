using UnityEngine;
using System.Collections.Generic; 
using UnityEngine;

// controls whether infected AI can have the basic AI or the monster AI 

public class AIManager : MonoBehaviour
{
    public enum InfectionMode
    {
        ONLY_STANDARD, 
        ONLY_MONSTER, 
        RANDOM
    } 

    [Header("References")]
    public static AIManager AIInstance; // since our manager should be a singleton have one static reference 
    public Transform globalPathsContainer; 
    public Transform Ais;
    
    [Header("Grouping Settings")]
    [SerializeField] private int minGroupSize = 2; 
    [SerializeField] private int maxGroupSize = 4;             
    [SerializeField] private float timeToBreakGroup = 15f;      // When the groups break up from their circles 
    public float groupFormRate = 5.0f;                          // period of time that npcs try to form a group 

    [Header("Infection Monster Settings")]
    public InfectionMode infectionMode = InfectionMode.ONLY_MONSTER;
    [SerializeField] private GameObject defaultMonsterModelPrefab;
    [SerializeField] private GameObject killMonsterModelPrefab; 
    public float smartAIChance = 50f;       // only works if set to random for infectionMode 
 
    private float movementTimer = 0f; 
    private float groupTimer = 0f; 
    private int LOOP = 0;

    void Awake()
    {
        AIInstance = this; 
    }

    // Update is called once per frame
    void Update()
    {
        if (Ais != null)
        {
            movementTimer = 8f; 
            Pathfinding[] allAgents = Ais.GetComponentsInChildren<Pathfinding>();
            LOOP++; 

            foreach (var a in allAgents)
            {
                // move npcs if they're not 'socializing' in a group 
                if (a != null && !a.follower && !a.isBusy && !a.isInfected) a.NodeMove(LOOP % 2);
            }
        }

        // path finding 
        Pathfinding[] activeAgents = Ais.GetComponentsInChildren<Pathfinding>();
        foreach (var a in activeAgents) if (a != null) a.Run();

        // group formation 
        groupTimer += Time.deltaTime;
        if (groupTimer > groupFormRate)
        {
            FormPartyGroup(activeAgents);
            groupTimer = 0;
        }
    }

    public void HandleInfection (Pathfinding victim, Pathfinding attacker)
    {
        // if victim is already infected don't do anything 
        if (victim is MonsterPathfinding)
        {
            victim.ApplyStandardInfection(); 
            return; 
        }

        bool attackerIsSmart = attacker != null && attacker is MonsterPathfinding;

        if (!attackerIsSmart)
        {
            victim.ApplyStandardInfection(); 
            return; 
        }

        bool makeSmart = false; 

        switch (infectionMode)
        {
            case InfectionMode.ONLY_STANDARD:   makeSmart = false;                                  break; 
            case InfectionMode.ONLY_MONSTER:    makeSmart = true;                                   break; 
            case InfectionMode.RANDOM:          makeSmart = Random.Range(0f, 100f) < smartAIChance; break; 
        }

        if (makeSmart) ApplySmartMonster(victim); 

        // TODO: fix this later 
        else
        { 
            // victim.ApplyStandardInfection(); 
            // victim.gameObject.tag = "Monster";
            // PhotoTag tag = victim.GetComponent<PhotoTag>();
            // if (tag == null) tag = victim.AddComponent<PhotoTag>();
            // tag.type = PhotoTag.SubjectType.Monster;

            // victim.ApplyStandardInfection(); 
        }
    }

    // replaces the references to the old guest to become a "smart" monster 
    private void ApplySmartMonster(Pathfinding oldScript)
    {
        GameObject body = oldScript.gameObject; 
        Transform savedPaths = oldScript.pathsContainer;
        Renderer savedRenderer = oldScript.myRenderer;
        // Material savedMat = oldScript.monsterMaterial;

 foreach (Transform child in body.transform)
        {
           child.gameObject.SetActive(false);
        }

        GameObject newDefaultModel = null;
        GameObject newKillModel = null;

        if (defaultMonsterModelPrefab != null)
        {
            newDefaultModel = Instantiate(defaultMonsterModelPrefab, body.transform);
            newDefaultModel.transform.localPosition = Vector3.zero;
            newDefaultModel.transform.localRotation = Quaternion.identity;
        }

        if (killMonsterModelPrefab != null)
        {
            newKillModel = Instantiate(killMonsterModelPrefab, body.transform);
            newKillModel.transform.localPosition = Vector3.zero;
            newKillModel.transform.localRotation = Quaternion.identity;
            newKillModel.SetActive(false);
        }

        body.tag = "Monster"; 
        PhotoTag tag = body.GetComponent<PhotoTag>();
        if (tag == null) tag = body.AddComponent<PhotoTag>();
        tag.type = PhotoTag.SubjectType.Monster;
        tag.poseScore = 3;          // maybe tweak this 

        Destroy(oldScript);

        MonsterPathfinding newBrain = body.AddComponent<MonsterPathfinding>();

        newBrain.pathsContainer = savedPaths;
        newBrain.myRenderer = savedRenderer;

        // Assign the newly created models to the new script
        newBrain.defaultModel = newDefaultModel; 
        newBrain.killModel = newKillModel;
        // newBrain.monsterMaterial = savedMat;

        newBrain.SetupNavigation(true); 
        newBrain.Infect(); 
        
        Debug.Log(body.name + " mutated now a monster");

    }

    void FormPartyGroup(Pathfinding[] agents)
        {
            // get available guest (not busy, a monster, or in a group)
            List<Pathfinding> available = new List<Pathfinding>();
            foreach (var a in agents)
            {
                if (!a.isInfected && !a.isBusy && !a.follower && !(a is MonsterPathfinding)) available.Add(a);
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
