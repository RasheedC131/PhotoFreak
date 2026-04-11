using UnityEngine;

public class CaptureSystem : MonoBehaviour
{
    /*
        This script takes the photo and saves and sends the raw data
    */

    [SerializeField] private Transform cameraTransform;

    [Header("SphereCast")]
    public float radius = 0.5f;
    public float maxDistance = 100f;
    public LayerMask layer;
    private RaycastHit subject; //what the SphereCast hit




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
      
    }


    public bool CaptureSubject()
    {
        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;

        if(Physics.SphereCast(origin,radius,direction,out subject,maxDistance, layer))
        {
            Debug.Log("Hit");
        
            //Checks for photo tag
            if (subject.collider.GetComponent<PhotoTag>())
            {
                Debug.Log("Hit Subject");

                //Call for photo calculation would include cameraTransform and subject as parameters

                return true;
            }

        } else
        {
            Debug.Log("Missed");
        }

        return false;
    }
   
    //Debug to see SphereCast
    void OnDrawGizmos()
    {
        if (cameraTransform == null) return;

        Vector3 origin = cameraTransform.position;
        Vector3 direction = cameraTransform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(origin + direction*maxDistance, radius);
        Debug.DrawRay(origin, direction*maxDistance, Color.green);
    }
    
}
