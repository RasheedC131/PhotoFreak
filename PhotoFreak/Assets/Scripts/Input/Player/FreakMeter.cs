using UnityEngine;
using UnityEditor;
public class FreakMeter : MonoBehaviour
{
    [SerializeField] PhotoCamera CameraScript;
    [SerializeField] PlayerMovement player;
    [SerializeField] private FreakMeterUI UI;
    [SerializeField] private int maxNPC;
    [SerializeField] private int maxFreak;
    [SerializeField] private int maxStrikes;
    private double currentFreak;
    [SerializeField] private Timer timer;
    private bool tmp;
    private int count;
    private Transform[] NPCs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        count = 0;
        NPCs = new Transform[1];
    }

    // Update is called once per frame
    void Update()
    {
        if (count > maxNPC) count = maxNPC;
        if (currentFreak >= maxFreak)
        {
            maxStrikes -= 1;
            currentFreak = 0;
        }
        if (maxStrikes <= 0)
        {
            Destroy(this.transform.gameObject);
        }
        if (player.getSprint())
        {
            if (count >= 0)
                currentFreak += count*(.01);
            Debug.Log(currentFreak);
            UI.UpdateMeter(currentFreak);
            timer.restart();
            tmp = true;
        }
        if (CameraScript.getCameraState())
        {
            currentFreak += count*(.03);
            Debug.Log(currentFreak);
            UI.UpdateMeter(currentFreak);
            timer.restart();
            tmp = true;
        }
        if (timer.getTime() <= 0 && tmp)
        {
            if (currentFreak > 0)
            {
                currentFreak -= (.01);
                if (currentFreak < 0)
                {
                    currentFreak = 0;
                    tmp = false;
                }
                Debug.Log(currentFreak);
                UI.UpdateMeter(currentFreak);
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null)
        {
            if (other.transform.parent.name == "NPCs")
        {
            if (!ArrayUtility.Contains(NPCs, other.transform))
            {
                ArrayUtility.Add(ref NPCs, other.transform);
                count = NPCs.Length - 1;
            }
            Debug.Log("Up: " + count);
        }
        }
        
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.parent != null)
        {
            if (other.transform.parent.name == "NPCs")
        {
            if (ArrayUtility.Contains(NPCs, other.transform))
            {
                ArrayUtility.Remove(ref NPCs, other.transform);
                count = NPCs.Length - 1;
            }
            Debug.Log("Down: " + count);
        }
        }
    }
}
