using UnityEngine;

public class PhotoScoring : MonoBehaviour
{
    [Header("Scoring Curves")]
    [SerializeField] private AnimationCurve distCurve;
    [SerializeField] private AnimationCurve faceCurve;
    [SerializeField] private AnimationCurve sizeCurve;
    [SerializeField] private AnimationCurve autoFocusCurve;
    [SerializeField] private AnimationCurve manualFocusCurve;
    [SerializeField] private AnimationCurve developCurve;
    
    [Header("Score Weights")]
    [SerializeField] private float distWeight = .15f;
    [SerializeField] private float faceWeight = .20f;
    [SerializeField] private float sizeWeight = .25f;
    [SerializeField] private float focusWeight = .20f;
    [SerializeField] private float developWeight = .20f;

    private ScoreParameters currScore;


    public void EvaluateCaptureData(CaptureData data)
    {
        currScore = new ScoreParameters();

        float dist = Vector3.Distance(data.subjectPos, data.playerPos);
        currScore.distance = ScoreDistance(dist);
        
        float angle = CalculateFacing(data);
        currScore.facing = ScoreFacing(angle);

        float size = CalclateScreenSize(data);
        currScore.size = ScoreSize(size);
        
        if (data.manualFocus)
        {
            currScore.focus = ScoreManualFocus(data.focus);
        } 
        else
        {
           currScore.focus = ScoreAutoFocus(data.focus);
        }

        currScore.extras = data.extras;
        
    }

    public void EvaluatePostData(float developPercent)
    {
        currScore.development = developCurve.Evaluate(developPercent);
        Debug.Log("develop: " + developPercent + " | Score: " + currScore.development);
    }

    public ScoreParameters CalculatePhotoScore()
    {
        float weightedDistance = currScore.distance * distWeight;
        float weightedFacing   = currScore.facing * faceWeight;
        float weightedSize     = currScore.size * sizeWeight;
        float weightedFocus    = currScore.focus * focusWeight;
        float weightedDevelop  = currScore.development * developWeight;
        
        float baseScore = weightedDistance + weightedFacing + weightedSize + weightedFocus + weightedDevelop;

        float extrasMultiplier = 1f + (currScore.extras * 0.1f);

        currScore.result = baseScore * extrasMultiplier;
        

        return currScore;
    }

    /*
        Helper Functions
    */
    private float ScoreDistance(float dist)
    {
        return distCurve.Evaluate(dist);
    }

    private float ScoreFacing(float angle)
    {
        return faceCurve.Evaluate(angle);
    }

    private float ScoreSize(float size)
    {
        return sizeCurve.Evaluate(size);
    }

    private float ScoreAutoFocus(float focus)
    {
        return autoFocusCurve.Evaluate(focus);
    }

    private float ScoreManualFocus(float focus)
    {
        return manualFocusCurve.Evaluate(focus);
    }

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

    private float CalclateScreenSize(CaptureData data)
    {
        Camera cam = data.camera;
        Collider col = data.subject.collider;

        Vector3 min = col.bounds.min;
        Vector3 max = col.bounds.max;

        Vector3 screenMin = cam.WorldToScreenPoint(min);
        Vector3 screenMax = cam.WorldToScreenPoint(max);

        float xMin = Mathf.Min(screenMin.x, screenMax.x);
        float yMin = Mathf.Min(screenMin.y, screenMax.y);

        float xMax = Mathf.Max(screenMin.x, screenMax.x);
        float yMax = Mathf.Max(screenMin.y, screenMax.y);

    
        xMin = Mathf.Clamp(xMin, 0, Screen.width);
        yMin = Mathf.Clamp(yMin, 0, Screen.height);
        xMax = Mathf.Clamp(xMax, 0, Screen.width);
        yMax = Mathf.Clamp(yMax, 0, Screen.height);


        float screenWidth = Mathf.Max(0, xMax - xMin);
        float screenHeight = Mathf.Max(0, yMax - yMin);

        float area = screenWidth * screenHeight;
        float normalizedArea = area / (Screen.width * Screen.height);

        return normalizedArea;
    }
}
