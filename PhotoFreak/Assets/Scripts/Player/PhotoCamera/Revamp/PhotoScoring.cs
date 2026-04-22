using UnityEngine;

public class PhotoScoring : MonoBehaviour
{
    [Header("Scoring Curves")]
    public AnimationCurve distanceCurve;
    public AnimationCurve facingCurve;

    struct ScoreParameters
    {
        public float distance; //How far is the subject
        public float facing; //Is the subject facing the camera
        public float size; //Does the subject fit in the camera

        //public int pose; //Taken from Photo Tag
        public float focus; //Taken from either Manual or Auto Focus

        //public const int numParameters = 5; //To easily update amount of parameters
    };

    public void EvaluateCaptureData(CaptureData data)
    {
        ScoreParameters score = new ScoreParameters();

        float dist = Vector3.Distance(data.subjectPos, data.playerPos);
        score.distance = ScoreDistance(dist);
        //Debug.Log("Distance : " + dist);
        //Debug.Log("Distance Score: " + score.distance);

        float angle = CalculateFacing(data);
        score.facing = ScoreFacing(angle);
        //Debug.Log("Angle : " + angle);
        //Debug.Log("Facing Score: " + score.facing);

        float size = 1f/(dist * data.fov);
        //score.size = ScoreSize(size);
        Debug.Log("Size : " + size);

        score.focus = data.focus;
        Debug.Log("Focus : " + size);
    }

    private float ScoreDistance(float dist)
    {
        return distanceCurve.Evaluate(dist);
    }

    private float ScoreFacing(float angle)
    {
        return facingCurve.Evaluate(angle);
    }

    //Need to implement
    //private float TotalScore();

    private float CalculateFacing(CaptureData data)
    {
        Vector3 fromSubject = data.subjectForward;
        Vector3 toPlayer = data.playerPos - data.subjectPos;

        //Remove horizontal component from vectors
        fromSubject.y = 0;
        toPlayer.y = 0;

        //Convert to Unit Vectors
        fromSubject.Normalize();
        toPlayer.Normalize();
        
        //Use dot product to calculate orthogonality
        float angle = Vector3.Dot(fromSubject,toPlayer);

        return angle;
    }
}
