using UnityEngine;

public class AIManager : MonoBehaviour
{
    [SerializeField] private Timer timer; 
    [SerializeField] private Transform Ais;
    private int NPCS = 0;
    private int LOOP = 0;
    Pathfinding[] NPCs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       NPCs = Ais.transform.GetChild(NPCS).GetComponentsInChildren<Pathfinding>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Ais != null)
        {
            for (int i = 0; i < NPCs.Length ; i++)
            {
                if (NPCs[i] != null)
                {
                    if (!NPCs[i].follower)
                    {
                        if (!(timer.getTime() > 0))
                        {
                            NPCs[i].NodeMove(++LOOP%2);
                        }
                    }
                    NPCs[i].Run();
                }
            }
        }
    }
}
