using System.Collections;
using UnityEngine;

public class CaptureSystem : MonoBehaviour
{
    /*
        This script takes the photo and saves and sends the raw data
    */

    [SerializeField] private Camera camera;
    private Transform cameraTransform;

    [Header("RayCast")]
    public float maxDistance = 20f;
    private RaycastHit subject;

    [Header("Overlay Sphere")]
    public float overlayRadius = 7f;

    [Header("Layers")]
    public LayerMask subjectLayer;
    public LayerMask obstructionLayer;

    //Other Scripts
    private PhotoScoring eval;
    private CameraController controller;
    private CameraAutoFocus autoFocus;
    private CameraManualFocus manualFocus;
    [SerializeField] private PlayerUIManager ui;
    [SerializeField] private CameraFlash cameraFlash;

    //Other Variables
    private bool MonsterGet;


    void Awake()
    {
        cameraTransform = camera.transform;

        eval = GetComponent<PhotoScoring>();
        controller = GetComponent<CameraController>();
        autoFocus = GetComponentInChildren<CameraAutoFocus>();
        manualFocus = GetComponentInChildren<CameraManualFocus>();
        if (ui == null) ui = GetComponentInParent<Transform>().root.GetComponentInChildren<PlayerUIManager>();
        if (cameraFlash == null) cameraFlash = GetComponentInChildren<CameraFlash>();
    }

    //Shoots a raycast straight forward, looking for a subject or bumping into a wall
    public bool CaptureSubject()
    {
        Vector3 origin = cameraTransform.position;
        Vector3 dir = cameraTransform.forward;

        if (Physics.Raycast(origin, dir, out subject, maxDistance, subjectLayer | obstructionLayer, QueryTriggerInteraction.Collide))
        {
            Debug.Log($"[CaptureSystem] Raycast hit: '{subject.collider.gameObject.name}' " +
                      $"on layer '{LayerMask.LayerToName(subject.collider.gameObject.layer)}'");

            PhotoTag tag = subject.collider.GetComponentInParent<PhotoTag>();
            Debug.Log($"[CaptureSystem] PhotoTag found: {tag != null}" +
                      (tag != null ? $" (on '{tag.gameObject.name}')" :
                      $" — collider root: '{subject.collider.transform.root.name}'"));

            if (tag != null)
            {
                Debug.Log("[CaptureSystem] Capturing subject.");
                if (tag.gameObject.tag == "monster")
                {
                    MonsterGet = true;
                }
                else
                {
                    MonsterGet = false;
                }
                cameraFlash?.TriggerFlash();
                StartCoroutine(SendCaptureData(origin, subject));
                return true;
            }
        }
        else
        {
            Debug.Log($"[CaptureSystem] Raycast hit nothing. " +
                      $"SubjectLayer mask value: {subjectLayer.value}, " +
                      $"ObstructionLayer mask value: {obstructionLayer.value}");
        }

        Debug.Log("[CaptureSystem] Missed.");
        return false;
    }

    // get if it was a monster or not
    public bool CheckMonster()
    {
        return MonsterGet;
    }

    //Summons an overlap sphere to catch nearby subjects
    private int CaptureExtras(RaycastHit subject)
    {
        int extrasCount = 0;
        Collider[] bgSubjects = Physics.OverlapSphere(subject.point, overlayRadius, subjectLayer);

        //Checks conditions for each extra
        foreach (var extra in bgSubjects)
        {
            if (extra == subject.collider) continue; //skip the original subject if caught in overlay

            Vector3 vp = camera.WorldToViewportPoint(extra.transform.position);
            bool inPlayerView = 
                vp.z>0 && //In front of player
                0<=vp.y && vp.y<=1 && //In vertical view
                0<=vp.x && vp.x<=1; //In horizontal view

            Vector3 dir = (extra.transform.position - cameraTransform.position).normalized;
            float dist = Vector3.Distance(extra.transform.position, cameraTransform.position);
            bool Blocked = Physics.Raycast(cameraTransform.position, dir, dist, obstructionLayer);

            if(inPlayerView && !Blocked) extrasCount += 1;
        }
        
        //Debug.Log("Extras: " + extrasCount);
        return extrasCount;
    }

    private IEnumerator SendCaptureData(Vector3 origin, RaycastHit subject)
    {
        CaptureData data = new CaptureData();

        data.playerPos = origin;
        data.subjectPos = subject.point;
        data.subjectForward = subject.collider.transform.root.forward;
                
        data.camera = camera;
        data.subject = subject;

        data.focus = controller.currentCamera.manualFocus ? manualFocus.GetFocusError() : autoFocus.GetFocus();
        data.manualFocus = controller.currentCamera.manualFocus;

        data.extras = CaptureExtras(subject);


        yield return new WaitForEndOfFrame();
        Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();
        data.currentPhoto = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        data.currentPhoto.SetPixels(screenShot.GetPixels());
        data.currentPhoto.Apply();


        eval.EvaluateCaptureData(data, MonsterGet);
        
    }


    void OnDrawGizmos()
    {
        if (camera == null) return;

        Transform cam = camera.transform;
        Vector3 origin = cam.position;
        Vector3 dir = cam.forward;

        // --- SphereCast direction ---
        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, dir * maxDistance);

        // --- Overlap sphere (debug area) ---
        if (Application.isPlaying && subject.collider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(subject.point, overlayRadius);
        }
    }
    
}