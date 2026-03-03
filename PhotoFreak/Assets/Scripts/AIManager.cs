using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform Ais;
    private int tmp = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Ais != null)
        {
            Pathfinding[] NPCs = Ais.transform.GetChild(tmp).GetComponentsInChildren<Pathfinding>();
            for (int i = 0; i < NPCs.Length ; i++)
            {
                if (NPCs[i] != null)
                {
                    NPCs[i].Run();
                }
            }
        }
    }
}
