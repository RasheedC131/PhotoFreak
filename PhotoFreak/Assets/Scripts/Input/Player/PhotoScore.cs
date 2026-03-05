using UnityEngine;

public class PhotoScore : MonoBehaviour
{
    [Header("SphereCast")]
    public float radius;
    public float maxDistance;
    public LayerMask layer;

    RaycastHit subject; //what the SphereCast hit

    //Debug to see SphereCast
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.forward*maxDistance, radius);
        Debug.DrawRay(transform.position, transform.forward * maxDistance, Color.green);
    }

    public void CaptureSubject()
    {
        if(Physics.SphereCast(transform.position,radius,transform.forward,out subject,maxDistance, layer))
        {
           Debug.Log(subject.collider.gameObject);
        } else
        {
            Debug.Log("oof");
        }
    }
}
