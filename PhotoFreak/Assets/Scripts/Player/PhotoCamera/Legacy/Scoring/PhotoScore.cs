using UnityEngine;

public class PhotoScore : MonoBehaviour

{
    [SerializeField] private Transform cameraTransform;
    [Header("SphereCast")]
    public float radius = 0.5f;
    public float maxDistance = 100f;
    public LayerMask layer;


    private RaycastHit subject; //what the SphereCast hit

    [Header("Scoring Curves")]
    public AnimationCurve distanceCurve;
    public AnimationCurve facingCurve;

    struct ScoreParameters
    {
        //Integers representing rating 1-5
        public int distance; //How far is the subject
        public int facing; //Is the subject facing the camera
        //public int framing; // How much of the photo does the subject take up
        public int pose; //Taken from Photo Tag
        public int focus; //Taken from CameraFocus

        public const int numParameters = 5; //To easily update amount of parameters
    };

    //Other scripts
    private CameraFocus cameraFocus;
    private PhotoCamera photoCamera;

    public int currentScore;

    void Start()
    {
        cameraFocus = GetComponent<CameraFocus>();
        photoCamera = GetComponent<PhotoCamera>();
    }



    public GameObject CaptureSubject()
    {
        ScoreParameters photo = new ScoreParameters();
        GameObject hitObject = null; 

        Vector3 origin = cameraTransform ? cameraTransform.position : transform.position;
        Vector3 direction = cameraTransform ? cameraTransform.forward : transform.forward;

        if(Physics.SphereCast(origin,radius,direction,out subject,maxDistance))
        {
            Debug.Log("Hit");
            hitObject = subject.collider.gameObject;
        
            //For when player takes a picture of a wall or any obstruction
            if (!subject.collider.GetComponent<PhotoTag>())
            {
                EmptyPhoto(ref photo);
                Debug.Log("No Tag");
            }
            else
            {
                PhotoTag tag = subject.collider.GetComponent<PhotoTag>();
                photo.distance = DistanceCalculation(subject.collider.transform.position);
                photo.facing = FacingCalculation(subject);
                photo.pose = tag.poseScore;
                photo.focus = FocusCalculation();
            }

        } else
        {
            EmptyPhoto(ref photo);
            Debug.Log("Missed");

        }


        currentScore = CalculateResult(photo);
        return hitObject; 
    }

    private int DistanceCalculation(Vector3 subjectPos)
    {
        Vector3 origin = cameraTransform ? cameraTransform.position : transform.position;
        float distance = Vector3.Distance(origin, subjectPos);

        //Debug.Log("Distance: " + distance);
        //Debug.Log("Distance: " + (int)distanceCurve.Evaluate(distance));

        return (int)distanceCurve.Evaluate(distance);
    }

    private int FacingCalculation(RaycastHit subject)
    {
        //Angle from subject pov
        Vector3 fromSubject = subject.collider.transform.forward;
        Vector3 origin = cameraTransform ? cameraTransform.position : transform.position;
        Vector3 toPlayer = origin - subject.collider.transform.position;
        
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

    private int FocusCalculation()
    {
        float accuracy = cameraFocus.GetFocusScore();
        return Mathf.RoundToInt(accuracy * 5);
    }

    private void EmptyPhoto(ref ScoreParameters photo)
    {
        photo.distance = 1;
        photo.facing = 1;
        //photo.framing = 0;
        photo.pose = 1;
        photo.focus = 1;
        Debug.Log("oof");
    }

    
    private int CalculateResult(ScoreParameters photo)
    {
        int result = (photo.distance + photo.facing + photo.pose + photo.focus)/(ScoreParameters.numParameters-1); //Version without framing until implemented
        //int result = (photo.distance + photo.facing + photo.framing + photo.focus + photo.pose)/ScoreParameters.numParameters;

        Debug.Log("Distance: " + photo.distance);
        Debug.Log("Facing: " + photo.facing);
        Debug.Log("Pose: " + photo.pose);
        Debug.Log("Focus: " + photo.focus);
        Debug.Log("Total Score: " + result);

        return result;
    }

    //Debug to see SphereCast
    void OnDrawGizmos()
    {
        Vector3 origin = cameraTransform ? cameraTransform.position : transform.position;
        Vector3 direction = cameraTransform ? cameraTransform.forward : transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(origin + direction*maxDistance, radius);
        Debug.DrawRay(origin, direction*maxDistance, Color.green);
    }

}
