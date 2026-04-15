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

    //OtherScript
    private PhotoScoring eval;

    void Awake()
    {
        cameraTransform = camera.transform;

        eval = GetComponent<PhotoScoring>();
    }

    //Shoots a raycast straight forward, looking for a subject or bumping into a wall
    public bool CaptureSubject()
    {
        Vector3 origin = cameraTransform.position;
        Vector3 dir = cameraTransform.forward;

        if(Physics.Raycast(origin,dir,out subject,maxDistance, subjectLayer | obstructionLayer))
        {
            //Checks for photo tag
            if (subject.collider.GetComponent<PhotoTag>())
            {
                Debug.Log("Hit Subject");

                CaptureData data = new CaptureData();

                data.playerPos = origin;
                data.subjectPos = subject.point;
                data.subjectForward = subject.collider.transform.forward;

                data.extras = CaptureExtras(subject);

                
                eval.EvaluateCaptureData(data);

                return true;
            }

        } 
        
        Debug.Log("Missed");
        

        return false;
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
        
        Debug.Log("Extras: " + extrasCount);
        return extrasCount;
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