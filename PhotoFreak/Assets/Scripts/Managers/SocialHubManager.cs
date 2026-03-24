using UnityEngine;
using System.Collections; 
using System.Collections.Generic; 

// Spawn nodes around the map where the npcs will eventually travel to depending on what they're doing 
public class SocialHubManager : MonoBehaviour
{
    public static SocialHubManager Instance; 

    [Header("Level References")]
    public Transform globalNodesContainer; 
    public GameObject socialHubPrefab; 

    [Header("Grouping Settings")]
    [SerializeField] private float groupFormRate = 5.0f; 
    [SerializeField] private int minGroupSize = 2; 
    [SerializeField] private int maxGroupSize = 4; 
    [SerializeField] private float minTimeToBreakUp = 15f; 
    [SerializeField] private float maxTimeToBreakUp = 30f; 

    public List<SocialHub> activeHubs = new List<SocialHub>(); 

    void Awake()
    {
        if (Instance is null) Instance = this; 
        else Destroy(gameObject); 
    }

    void Start()
    {
        if (globalNodesContainer is not null && socialHubPrefab is not null)
        {
            StartCoroutine(NewGroupRoutine()); 
            return; 
        }
        
        Debug.LogWarning("SocialHubManager is missing reference(s)"); 
    }

    private IEnumerator NewGroupRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(groupFormRate); 
            CreateSocialHub(); 
        }
    }

    private void CreateSocialHub()
    {
        int zoneCount = globalNodesContainer.childCount; 
        if (zoneCount == 0) return; 

        Transform randomZone = globalNodesContainer.GetChild(Random.Range(0, zoneCount)); 

        GameObject newHubObj = Instantiate(socialHubPrefab, randomNode.position, Quaternion.identity); 
        SocialHub newHub = newHubObj.GetComponent<SocialHub>(); 

        if (newHub is not null)
        {
            int randomCapacity = Random.Range(minGroupSize, maxGroupSize + 1);
            float randomTimeToBreakUp = Random.Range(minTimeToBreakUp, maxTimeToBreakUp); 

            newHub.Initialize(randomCapacity, randomTimeToBreakUp);    
            activeHubs.Add(newHub);        
        }

    }
}
