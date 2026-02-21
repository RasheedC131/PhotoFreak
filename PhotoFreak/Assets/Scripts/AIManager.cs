using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Pathfinding moveScript;
    private int tmp = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (moveScript != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                moveScript.MouseMove();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                moveScript.NodeMove(tmp);
                tmp = (tmp + 1) % 2;
            }
        }
    }
}
