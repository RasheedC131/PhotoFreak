using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class FreakMeter : MonoBehaviour
{
    public static event System.Action<int> OnStrikeEarned;

    [Header("FreakMeter settings")]
    [SerializeField] private int maxNPC;
    [SerializeField] private float maxFreak;
    [SerializeField] private int maxStrikes;
    [SerializeField] private int decayRate;
    [Header("Camera Freak Function")]
    [SerializeField] private float k1;
    [SerializeField] private float x1;
    [Header("Camera Detection")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float cameraDetectRange = 10f;
    [SerializeField] private float cameraDetectAngle = 35f;
    [Header("Sprint Freak Function")]
    [SerializeField] private float k2;
    [SerializeField] private float x2;
    [Header("Script info")]
    [SerializeField] CameraController CameraScript;
    [SerializeField] PlayerMovement player;
    [SerializeField] private FreakMeterUI UI;
    [SerializeField] private Timer timer;
    [SerializeField] private FreakMeterTimer freakTimer;

    [SerializeField] private float currentFreak = 0f;
    private bool isMeterDecaying;

    [SerializeField] private int count = 0;
    private List<Transform> visibleNPCs = new List<Transform>();

    private float prevVal = 0;
    private int currentStrikes = 0;
    private bool isFinalPanic = false;
    private bool striked = false;

    public float FreakRatio => maxFreak > 0f ? Mathf.Clamp01(currentFreak / maxFreak) : 0f;

    void Start()
    {
        UpdateUI();
        if (UI == null) Debug.LogError("[FreakMeter]: UI reference is missing in the Inspector!");
        if (CameraScript == null) Debug.LogError("[FreakMeter]: CameraScript reference is missing!");
        UI.UpdateStrikes(currentStrikes);
    }

    void Update()
    {
        if (GlobalGameState.Instance != null && GlobalGameState.Instance.currentState != GlobalGameState.GameState.PLAYING) return;

        if (count > maxNPC) count = maxNPC;

        if (currentFreak >= maxFreak)
        {
            if (!striked)
                striked = true;
            freakTimer.restartTime();
            currentStrikes += 1;
            if (currentStrikes >= maxStrikes)
            {
                if (!isFinalPanic)
                {
                    isFinalPanic = true;
                    if (CrowdStateManager.Instance != null) CrowdStateManager.Instance.TriggerFinalPanic();
                }
                return;
            }
            currentFreak = maxFreak - 2;
            if (UI != null) UI.UpdateMeter(currentFreak, maxFreak);
            UI.UpdateStrikes(currentStrikes);

            OnStrikeEarned?.Invoke(currentStrikes);
        }

        if (striked)
        {
            currentFreak -= .01f * 10;
            UI.UpdateMeter(currentFreak, maxFreak);
            if (currentFreak <= 0)
            {
                currentFreak = 0;
                striked = false;
            }
        }

        bool isMeterRising = false;

        if (player.getSprint() && count > -1 && !striked)
        {
            freakTimer.unpause();
            timer.restart();
            currentFreak += sprintFunction(1, freakTimer.getTime()) * 1f;
            isMeterRising = true;
        }
        else if (CameraScript.getCameraState() && !striked)
        {
            int viewCount = CountNPCsInCameraView();
            if (viewCount > 0)
            {
                freakTimer.unpause();
                timer.restart();
                currentFreak += cameraFunction(viewCount, freakTimer.getTime());
                isMeterRising = true;
            }
        }
        else
        {
            freakTimer.pause();
        }

        if (isMeterRising)
        {
            UpdateUI();
        }
        else if (isMeterDecaying)
        {
            freakTimer.pause();
            if (currentFreak > 0)
            {
                currentFreak -= .01f * decayRate;
                if (currentFreak < 0)
                {
                    currentFreak = 0;
                    isMeterDecaying = false;
                }
                freakTimer.restartTime();
                UI.UpdateMeter(currentFreak, maxFreak);
            }
        }
        if (timer.getTime() <= 0)
        {
            isMeterDecaying = true;
        }
        else
        {
            isMeterDecaying = false;
        }
    }

    private int CountNPCsInCameraView()
    {
        if (player == null) return 0;

        Transform aimTransform = cameraTransform != null ? cameraTransform : (Camera.main != null ? Camera.main.transform : null);
        if (aimTransform == null)
        {
            Debug.LogWarning("[FreakMeter] CountNPCsInCameraView: no camera transform found. Assign cameraTransform in the Inspector.");
            return 0;
        }

        Vector3 sphereOrigin = player.transform.position;

        Collider[] hits = Physics.OverlapSphere(sphereOrigin, cameraDetectRange);
        System.Collections.Generic.HashSet<Transform> counted = new System.Collections.Generic.HashSet<Transform>();
        int viewCount = 0;
        foreach (Collider col in hits)
        {
            PhotoTag tag = col.GetComponentInParent<PhotoTag>();
            if (tag == null) continue;

            Transform root = col.transform.root;
            if (!counted.Add(root)) continue;

            Vector3 dirToNPC = (col.transform.position - sphereOrigin).normalized;
            if (Vector3.Angle(aimTransform.forward, dirToNPC) <= cameraDetectAngle)
                viewCount++;
        }
        return viewCount;
    }

    float sprintFunction(int count, float time)
    {
        float val = k2 * Mathf.Pow(x2, time);
        prevVal = val  * count - prevVal;
        if (prevVal < 0)
        {
            prevVal = 0;
        }
        return prevVal;
    }

    float cameraFunction(int count, float time)
    {
        float val = k1 * Mathf.Pow(x1, time);
        prevVal = val * count - prevVal;
        if (prevVal < 0)
        {
            prevVal = 0;
        }
        return prevVal;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!visibleNPCs.Contains(other.transform))
        {
            visibleNPCs.Add(other.transform);
            count = visibleNPCs.Count / 2;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Guest") || other.CompareTag("Monster"))
        {
            if (visibleNPCs.Contains(other.transform))
            {
                visibleNPCs.Remove(other.transform);
                count = visibleNPCs.Count / 2;
                Debug.Log("NPC Left Range, Count: " + count);
            }
        }
    }

    public void AddFreakScore(float amount)
    {
        if (GlobalGameState.Instance != null && GlobalGameState.Instance.currentState != GlobalGameState.GameState.PLAYING) return;

        currentFreak += amount;
        if (currentFreak > maxFreak) currentFreak = maxFreak;
        UpdateUI();
    }

    private void UpdateUI()
    {
        UI.UpdateMeter(currentFreak, maxFreak);
        if(timer != null) timer.restart();
        isMeterDecaying = true;
    }
}
