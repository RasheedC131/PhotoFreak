using UnityEngine;

public class Development : MonoBehaviour
{
    private bool isDeveloping = false;
    private float currDevelop = 0f;
    private float maxDevelop = 10f;
    private float minDevelop = 0f;
    private float growthRate = 1f;
    private float decayRate = 0.3f;

    //Other Scripts
    private CameraController controller;
    private PlayerUIManager ui;

    void Awake()
    {
        controller = GetComponent<CameraController>();
        ui = GetComponentInParent<Transform>().parent.GetComponentInChildren<PlayerUIManager>();
    }

    void Update()
    {
        if (!controller.HasPendingPhoto()) return;

        if (isDeveloping)
        {
            currDevelop += growthRate * Time.deltaTime;
            currDevelop = Mathf.Min(currDevelop, maxDevelop);

        }
        else
        {
            if (currDevelop > minDevelop)
            {
                currDevelop -= decayRate * Time.deltaTime;
                currDevelop = Mathf.Max(currDevelop, minDevelop);
            }
        }
        ui.UpdateDevelopmentBar(GetDevelopPercent());
    }

    public void ToggleDevelopment(bool toggle)
    {
        isDeveloping = toggle;
    }

    public bool IsDevelopComplete()
    {
        return currDevelop >= maxDevelop;
    }

    public void ResetDevelopment()
    {
        currDevelop = minDevelop;
        ui.UpdateDevelopmentBar(GetDevelopPercent());
    }

    public float GetDevelopPercent()
    {
        return currDevelop/maxDevelop;
    }
}
