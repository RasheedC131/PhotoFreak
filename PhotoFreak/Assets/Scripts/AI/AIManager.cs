using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] private Timer timer; 
    public Transform Ais;
    private int NPCS = 0;
    private int LOOP = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Ais != null)
        {
            Pathfinding[] NPCs = Ais.transform.GetChild(NPCS).GetComponentsInChildren<Pathfinding>();
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
