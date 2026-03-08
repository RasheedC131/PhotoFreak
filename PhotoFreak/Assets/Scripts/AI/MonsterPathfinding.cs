using UnityEngine;
using System.Collections; 
// TODO: still need to scale tell chance exponetially when we have our ingame clock setup 

public class MonsterPathfinding : Pathfinding
{

    [Header("Monster Behavior")]
    [SerializeField] private float minTimeBetweenTells = 5f; 
    [SerializeField] private float maxTimeBetweenTells = 15f; 
    [Range(0f, 1f)]
    [SerializeField] private float tellTriggerProbability = 0.7f; 

    private float nextTellTime = 0f; 
    private bool isGlitching = false; 
    private float defaultAngularSpeed; 

    protected override void Start()
    {
        isInfected = true; 
        base.Start(); 
        defaultAngularSpeed = agent.angularSpeed; 
        SetNextTellTime();
    }

    public override void Run()
    {
        // see if monster is able to do its tell 
        if (!isGlitching && Time.time >= nextTellTime) 
        {    
            if (Random.value <= tellTriggerProbability) StartCoroutine(TriggerTell());
            else SetNextTellTime(); 
        }

        // TODO: tweak or change this later based on in-game clock 
        // Snap to the player to reveal itself as a tell 
        if (!isGlitching)
        {
            // personalSpaceDist = 2.0f; 
            // agent.angularSpeed = defaultAngularSpeed; 
            base.Run(); 
        }
        else
        {
            personalSpaceDist = 2.0f; 
            agent.angularSpeed = defaultAngularSpeed; 
            base.Run(); 
        }
    }

    private void SetNextTellTime()
    {
        nextTellTime = Time.time + Random.Range(minTimeBetweenTells, maxTimeBetweenTells);
    }

    private IEnumerator TriggerTell()
    {
        isGlitching = true;
        
        int tellType = Random.Range(0, 3);

        switch (tellType)
        {
            // come close 
            case 0: 
                personalSpaceDist = 0.5f; 
                break;
            // snap quickly
            case 1: 
                agent.angularSpeed = 10000f; 
                break;
            // 
            case 2: 
                agent.isStopped = true;
                yield return new WaitForSeconds(0.4f); 
                agent.isStopped = false;
                break;
        }

        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        isGlitching = false;
        SetNextTellTime();
    }
}
