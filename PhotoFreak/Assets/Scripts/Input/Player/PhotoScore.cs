using UnityEngine;

public class PhotoScore : MonoBehaviour
{
    [Header("SphereCast")]
    public float radius;
    public float maxDistance;
    public LayerMask layer;

    RaycastHit subject; //what the SphereCast hit

    [Header("Scoring Curves")]
    public AnimationCurve distanceCurve;
    public AnimationCurve facingCurve;

    struct ScoreParameters
    {
        //Integers representing rating 1-5
        public int distance; //How far is the subject
        public int facing; //Is the subject facing the camera
        public int framing; // How much of the photo does the subject take up
        public int pose; //Taken from Photo Tag

        public const int numParameters = 4; //To easily update amount of parameters
    };



    public void CaptureSubject()
    {
        ScoreParameters photo = new ScoreParameters();

        if(Physics.SphereCast(transform.position,radius,transform.forward,out subject,maxDistance, layer))
        {
           PhotoTag tag = subject.collider.GetComponent<PhotoTag>();

            //For when player takes a picture of a wall or any obstruction
           if (!tag)
            {
                EmptyPhoto(photo);
                return;
            }

            photo.distance = DistanceCalculation(subject.point);
            photo.facing = FacingCalculation(subject);
            photo.pose = tag.poseScore;



        } else
        {
            EmptyPhoto(photo);
        }

        DisplayResult(photo);
    }

    private int DistanceCalculation(Vector3 subjectPos)
    {
        float distance = Vector3.Distance(transform.position, subjectPos);

        //Debug.Log("Distance: " + distance);
        //Debug.Log("Distance: " + (int)distanceCurve.Evaluate(distance));

        return (int)distanceCurve.Evaluate(distance);
    }

    private int FacingCalculation(RaycastHit subject)
    {
        //Angle from subject pov
        Vector3 fromSubject = subject.collider.transform.forward;
        Vector3 toPlayer = transform.position - subject.point;
        
        //Takes horizontals out of the calculation
        fromSubject.y = 0;
        toPlayer.y = 0;

        //Convert to unit vectors to stay in bounds
        fromSubject.Normalize();
        toPlayer.Normalize();
        
        float angle = Vector3.Dot(fromSubject,toPlayer); // 1 in front, 0 next to, -1 behind

        //Debug.Log("Angle: " + angle);
        //Debug.Log("Angle: " + (int)facingCurve.Evaluate(angle));

        return (int)facingCurve.Evaluate(angle);
    }

    private void EmptyPhoto(ScoreParameters photo)
    {
        photo.distance = 0;
        photo.facing = 0;
        photo.framing = 0;
        photo.pose = 0;
    }

    private void DisplayResult(ScoreParameters photo)
    {
        int result = (photo.distance + photo.facing + photo.framing + photo.pose)/ScoreParameters.numParameters;
        Debug.Log("Score: " + result);
    }

    //Debug to see SphereCast
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.forward*maxDistance, radius);
        Debug.DrawRay(transform.position, transform.forward * maxDistance, Color.green);
    }

}
